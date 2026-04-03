#nullable enable
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
    public class VariantCharacterTarget
    {
        public enum TargetType
        {
            CharacterBody,
            CharacterMaster
        }

        public TargetType targetType;
        public string key = "";
        public bool keyIsFromCatalog = false;

        public Component? LoadCharacterComponent()
        {
            switch (targetType)
            {
                case TargetType.CharacterBody:
                    return LoadBodyComponent();
                case TargetType.CharacterMaster:
                    return LoadMasterComponent();
            }

            return null;
        }

        private Component? LoadBodyComponent()
        {
            if(keyIsFromCatalog)
            {
                BodyIndex bodyIndex = BodyCatalog.FindBodyIndexCaseInsensitive(key);
                if (bodyIndex == BodyIndex.None)
                    return null;

                return BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
            }
            else
            {
                var prefab = Addressables.LoadAssetAsync<GameObject>(key).WaitForCompletion();
                if (!prefab || !prefab.TryGetComponent<CharacterBody>(out var characterBody))
                    return null;

                return characterBody;
            }
        }
        private Component? LoadMasterComponent()
        {
            if (keyIsFromCatalog)
            {
                MasterCatalog.MasterIndex masterIndex = MasterCatalog.FindMasterIndex(key);
                if (masterIndex == MasterCatalog.MasterIndex.none)
                    return null;

                return MasterCatalog.GetMasterPrefab(masterIndex).GetComponent<CharacterMaster>();
            }
            else
            {
                var prefab = Addressables.LoadAssetAsync<GameObject>(key).WaitForCompletion();
                if (!prefab || !prefab.TryGetComponent<CharacterMaster>(out var characterMaster))
                    return null;

                return characterMaster;
            }
        }
    }
}