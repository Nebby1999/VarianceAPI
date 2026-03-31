#nullable enable
using R2API.AddressReferencedAssets;
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
    }
}