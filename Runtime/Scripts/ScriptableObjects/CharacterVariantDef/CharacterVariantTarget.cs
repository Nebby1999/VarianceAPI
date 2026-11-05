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
    /// Serializable class that represents a the "Target Character" for a CharacterVariantDef.
    /// <para></para>
    /// Internally uses a <see cref="AddressReferencedBody"/> to store the body's reference. Which can be either a direct reference, address reference, or a catalog name reference.
    /// </summary>
    [Serializable]
    public sealed class CharacterVariantTarget : ICloneable
    {
        [AddressableComponentRequirement(typeof(CharacterBody))]
        [Tooltip("The target body for this variant.")]
        public AddressReferencedBody targetBody = new AddressReferencedBody();
        private CharacterBody? _cachedBody;

        /// <summary>
        /// Attempts to load the <see cref="CharacterBody"/> component stored in this <see cref="CharacterVariantTarget"/>.
        /// <para></para>
        /// <paramref name="characterComponent"/> is null only if this method returns false.
        /// </summary>
        public bool TryLoadCharacterComponent(out CharacterBody? characterComponent)
        {
            characterComponent = LoadCharacterComponent();
            return characterComponent;
        }

        /// <summary>
        /// Loads the <see cref="CharacterBody"/> component stored in this <see cref="CharacterVariantTarget"/>.
        /// <para></para>
        /// Result may be null if:
        /// <list type="bullet">
        /// <item>The body catalog is not available</item>
        /// <item>The <see cref="targetBody"/> value points to a non-existent body</item>
        /// <item>The <see cref="targetBody"/> value doesnt point to a CharacterBody</item>
        /// </list>
        /// </summary>
        public CharacterBody? LoadCharacterComponent()
        {
            if(BodyCatalog.availability.available == false)
            {
                VAPILog.Error($"Cannot load character component for {targetBody} as the body catalog is not initialized!");
                return null;
            }

            if(_cachedBody)
            {
                return _cachedBody;
            }

            if (targetBody.IsInvalid)
                return null;

            var bodyPrefab = targetBody.LoadAssetNow();
            if(!bodyPrefab)
            {
                return null;
            }

            if(bodyPrefab.TryGetComponent<CharacterBody>(out var body))
            {
                _cachedBody = body;
                return _cachedBody;
            }
            return null;
        }

        /// <summary>
        /// Returns true if this <see cref="CharacterVariantTarget"/> has a valid value.
        /// </summary>
        public bool IsKeyValid()
        {
            return !targetBody.IsInvalid;
        }

        /// <summary>
        /// Clones this <see cref="CharacterVariantTarget"/>
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new CharacterVariantTarget(VAPIUtils.CloneAddressReferencedAsset<AddressReferencedBody, GameObject>(targetBody));
        }

        #region Constructors
        /// <summary>
        /// Constructor. Sets the targetBody for this instance via <paramref name="_targetBody"/>
        /// </summary>
        /// <param name="_targetBody">The target body to use</param>
        public CharacterVariantTarget(AddressReferencedBody _targetBody)
        {
            targetBody = _targetBody;
            _cachedBody = null;
        }

        /// <summary>
        /// Constructor. Encapsulates <paramref name="_addressOrCatalogName"/> inside a <see cref="AddressReferencedBody"/> then calls <see cref="CharacterVariantTarget.CharacterVariantTarget(AddressReferencedBody)"/>
        /// </summary>
        /// <param name="_addressOrCatalogName">The body's name</param>
        public CharacterVariantTarget(string _addressOrCatalogName) : this(new AddressReferencedBody(_addressOrCatalogName)) { }

        /// <summary>
        /// Constructor. Encapsulates <paramref name="_bodyPrefab"/> inside a <see cref="AddressReferencedBody"/> then calls <see cref="CharacterVariantTarget.CharacterVariantTarget(AddressReferencedBody)"/>
        /// </summary>
        /// <param name="_bodyPrefab">The body to use</param>
        public CharacterVariantTarget(GameObject _bodyPrefab) : this(new AddressReferencedBody(_bodyPrefab)) { }

        /// <summary>
        /// Parameterless Constructor
        /// </summary>
        public CharacterVariantTarget() { }
        #endregion
    }
}