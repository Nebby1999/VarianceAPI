using System;
using System.Collections;
using System.Collections.Generic;

namespace VAPI
{
    public enum VariantPackIndex
    {
        None = -1,
    }

    public static class VariantPackManager
    {
        public static int variantPackCount => _registeredPacks.Length;
        private static VariantPack[] _registeredPacks;
        private static Dictionary<string, VariantPackIndex> _identifierToPackIndex = new Dictionary<string, VariantPackIndex>();

        public static ResourceAvailability managerAvailability;

        public static void AddVariantPack(VariantPack variantPack)
        {
            ThrowIfAvailable();
        }

        #region Get and Find Methods
        public static ReadOnlyVariantPack GetVariantPack(VariantPackIndex index)
        {
            if(HG.ArrayUtils.IsInBounds(_registeredPacks, (int)index))
            {
                return new ReadOnlyVariantPack(_registeredPacks[(int)index]);
            }
            return default;
        }

        public static VariantPackIndex FindVariantPackIndex(string variantPackIdentifier)
        {
            return _identifierToPackIndex.GetValueOrDefault(variantPackIdentifier, VariantPackIndex.None);
        }

        public static VariantPackIndex FindVariantPackIndex(CharacterVariantIndex variantIndex) => FindVariantPackIndex(CharacterVariantCatalog.GetCharacterVariantDef(variantIndex));
        public static VariantPackIndex FindVariantPackIndex(CharacterVariantDef variantDef)
        {
            for(int i = 0; i < _registeredPacks.Length; i++)
            {
                if (_registeredPacks[i].characterVariantDefs.Contains(variantDef))
                {
                    return _registeredPacks[i].variantPackIndex;
                }
            }
            return VariantPackIndex.None;
        }

        public static VariantPackIndex FindVariantPackIndex(CharacterVariantTierIndex tierIndex) => FindVariantPackIndex(CharacterVariantTierCatalog.GetCharacterVariantTierDef(tierIndex));
        public static VariantPackIndex FindVariantPackIndex(CharacterVariantTierDef tierDef)
        {
            if (!tierDef)
                return VariantPackIndex.None;

            for(int i = 0; i < _registeredPacks.Length; i++)
            {
                if (_registeredPacks[i].characterVariantTierDefs.Contains(tierDef))
                {
                    return _registeredPacks[i].variantPackIndex;
                }
            }
            return VariantPackIndex.None;
        }
        #endregion

        private static void ThrowIfUnavailable()
        {
            if(!managerAvailability.available)
            {
                throw new InvalidOperationException($"VariantPackManager is not initialized! Consider subscribing to it's managerAvailabilty.");
            }
        }

        private static void ThrowIfAvailable()
        {
            if(managerAvailability.available)
            {
                throw new InvalidOperationException($"Too late! the PackManager is already initialized and this method cannot be called.");
            }
        }
    }
}