#nullable enable
using HG;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    //This class is added dynamically by the CharacterBodyVariantController if the body has a master. it's added to CharacterMasters.
    //
    public sealed class CharacterMasterVariantStorage : MonoBehaviour
    {
        private struct StorageMetadata
        {
            //Invalid metadata is body index set to none
            public bool validBodyIndex => bodyIndex != BodyIndex.None;
            public int variantCount => storedVariants?.Length ?? -1;

            public BodyIndex bodyIndex;
            public CharacterBodyVariantController? controller;
            public CharacterVariantDef[]? storedVariants;

            public void UnsubscribeFromController(CharacterMasterVariantStorage storage)
            {                
                //If the controller is not null, unsubscribe.
                //The controller sets it's events to null if it gets destroyed, so no worries with that
                if (controller != null)
                {
                    controller.onVariantsAppliedBody -= storage.ApplyMasterModifications;
                    controller.onVariantsUnappliedBody -= storage.UnapplyMasterModifications;
                }
            }

            public void SubscribeToController(CharacterMasterVariantStorage storage)
            {
                if(controller != null)
                {
                    controller.onVariantsAppliedBody += storage.ApplyMasterModifications;
                    controller.onVariantsUnappliedBody += storage.UnapplyMasterModifications;
                }
            }
        }

        //A variant count of 0 _technically_ is a valid storage, as it means it didnt roll any variants.
        public bool hasStoredVariants => _storageMetadata.variantCount >= 0;
        public CharacterVariantDef[]? storedVariants => _storageMetadata.storedVariants;
        private StorageMetadata _storageMetadata;

        public CharacterBody? masterBody => characterMaster.GetBody();
        public CharacterMaster characterMaster { get; private set; }

        /// <summary>
        /// Event fired when variants are unapplied.
        /// </summary>
        public event Action<ReadOnlyArray<CharacterVariantDef>>? onCharacterMasterVariantStorageUnapply;
        /// <summary>
        /// Event fired when variants are applied
        /// </summary>
        public event Action<ReadOnlyArray<CharacterVariantDef>>? onCharacterMasterVariantStorageApply;

        //This gets called on start, the VariantController will try to obtain this component and assign itself.
        internal void TryAssignController(CharacterBodyVariantController characterBodyVariantController)
        {
            //If null is passed, return
            if (!characterBodyVariantController)
                return;

            //If there's no body in the controller, return.
            var bodyComponent = characterBodyVariantController.characterBody;
            if (!bodyComponent)
                return;

            //If the body component is NOT the master's body, reject it.
            //Havent confirmed this, but i _think_ when you get your soul kicked out during anima harvester's boss, your survivor's body _still thinks_ it's master exists, however, the player controlled master's body is only the soul body.
            if (bodyComponent != masterBody)
                return;

            //Initialize the new metadata
            var potentialNewMetadata = new StorageMetadata
            {
                bodyIndex = bodyComponent.bodyIndex,
                controller = characterBodyVariantController
            };

            //Take care of different scenarios:

            //If the index in the metadata matches the incoming body index, and the metadata has variants, we want to transfer them.
            //This takes care of a body "Respawning" from an extra life.
            if(_storageMetadata.bodyIndex == bodyComponent.bodyIndex && _storageMetadata.variantCount >= 0)
            {
                potentialNewMetadata.storedVariants = _storageMetadata.storedVariants;
                SetStorageMetadata(potentialNewMetadata);
            }

            //The body index does not match, we need to reset the metadata to the new incoming one
            SetStorageMetadata(potentialNewMetadata);
        }

        private void SetStorageMetadata(StorageMetadata newMetadata)
        {
            //The metadata is not valid, so just assign as is.
            if(!_storageMetadata.validBodyIndex)
            {
                _storageMetadata = newMetadata;
                _storageMetadata.SubscribeToController(this);
            }
            else //The current metadata is valid
            {
                _storageMetadata.UnsubscribeFromController(this);
                _storageMetadata = newMetadata;
                _storageMetadata.SubscribeToController(this);
            }
        }

        private DisposableCollectionHelper _disposableCollectionHelper = new DisposableCollectionHelper(disposeInReverseOrder: true);
        private void ApplyMasterModifications(CharacterVariantDef[] variantDefs)
        {
            for (int i = 0; i < variantDefs.Length; i++)
            {
                CharacterVariantDef variantDef = variantDefs[i];

                //Apply Tier data.
                if (variantDef.variantTier)
                {
                    //Modify master returns if not server
                    _disposableCollectionHelper.AddDisposable(variantDef.variantTier!.ModifyMaster(characterMaster));
                }

                //Apply channeled items && equipment info
                //Inventory definition returns if not server
                _disposableCollectionHelper.AddDisposable(variantDef.inventoryDefinition.ApplyToInventory(characterMaster.inventory));

                //Apply modifiers
                for (int j = 0; j < variantDefs[i].masterModifiers.Length; j++)
                {
                    //Base API modifiers return if not server
                    IVariantMasterModifier? modifier = variantDefs[i].masterModifiers[j];
                    _disposableCollectionHelper.AddDisposable(variantDef.masterModifiers[j]?.ModifyMaster(characterMaster));
                }

                //Apply components, the collection only adds components depending on the network context.
                _disposableCollectionHelper.AddDisposable(variantDef.additionalVariantComponents.ApplyComponents(characterMaster));
            }

            _storageMetadata.storedVariants = variantDefs;
        }

        private void UnapplyMasterModifications()
        {
            //Dispose Tier Data, modifiers, item inventory, equipment and components.
            _disposableCollectionHelper.Dispose();
        }

        private void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
        }
    }
    /*
    //Stores the CharacterVariantDefs for this master, the CharacterBodyVariantController will attempt to link itself to this, if the link is successful, then this is the single source of truth for which variants to use
    /// <summary>
    /// A <see cref="NetworkBehaviour"/> used to store and apply variants at a Master level.
    /// <para></para>
    /// When a CharacterBody spawns, if it has a <see cref="CharacterBodyVariantController"/>, it'll try to sync itself to it's variant storage. If this succeeds it'll utilize the <see cref="CharacterVariantDef"/>s stored in the storage.
    /// <br></br>
    /// This way variants persist across stages. Particularly useful for devoted lemurians which are added to DontDestoryOnLoad
    /// </summary>
    public sealed class CharacterMasterVariantStorage : NetworkBehaviour
    {
        
        /// <summary>
        /// Event fired when variants are unapplied.
        /// </summary>
        public event Action<ReadOnlyArray<CharacterVariantDef>>? onCharacterMasterVariantStorageUnapply;
        /// <summary>
        /// Event fired when variants are applied
        /// </summary>
        public event Action<ReadOnlyArray<CharacterVariantDef>>? onCharacterMasterVariantStorageApply;

        /// <summary>
        /// The <see cref="CharacterVariantDef"/>s stored in this <see cref="CharacterMasterVariantStorage"/>
        /// </summary>
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
            _characterVariantIndicesSync.Callback = SyncListCallback; //apply callback
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
            onCharacterMasterVariantStorageUnapply?.Invoke(characterVariantDefs);

            //Second, create new array and populate
            _characterVariants = new CharacterVariantDef[_characterVariantIndicesSync.Count];
            for(int i = 0; i < _characterVariantIndicesSync.Count; i++)
            {
                _characterVariants[i] = CharacterVariantCatalog.GetCharacterVariantDef(_characterVariantIndicesSync[i])!;
            }

            //Thirdy, apply master modifications
            ApplyMasterModifications(characterVariantDefs);
            onCharacterMasterVariantStorageApply?.Invoke(characterVariantDefs);
        }

        /// <summary>
        /// Server method, sets the variants for this character.
        /// </summary>
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

        private DisposableCollectionHelper _disposableCollectionHelper = new DisposableCollectionHelper(disposeInReverseOrder: true);
        private void ApplyMasterModifications(ReadOnlyArray<CharacterVariantDef> variantDefs)
        {
            for (int i = 0; i < variantDefs.Length; i++)
            {
                CharacterVariantDef variantDef = variantDefs[i];

                //Apply Tier data.
                if (variantDef.variantTier)
                {
                    //Modify master returns if not server
                    _disposableCollectionHelper.AddDisposable(variantDef.variantTier!.ModifyMaster(characterMaster));
                }

                //Apply channeled items && equipment info
                //Inventory definition returns if not server
                _disposableCollectionHelper.AddDisposable(variantDef.inventoryDefinition.ApplyToInventory(characterMaster.inventory));

                //Apply modifiers
                for (int j = 0; j < variantDefs[i].masterModifiers.Length; j++)
                {
                    //Base API modifiers return if not server
                    IVariantMasterModifier? modifier = variantDefs[i].masterModifiers[j];
                    _disposableCollectionHelper.AddDisposable(variantDef.masterModifiers[j]?.ModifyMaster(characterMaster));
                }

                //Apply components, the collection only adds components depending on the network context.
                _disposableCollectionHelper.AddDisposable(variantDef.additionalVariantComponents.ApplyComponents(characterMaster));
            }
        }
        
        private void UnapplyMasterModifications(ReadOnlyArray<CharacterVariantDef> variantDefs)
        {
            //Dispose Tier Data, modifiers, item inventory, equipment and components.
            _disposableCollectionHelper.Dispose();
        }

        /// <summary>
        /// Server method, clears the VariantDefs stored in this <see cref="CharacterMasterVariantStorage"/>
        /// </summary>
        [Server]
        public void ClearVariantDefsForCharacterServer()
        {
            _characterVariantIndicesSync.Clear();
            OnSyncListDirty();
        }
    }*/
}