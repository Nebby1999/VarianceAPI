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
    public class VariantCharacterTarget
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
        public AddressReferencedCharacterBody characterBodyRef = new AddressReferencedCharacterBody();
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
    }
}