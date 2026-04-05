#nullable enable
using HG;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace VAPI
{
    //Stores the CharacterVariantDefs for this master, the CharacterBodyVariantController will attempt to link itself to this, if the link is successful, then this is the single source of truth for which variants to use
    public class CharacterMasterVariantStorage : NetworkBehaviour
    {
        public CharacterMaster characterMaster { get; private set; }
        public bool doNotRollForVariants
        {
            get => _doNotRollForVariants;
            [Server]
            set
            {
                if(_doNotRollForVariants != value)
                {
                    _doNotRollForVariants = value;
                }
            }
        }
        [SyncVar]
        private bool _doNotRollForVariants;

        public ReadOnlyArray<CharacterVariantDef> characterVariantDefs => _characterVariants;
        private CharacterVariantDef[] _characterVariants = Array.Empty<CharacterVariantDef>();

        private SyncListCharacterVariantIndex _characterVariantIndicesSync = new SyncListCharacterVariantIndex();

        private void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _characterVariantIndicesSync.Callback = SyncListCallback;
        }

        //Callback is only called if the server is a client. So i _think_ a dedicated server will never get this callback, which isnt good.
        //Because of that, we will use this callback as a "Client only" callback.
        //From HLAPI source code:
        //
        //if (m_Behaviour.isServer && m_Behaviour.isClient && m_Callback != null)
        //{
        //  m_Callback.Invoke(op, itemIndex);
        //}
        private void SyncListCallback(SyncList<NetworkCharacterVariantIndex>.Operation op, int itemIndex)
        {
            //GTFO if server.
            if (NetworkServer.active)
                return;

            OnSyncListDirty();
        }

        private void OnSyncListDirty()
        {
            //First, unapply the master modifications.
            UnapplyMasterModifications(characterVariantDefs);

            //Second, create new array and populate
            _characterVariants = new CharacterVariantDef[_characterVariantIndicesSync.Count];
            for(int i = 0; i < _characterVariantIndicesSync.Count; i++)
            {
                _characterVariants[i] = CharacterVariantManager.GetCharacterVariantDef(_characterVariantIndicesSync[i])!;
            }

            //Thirdy, apply master modifications
            ApplyMasterModifications(characterVariantDefs);
        }

        [Server]
        public void SetVariantDefsForCharacter(CharacterVariantDef[] characterVariantDefs)
        {
            //First, clear the syncList;
            _characterVariantIndicesSync.Clear();
            //Then, add the indices to the sync list.
            for(int i = 0; i < characterVariantDefs.Length; i++)
            {
                _characterVariantIndicesSync.Add(characterVariantDefs[i].characterVariantIndex);
            }

            //Call the OnSyncListDirty(), which will properly update our variants.
            OnSyncListDirty();
        }

        private VariantComponentStorage? _componentStorage;
        private List<IDisposable?> _disposableModifications = new List<IDisposable?>();
        private void UnapplyMasterModifications(ReadOnlyArray<CharacterVariantDef> variantDefs)
        {
            for (int i = variantDefs.Length - 1; i >= 0; i--)
            {
                //Apply channeled items && equipment info
                variantDefs[i].inventoryDefinition.UnapplyToInventory(characterMaster.inventory);
            }

            //Undo modifiers
            for(int i = _disposableModifications.Count - 1; i >= 0; i--)
            {
                _disposableModifications[i]?.Dispose();
            }
            _disposableModifications.Clear();

            _componentStorage?.Dispose();
            _componentStorage = null;
        }

        private void ApplyMasterModifications(ReadOnlyArray<CharacterVariantDef> variantDefs)
        {
            for (int i = 0; i < variantDefs.Length; i++)
            {
                //Apply channeled items && equipment info
                variantDefs[i].inventoryDefinition.ApplyToInventory(characterMaster.inventory);

                //Apply modifiers
                for (int j = 0; j < variantDefs[i].masterModifiers.Length; j++)
                {
                    IVariantMasterModifier? modifier = variantDefs[i].masterModifiers[j];
                    if (modifier == null)
                        continue;

                    _disposableModifications.Add(modifier.ModifyMaster(characterMaster));
                }
            }

            _componentStorage = new VariantComponentStorage(characterMaster!, variantDefs);
        }
    }
}