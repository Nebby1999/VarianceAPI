using HG;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VAPI
{
    public enum VariantPackIndex
    {
        None = -1,
    }

    public static class VariantPackManager
    {
        public static ReadOnlyArray<CharacterVariantTierDef> characterVariantTierDefs => _characterVariantTierDefs;
        private static CharacterVariantTierDef[] _characterVariantTierDefs;

        public static ReadOnlyArray<CharacterVariantDef> characterVariantDefs => _characterVariantDefs;
        private static CharacterVariantDef[] _characterVariantDefs;

        public static ReadOnlyArray<ReadOnlyVariantPack> registeredVariantPacks => _readOnlyVariantPacks;
        private static ReadOnlyVariantPack[] _readOnlyVariantPacks;

        public static int variantPackCount => _registeredPacks.Length;
        
        private static VariantPack[] _registeredPacks;
        private static Dictionary<string, VariantPackIndex> _identifierToPackIndex = new Dictionary<string, VariantPackIndex>();

        public static ResourceAvailability managerAvailability;

        private static List<VariantPack> _unregisteredPacks = new List<VariantPack>();
        public static void AddVariantPack(VariantPack variantPack)
        {
            ThrowIfAvailable();
            _unregisteredPacks.Add(variantPack);
        }

        #region Get and Find Methods
        public static ReadOnlyVariantPack GetVariantPack(VariantPackIndex index)
        {
            ThrowIfUnavailable();
            if(HG.ArrayUtils.IsInBounds(_registeredPacks, (int)index))
            {
                return new ReadOnlyVariantPack(_registeredPacks[(int)index]);
            }
            return default;
        }

        public static VariantPackIndex FindVariantPackIndex(string variantPackIdentifier)
        {
            ThrowIfUnavailable();
            return _identifierToPackIndex.GetValueOrDefault(variantPackIdentifier, VariantPackIndex.None);
        }

        public static VariantPackIndex FindVariantPackIndex(CharacterVariantIndex variantIndex) => FindVariantPackIndex(CharacterVariantCatalog.GetCharacterVariantDef(variantIndex));
        public static VariantPackIndex FindVariantPackIndex(CharacterVariantDef variantDef)
        {
            ThrowIfUnavailable();
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
            ThrowIfUnavailable();
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

        [SystemInitializer]
        private static void Initialize()
        {
            _unregisteredPacks.OrderBy(variantPack => variantPack.identifier);

            int packCount = _unregisteredPacks.Count;
            _registeredPacks = new VariantPack[packCount];
            List<ReadOnlyVariantPack> readOnlyVariantPacks = new List<ReadOnlyVariantPack>();

            List<CharacterVariantDef> variantDefs = new List<CharacterVariantDef>();
            List<CharacterVariantTierDef> tierDefs = new List<CharacterVariantTierDef>();
            for(VariantPackIndex packIndex = 0; (int)packIndex < packCount; packIndex++)
            {
                VariantPack pack = _unregisteredPacks[(int)packIndex];
                pack._packIndex = packIndex;
                _registeredPacks[(int)packIndex] = pack;
                _identifierToPackIndex.Add(pack.identifier, packIndex);
                readOnlyVariantPacks.Add(new ReadOnlyVariantPack(pack));

                ModSettingsManager.SetModIcon(pack.packIcon, pack.ownerPlugin.GUID, pack.ownerPlugin.Name);
                ModSettingsManager.SetModDescription(pack.descriptionToken, pack.ownerPlugin.GUID, pack.ownerPlugin.Name);

                foreach(var variant in pack.characterVariantDefs)
                {
                    variant.associatedConfigFile = pack.variantConfig;
                    variantDefs.Add(variant);
                }

                foreach(var tier in pack.characterVariantTierDefs)
                {
                    tier.associatedConfigFile = pack.tierConfig;
                    tierDefs.Add(tier);
                }
            }

            _characterVariantDefs = variantDefs.ToArray();
            _characterVariantTierDefs = tierDefs.ToArray();
            _readOnlyVariantPacks = readOnlyVariantPacks.ToArray();

            VAPILog.Info("VariantPackManager Initialized.");
            managerAvailability.MakeAvailable();
        }

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