#nullable enable
using HG;
using MSU;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VAPI.Addressables;

namespace VAPI
{
    /// <summary>
    /// Serializable class that represents a the "Target Character" for a CharacterVariantDef
    /// </summary>
    [Serializable]
    public sealed class VariantCharacterTarget : ICloneable
    {
        [AddressableComponentRequirement(typeof(CharacterBody), searchInChildren = false)]
        public AddressReferencedCharacterBody characterBodyRef = new AddressReferencedCharacterBody();
        [AddressableComponentRequirement(typeof(CharacterMaster), searchInChildren = false)]
        public AddressReferencedCharacterMaster characterMasterRef = new AddressReferencedCharacterMaster();

        public Component? LoadCharacterComponent()
        {
            Component? body = LoadComponent<CharacterBody>(characterBodyRef);
            return body ?? LoadComponent<CharacterMaster>(characterMasterRef);
        }

        private T? LoadComponent<T>(AddressReferencedPrefab prefabReference) where T : Component
        {
            var prefab = prefabReference.LoadAssetNow();
            if(prefab && prefab.TryGetComponent<T>(out var t))
            {
                return t;
            }
            return null;
        }

        public bool AnyAddressReferencedAssetValid()
        {
            bool characterBodyRefHasValidValue = characterBodyRef.AssetExists || !string.IsNullOrWhiteSpace(characterBodyRef.Address);
            bool characterMasterRefHasValidValue = characterMasterRef.AssetExists || !string.IsNullOrWhiteSpace(characterMasterRef.Address);
            return characterBodyRefHasValidValue || characterMasterRefHasValidValue;
        }

        public object Clone()
        {
            VariantCharacterTarget clone = new VariantCharacterTarget();
            clone.characterBodyRef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedCharacterBody, GameObject>(this.characterBodyRef);
            clone.characterMasterRef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedCharacterMaster, GameObject>(this.characterMasterRef);
            return clone;
        }

        #region Constructors
        public VariantCharacterTarget(GameObject directObjectReference, bool isForBody)
        {
            if(isForBody)
            {
                characterBodyRef.Asset = directObjectReference;
            }
            else
            {
                characterMasterRef.Asset = directObjectReference;
            }
        }
        public VariantCharacterTarget(string keyOrCatalogEntry, bool isForBody)
        {
            if(isForBody)
            {
                characterBodyRef.Address = keyOrCatalogEntry;
            }
            else
            {
                characterMasterRef.Address = keyOrCatalogEntry;
            }
        }
        public VariantCharacterTarget() { }
        #endregion
    }
}