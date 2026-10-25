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
    public sealed class CharacterMasterVariantStorage : MonoBehaviour
    {
        private struct StorageMetadataEntry
        {
            public BodyIndex relatedBodyIndex;
            public CharacterVariantDef[] storedVariants;
        }

        public CharacterMaster characterMaster { get; private set; }

        public event Action? onVariantsUnapplied;
        public event Action? onVariantsApplied;

        private List<StorageMetadataEntry> _storageMetadataList = new List<StorageMetadataEntry>();
        private BodyIndex _bodyIndexOfAppliedVariants = BodyIndex.None;

        private void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
            characterMaster.onBodyStart += OnBodyStart;
        }

        private void OnBodyStart(CharacterBody obj)
        {
            if(obj.bodyIndex != _bodyIndexOfAppliedVariants)
            {
                UnapplyVariants();
            }
        }

        public bool TryGetVariantsStoredForCharacter(in BodyIndex bodyIndex, out CharacterVariantDef[]? variantDefs)
        {
            variantDefs = null;

            if (bodyIndex == BodyIndex.None)
                return false;

            int variantStorageListIndex = GetVariantStorageIndex(bodyIndex);
            if (variantStorageListIndex == -1)
                return false;

            variantDefs = _storageMetadataList[variantStorageListIndex].storedVariants;
            return true;
        }

        public bool HasVariantsForBody(BodyIndex bodyIndex) => GetVariantStorageIndex(bodyIndex) != -1;

        private bool PushVariantsForCharacter(in BodyIndex bodyIndex, in CharacterVariantDef[] variantDefs)
        {
            if(bodyIndex == BodyIndex.None || variantDefs == null)
            {
                return false;
            }

            //If we already have a storage index for this body, return false.
            int variantStorageListIndex = GetVariantStorageIndex(bodyIndex);
            if (variantStorageListIndex != -1)
                return false;

            //Add to storage list and return true;
            _storageMetadataList.Add(new StorageMetadataEntry { relatedBodyIndex = bodyIndex, storedVariants = variantDefs });
            return true;
        }

        private int GetVariantStorageIndex(BodyIndex bodyIndex)
        {
            for (int i = 0; i < _storageMetadataList.Count; i++)
            {
                if (_storageMetadataList[i].relatedBodyIndex == bodyIndex)
                    return i;
            }

            return -1;
        }

        private DisposableCollectionHelper _disposableCollectionHelper = new DisposableCollectionHelper(disposeInReverseOrder: true);

        internal void ApplyVariants(BodyIndex bodyIndex, CharacterVariantDef[] fallbackCharacterVariantDefs)
        {
            //If the body index of applied variants is the one that's incoming, return.
            //We definetly don't want to apply the master modifications twice.
            if (_bodyIndexOfAppliedVariants == bodyIndex)
                return;

            //If the storage doesnt have the variants for this body, push it
            if(!HasVariantsForBody(bodyIndex))
            {
                PushVariantsForCharacter(bodyIndex, fallbackCharacterVariantDefs);
            }

            for (int i = 0; i < fallbackCharacterVariantDefs.Length; i++)
            {
                CharacterVariantDef variantDef = fallbackCharacterVariantDefs[i];

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
                for (int j = 0; j < fallbackCharacterVariantDefs[i].masterModifiers.Length; j++)
                {
                    //Base API modifiers return if not server
                    IVariantMasterModifier? modifier = fallbackCharacterVariantDefs[i].masterModifiers[j];
                    _disposableCollectionHelper.AddDisposable(variantDef.masterModifiers[j]?.ModifyMaster(characterMaster));
                }

                //Apply components, the collection only adds components depending on the network context.
                _disposableCollectionHelper.AddDisposable(variantDef.additionalVariantComponents.ApplyComponents(characterMaster));
            }

            _bodyIndexOfAppliedVariants = bodyIndex;

            onVariantsApplied?.Invoke();
        }

        internal void UnapplyVariants()
        {
            //Dispose Tier Data, modifiers, item inventory, equipment and components.
            _disposableCollectionHelper.Dispose();

            //Set the body index of applied variants to none
            _bodyIndexOfAppliedVariants = BodyIndex.None;

            onVariantsUnapplied?.Invoke();
        }
    }
}