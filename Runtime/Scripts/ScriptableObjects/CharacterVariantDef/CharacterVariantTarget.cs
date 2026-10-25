#nullable enable
using HG;
using MSU;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VAPI.AddressReferencedAssets;

namespace VAPI
{
    /// <summary>
    /// Serializable class that represents a the "Target Character" for a CharacterVariantDef
    /// </summary>
    [Serializable]
    public sealed class CharacterVariantTarget : ICloneable
    {
        [AddressableComponentRequirement(typeof(CharacterBody))]
        public AddressReferencedBody targetBody = new AddressReferencedBody();

        public bool TryLoadCharacterComponent(out CharacterBody? characterComponent)
        {
            characterComponent = LoadCharacterComponent();
            return characterComponent;
        }

        public CharacterBody? LoadCharacterComponent()
        {
            if (targetBody.IsInvalid)
                return null;

            var bodyPrefab = targetBody.LoadAssetNow();
            if(!bodyPrefab)
            {
                return null;
            }

            if(bodyPrefab.TryGetComponent<CharacterBody>(out var body))
            {
                return body;
            }
            return null;
        }

        public bool IsKeyValid()
        {
            return !targetBody.IsInvalid;
        }

        public object Clone()
        {
            return new CharacterVariantTarget(VAPIUtils.CloneAddressReferencedAsset<AddressReferencedBody, GameObject>(targetBody));
        }

        #region Constructors
        public CharacterVariantTarget(AddressReferencedBody _targetBody)
        {
            targetBody = _targetBody;
        }

        public CharacterVariantTarget(string _bodyName) : this(new AddressReferencedBody(_bodyName)) { }

        public CharacterVariantTarget(GameObject _bodyPrefab) : this(new AddressReferencedBody(_bodyPrefab)) { }

        public CharacterVariantTarget() { }
        #endregion
    }
}