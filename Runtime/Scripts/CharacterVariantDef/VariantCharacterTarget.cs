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
        public VariantCharacterTarget(string key, bool isBody)
        {
            if(isBody)
            {
                characterBodyRef.Address = key;
            }
            else
            {
                characterMasterRef.Address = key;
            }
        }

        public VariantCharacterTarget(Either<AddressReferencedCharacterMaster, AddressReferencedCharacterBody> masterOrBody)
        {
            if(masterOrBody.isA)
            {
                characterMasterRef.Asset = masterOrBody.a;
            }
            else if(masterOrBody.isB)
            {
                characterBodyRef = masterOrBody.b;
            }
        }

        public VariantCharacterTarget(GameObject prefab, bool isBody)
        {
            if(isBody)
            {
                characterBodyRef.Asset = prefab;
            }
            else
            {
                characterMasterRef.Asset = prefab;
            }
        }
        public VariantCharacterTarget() { }
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

        public object Clone()
        {
            VariantCharacterTarget clone = new VariantCharacterTarget();
            clone.characterBodyRef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedCharacterBody, GameObject>(this.characterBodyRef);
            clone.characterMasterRef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedCharacterMaster, GameObject>(this.characterMasterRef);
            return clone;
        }
    }
}