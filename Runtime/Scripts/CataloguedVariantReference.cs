#nullable enable
using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// Provides a method to reference an existing <see cref="CharacterVariantDef"/> either via it's cachedName or via a direct reference.
    /// </summary>
    [Serializable]
    public struct CataloguedVariantReference
    {
        [SerializeField]
        [Tooltip("The direct reference to a CharacterVariantDef")]
        private CharacterVariantDef? _characterVariantDef;
        [SerializeField]
        [Tooltip("The name of a variant def, which will be retrieved via the catalog.")]
        private string? _cataloguedVariantName;

        private bool _triedToRetrieveFromCatalog;

        /// <summary>
        /// Retrieves the CharacterVariantDef referenced by this.
        /// <br></br>
        /// Returns null if the underlying metadata resolved to an invalid variant.
        /// </summary>
        public CharacterVariantDef? GetCharacterVariantDef()
        {
            if (_triedToRetrieveFromCatalog == false && _characterVariantDef == null)
            {
                _triedToRetrieveFromCatalog = true;
                _characterVariantDef = CharacterVariantCatalog.GetCharacterVariantDef(CharacterVariantCatalog.FindCharacterVariantIndex(_cataloguedVariantName));
            }
            return _characterVariantDef;
        }

        /// <summary>
        /// Returns true if this reference is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return _characterVariantDef != null || !string.IsNullOrWhiteSpace(_cataloguedVariantName);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CataloguedVariantReference(CharacterVariantDef directReference)
        {
            _characterVariantDef = directReference;
            _cataloguedVariantName = "";
            _triedToRetrieveFromCatalog = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CataloguedVariantReference(string catalogReference)
        {
            _cataloguedVariantName = catalogReference;
            _characterVariantDef = null;
            _triedToRetrieveFromCatalog = false;
        }
    }
}