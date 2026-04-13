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
            characterComponent = LoadCharacterComponent();
            return characterComponent;
        }

        public Component? LoadCharacterComponent()
        {
            switch(type)
            {
                case TargetType.CharacterBody:
                    {
                        var bodyIndex = BodyCatalog.FindBodyIndex(key);
                        return BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
                    }
                case TargetType.CharacterMaster:
                    {
                        var masterIndex = MasterCatalog.FindMasterIndex(key);
                        GameObject masterPrefab = MasterCatalog.GetMasterPrefab(masterIndex);
                        if(masterPrefab && masterPrefab.TryGetComponent<CharacterMaster>(out var master))
                        {
                            return master;
                        }
                        return null;
                    }
            }
            return null;
        }

        public bool IsKeyValid()
        {
            return !string.IsNullOrWhiteSpace(key);
        }

        public object Clone()
        {
            return new CharacterVariantTarget(key, type);
        }

        #region Constructors
        public CharacterVariantTarget(string catalogName, TargetType targetType)
        {
            key = catalogName;
            type = targetType;
        }
        public CharacterVariantTarget() { }
        #endregion
    }
}