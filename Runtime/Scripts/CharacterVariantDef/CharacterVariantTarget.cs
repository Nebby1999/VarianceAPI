#nullable enable
using HG;
using MSU;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VAPI
{
    /// <summary>
    /// Serializable class that represents a the "Target Character" for a CharacterVariantDef
    /// </summary>
    [Serializable]
    public sealed class CharacterVariantTarget : ICloneable
    {
        public enum TargetType
        {
            CharacterBody,
            CharacterMaster
        }

        public string key = "";
        public TargetType type;

        public bool TryLoadCharacterComponent(out Component? characterComponent)
        {
            characterComponent = null;
            return false;

            /*characterComponent = LoadComponent<CharacterBody>(characterBodyRef);
            if (characterComponent)
                return true;

            characterComponent = LoadComponent<CharacterMaster>(characterMasterRef);
            return characterComponent;*/
        }
        public Component? LoadCharacterComponent()
        {
            return null;
            /*Component? body = LoadComponent<CharacterBody>(characterBodyRef);
            return body ?? LoadComponent<CharacterMaster>(characterMasterRef);*/
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
            return false;
            /*bool characterBodyRefHasValidValue = characterBodyRef.AssetExists || !string.IsNullOrWhiteSpace(characterBodyRef.Address);
            bool characterMasterRefHasValidValue = characterMasterRef.AssetExists || !string.IsNullOrWhiteSpace(characterMasterRef.Address);
            return characterBodyRefHasValidValue || characterMasterRefHasValidValue;*/
        }

        public object Clone()
        {
            return null;
            /*CharacterVariantTarget clone = new CharacterVariantTarget();
            clone.characterBodyRef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedCharacterBody, GameObject>(this.characterBodyRef);
            clone.characterMasterRef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedCharacterMaster, GameObject>(this.characterMasterRef);
            return clone;*/
        }

        #region Constructors
        /*public CharacterVariantTarget(GameObject directObjectReference, bool isForBody)
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
        public CharacterVariantTarget(string keyOrCatalogEntry, bool isForBody)
        {
            if(isForBody)
            {
                characterBodyRef.Address = keyOrCatalogEntry;
            }
            else
            {
                characterMasterRef.Address = keyOrCatalogEntry;
            }
        }*/
        public CharacterVariantTarget() { }
        #endregion
    }
}