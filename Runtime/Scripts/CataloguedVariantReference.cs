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
        private CharacterVariantDef? _characterVariantDef;
        [SerializeField]
        private string? _cataloguedVariantName;

        private bool _triedToRetrieveFromCatalog;

        public CharacterVariantDef? GetCharacterVariantDef()
        {
            if (_triedToRetrieveFromCatalog == false && _characterVariantDef == null)
            {
                _triedToRetrieveFromCatalog = true;
                _characterVariantDef = CharacterVariantCatalog.GetCharacterVariantDef(CharacterVariantCatalog.FindCharacterVariantIndex(_cataloguedVariantName));
            }
            return _characterVariantDef;
        }

        public bool IsValid()
        {
            return _characterVariantDef != null || !string.IsNullOrWhiteSpace(_cataloguedVariantName);
        }

        public CataloguedVariantReference(CharacterVariantDef directReference)
        {
            _characterVariantDef = directReference;
            _cataloguedVariantName = "";
            _triedToRetrieveFromCatalog = false;
        }

        public CataloguedVariantReference(string catalogReference)
        {
            _cataloguedVariantName = catalogReference;
            _characterVariantDef = null;
            _triedToRetrieveFromCatalog = false;
        }
    }
}