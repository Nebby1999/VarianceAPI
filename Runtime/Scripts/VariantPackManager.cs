#nullable enable
using HG;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VAPI
{
    /// <summary>
    /// Index for <see cref="VariantPack"/>s
    /// </summary>
    public enum VariantPackIndex
    {
        /// <summary>
        /// Represents an invalid index
        /// </summary>
        None = -1,
    }

    /// <summary>
    /// Class for managing VariantPacks, VariantPacks are added to this class and then <see cref="CharacterVariantTierCatalog"/> and <see cref="CharacterVariantCatalog"/> use them to register their respective scriptable objects
    /// </summary>
    public static class VariantPackManager
    {
        /// <summary>
        /// Returns a read only array of all the <see cref="CharacterVariantTierDef"/>s from the API
        /// </summary>
        public static ReadOnlyArray<CharacterVariantTierDef> characterVariantTierDefs => _characterVariantTierDefs;
        private static CharacterVariantTierDef[] _characterVariantTierDefs = Array.Empty<CharacterVariantTierDef>();

        /// <summary>
        /// Returns a read only array of all the <see cref="CharacterVariantDef"/>s from the API
        /// </summary>
        public static ReadOnlyArray<CharacterVariantDef> characterVariantDefs => _characterVariantDefs;
        private static CharacterVariantDef[] _characterVariantDefs = Array.Empty<CharacterVariantDef>();

        /// <summary>
        /// Returns an read only array of all the <see cref="ReadOnlyVariantPack"/>s from the API
        /// </summary>
        public static ReadOnlyArray<ReadOnlyVariantPack> registeredVariantPacks => _readOnlyVariantPacks;
        private static ReadOnlyVariantPack[] _readOnlyVariantPacks = Array.Empty<ReadOnlyVariantPack>();

        /// <summary>
        /// Returns the amount of VariantPacks registered in the manager
        /// <br></br>
        /// Returns -1 if the manager is not initialized
        /// </summary>
        public static int variantPackCount => _registeredPacks?.Length ?? -1;
        
        private static VariantPack[] _registeredPacks = Array.Empty<VariantPack>();
        private static Dictionary<string, VariantPackIndex> _identifierToPackIndex = new Dictionary<string, VariantPackIndex>();

        /// <summary>
        /// Manager availability, becomes available once all variants are registered 
        /// </summary>
        public static ResourceAvailability managerAvailability;

        private static List<VariantPack> _unregisteredPacks = new List<VariantPack>();

        /// <summary>
        /// Adds a VariantPack to the manager.
        /// <br></br>
        /// Throws an <see cref="InvalidOperationException"/> if the manager has already initialized
        /// </summary>
        /// <param name="variantPack">The VariantPack to add</param>
        public static void AddVariantPack(VariantPack? variantPack)
        {
            ThrowIfAvailable();

            if(variantPack == null)
            {
                VAPILog.Error($"VariantPack cannot be null!");
                return;
            }

            _unregisteredPacks.Add(variantPack);
        }

        #region Get and Find Methods
        /// <summary>
        /// Retrieves the <see cref="ReadOnlyVariantPack"/> at the requested <paramref name="index"/>.
        /// <br></br>
        /// The value returned may be invalid if the <paramref name="index"/> is out of bounds
        /// </summary>
        /// <param name="index">The index of variant pack to retrieve</param>
        public static ReadOnlyVariantPack GetVariantPack(VariantPackIndex index)
        {
            ThrowIfUnavailable();

            if(HG.ArrayUtils.IsInBounds(_readOnlyVariantPacks, (int)index))
            {
                return _readOnlyVariantPacks[(int)index];
            }
            return default;
        }

        /// <summary>
        /// Finds the <see cref="VariantPackIndex"/> which pack has the provided <paramref name="variantPackIdentifier"/>.
        /// <br></br>
        /// Returns <see cref="VariantPackIndex.None"/> if there's no pack with the specified identifier
        /// </summary>
        /// <param name="variantPackIdentifier">The pack identifier</param>
        public static VariantPackIndex FindVariantPackIndex(string variantPackIdentifier)
        {
            ThrowIfUnavailable();
            return _identifierToPackIndex.GetValueOrDefault(variantPackIdentifier, VariantPackIndex.None);
        }

        /// <summary>
        /// Finds the <see cref="VariantPackIndex"/> that owns the provided <paramref name="variantIndex"/>
        /// <br></br>
        /// Returns <see cref="VariantPackIndex.None"/> if no pack contains a character variant def of the specified index.
        /// </summary>
        /// <param name="variantIndex">The <see cref="CharacterVariantIndex"/> to retrieve it's Pack.</param>
        public static VariantPackIndex FindVariantPackIndex(CharacterVariantIndex variantIndex) => FindVariantPackIndex(CharacterVariantCatalog.GetCharacterVariantDef(variantIndex));

        /// <summary>
        /// Finds the <see cref="VariantPackIndex"/> that owns the provided <paramref name="variantDef"/>
        /// <br></br>
        /// Returns <see cref="VariantPackIndex.None"/> if no pack contains the specified variantDef
        /// </summary>
        /// <param name="variantDef">The <see cref="CharacterVariantDef"/> to retrieve it's Pack</param>
        public static VariantPackIndex FindVariantPackIndex(CharacterVariantDef? variantDef)
        {
            ThrowIfUnavailable();

            if(variantDef == null)
            {
                VAPILog.Error($"CharacterVariantDef cannot be null!");
                return VariantPackIndex.None;
            }

            for(int i = 0; i < _registeredPacks.Length; i++)
            {
                if (_registeredPacks[i].characterVariantDefs.Contains(variantDef))
                {
                    return _registeredPacks[i].variantPackIndex;
                }
            }
            return VariantPackIndex.None;
        }

        /// <summary>
        /// Finds the <see cref="VariantPackIndex"/> that owns the provided <paramref name="tierIndex"/>
        /// <br></br>
        /// Returns <see cref="VariantPackIndex.None"/> if no pack contains a CharacterVariantTierDef of the specified index.
        /// </summary>
        /// <param name="tierIndex">The <see cref="CharacterVariantTierIndex"/> to retrieve it's Pack.</param>
        public static VariantPackIndex FindVariantPackIndex(CharacterVariantTierIndex tierIndex) => FindVariantPackIndex(CharacterVariantTierCatalog.GetCharacterVariantTierDef(tierIndex));

        /// <summary>
        /// Finds the <see cref="VariantPackIndex"/> that owns the provided <paramref name="tierDef"/>
        /// <br></br>
        /// Returns <see cref="VariantPackIndex.None"/> if no pack contains the specified tierDef
        /// </summary>
        /// <param name="tierDef">The <see cref="CharacterVarCharacterVariantTierDefiantDef"/> to retrieve it's Pack</param>
        public static VariantPackIndex FindVariantPackIndex(CharacterVariantTierDef? tierDef)
        {
            ThrowIfUnavailable();
            if (tierDef == null)
            {
                VAPILog.Error($"CharacterVariantTierDef cannot be null!");
                return VariantPackIndex.None;
            }

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
            //Order them by identifier
            _unregisteredPacks.OrderBy(variantPack => variantPack.identifier);

            int packCount = _unregisteredPacks.Count;
            _registeredPacks = new VariantPack[packCount];
            List<ReadOnlyVariantPack> readOnlyVariantPacks = new List<ReadOnlyVariantPack>();

            using var _ = ListPool<CharacterVariantDef>.RentCollection(out var variantDefs);
            List<CharacterVariantTierDef> tierDefs = new List<CharacterVariantTierDef>();

            //Iterate and assign indices
            for(VariantPackIndex packIndex = 0; (int)packIndex < packCount; packIndex++)
            {
                VariantPack pack = _unregisteredPacks[(int)packIndex];

                //assign index to pack, add to manager
                pack._packIndex = packIndex;
                _registeredPacks[(int)packIndex] = pack;
                _identifierToPackIndex.Add(pack.identifier, packIndex);
                readOnlyVariantPacks.Add(new ReadOnlyVariantPack(pack));

                //Set the mod's icon in ROO
                ModSettingsManager.SetModIcon(pack.packIcon, pack.ownerPlugin.GUID, pack.ownerPlugin.Name);
                ModSettingsManager.SetModDescription(pack.descriptionToken, pack.ownerPlugin.GUID, pack.ownerPlugin.Name);

                //Associate each variant and tier with the specified config files, add them to the manager
                foreach(var variant in pack.characterVariantDefs)
                {
                    variant.associatedConfigFile = pack.variantConfig;
                    variant.ownerPlugin = pack.ownerPlugin;
                    variantDefs.Add(variant);
                }

                foreach(var tier in pack.characterVariantTierDefs)
                {
                    tier.associatedConfigFile = pack.tierConfig;
                    tier.ownerPlugin = pack.ownerPlugin;
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