using BepInEx.Configuration;
using Moonstorm;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// VarianceAPI's VariantPackCatalog
    /// </summary>
    public static class VariantPackCatalog
    {
        private struct ConfigPair
        {
            public readonly ConfigFile tierConfig;
            public readonly ConfigFile variantConfig;
            public ConfigPair(ConfigFile configFile)
            {
                variantConfig = configFile;
                tierConfig = configFile;
            }

            public ConfigPair(ConfigFile tierFile, ConfigFile variantFile)
            {
                tierConfig = tierFile;
                variantConfig = variantFile;
            }

            public override bool Equals(object obj) => obj is ConfigPair cp && this.Equals(cp);
            public bool Equals(ConfigPair other) => this.tierConfig == other.tierConfig && this.variantConfig == other.variantConfig;
            public override int GetHashCode() => base.GetHashCode();
            public static bool operator ==(ConfigPair lhs, ConfigPair rhs) => lhs.Equals(rhs);
            public static bool operator !=(ConfigPair lhs, ConfigPair rhs) => !(lhs == rhs);
        }

        /// <summary>
        /// The total amount of registered packs
        /// </summary>
        public static int VariantPackCount => registeredPacks.Length;
        /// <summary>
        /// Utilize this to execute an Action as soon as the VariantPackCatalog becomes available
        /// </summary>
        public static ResourceAvailability availability = default(ResourceAvailability);

        private static Dictionary<ConfigPair, List<VariantPackDef>> unregisteredPacks = new Dictionary<ConfigPair, List<VariantPackDef>>();
        internal static VariantPackDef[] registeredPacks = Array.Empty<VariantPackDef>();
        private static readonly Dictionary<string, VariantPackIndex> nameToIndex = new Dictionary<string, VariantPackIndex>();

        #region Find Methods
        /// <summary>
        /// Gets the VariantPackDef tied to the given VariantPackIndex
        /// </summary>
        /// <param name="variantPackIndex">The VariantPackIndex</param>
        /// <returns>The VariantPackDef, null if <paramref name="variantPackIndex"/> is invalid</returns>
        public static VariantPackDef GetVariantPackDef(VariantPackIndex variantPackIndex)
        {
            ThrowIfNotInitialized();
            return HG.ArrayUtils.GetSafe(registeredPacks, (int)variantPackIndex);
        }

        /// <summary>
        /// Finds the VariantPackIndex using the given name
        /// </summary>
        /// <param name="packName">The name of the VariantPackDef to use for the search</param>
        /// <returns>The VariantPackIndex, none if none could be found.</returns>
        public static VariantPackIndex FindVariantPackIndex(string packName)
        {
            ThrowIfNotInitialized();
            if (nameToIndex.TryGetValue(packName, out var index))
            {
                return index;
            }
            return VariantPackIndex.None;
        }

        /// <summary>
        /// Finds the VariantPackDef that implements <paramref name="variant"/>
        /// </summary>
        /// <param name="variant">The VariantDef to use for the search</param>
        /// <returns>The VariantPackDef that implements <paramref name="variant"/></returns>
        public static VariantPackDef FindVariantPackDef(VariantDef variant)
        {
            ThrowIfNotInitialized();
            foreach (VariantPackDef packDef in registeredPacks)
            {
                if (packDef.variants.Contains(variant))
                {
                    return packDef;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the VariantPackDef that implements <paramref name="variantTier"/>
        /// </summary>
        /// <param name="variantTier">The VariantTierDef to use for the search</param>
        /// <returns>The VariantPackDef that implements <paramref name="variantTier"/></returns>
        public static VariantPackDef FindVariantPackDef(VariantTierDef variantTier)
        {
            ThrowIfNotInitialized();
            foreach (VariantPackDef packDef in registeredPacks)
            {
                if (packDef.variantTiers.Contains(variantTier))
                {
                    return packDef;
                }
            }
            return null;
        }
        #endregion

        #region Add Methods
        /// <summary>
        /// Adds all the VariantPacks found in <paramref name="assetBundle"/>, the VariantPacks added this way will not be configurable
        /// </summary>
        /// <param name="assetBundle">The Assetbundle to load from</param>
        public static void AddVariantPacks(AssetBundle assetBundle)
        {
            ThrowIfInitialized();

            AddVariantPacks(assetBundle.LoadAllAssets<VariantPackDef>());
        }
        /// <summary>
        /// Adds all the VariantPacks found in <paramref name="assetBundle"/>, the VariantPack's tiers and variants will be configurable using <paramref name="configFile"/>
        /// </summary>
        /// <param name="assetBundle">The AssetBundle to load from</param>
        /// <param name="configFile">The configFile for the VariantPacks</param>
        public static void AddVariantPacks(AssetBundle assetBundle, ConfigFile configFile)
        {
            ThrowIfInitialized();

            AddVariantPacks(assetBundle.LoadAllAssets<VariantPackDef>(), configFile);
        }

        /// <summary>
        /// Adds all the VariantPacks found in <paramref name="assetBundle"/>, the tiers will be configurable using <paramref name="tierConfig"/> and the variants using <paramref name="variantConfig"/>
        /// </summary>
        /// <param name="assetBundle">The AssetBundle to load from</param>
        /// <param name="tierConfig">The config file for VariantTiers</param>
        /// <param name="variantConfig">The config file for VariantDefs</param>
        public static void AddVariantPacks(AssetBundle assetBundle, ConfigFile tierConfig, ConfigFile variantConfig)
        {
            ThrowIfInitialized();

            AddVariantPacks(assetBundle.LoadAllAssets<VariantPackDef>(), tierConfig, variantConfig);
        }

        /// <summary>
        /// Adds all the VariantPacks specified in <paramref name="variantPacks"/>, the VariantPacks added this way will not be configurable
        /// </summary>
        /// <param name="variantPacks">The VariantPacks to add</param>
        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks)
        {
            ThrowIfInitialized();

            foreach (VariantPackDef packDef in variantPacks)
            {
                AddVariantPack(packDef);
            }
        }

        /// <summary>
        /// Adds all the VariantPacks specified in <paramref name="variantPacks"/>, the VariantPack's tiers and variants will be configurable using <paramref name="configFile"/>
        /// </summary>
        /// <param name="variantPacks">The VariantPacks to add</param>
        /// <param name="configFile">The config file for the VariantPacks</param>
        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks, ConfigFile configFile)
        {
            ThrowIfInitialized();

            foreach (VariantPackDef packDef in variantPacks)
            {
                AddVariantPack(packDef, configFile);
            }
        }

        /// <summary>
        /// Adds all the VariantPAcks specified in <paramref name="variantPacks"/>, the tiers will be configurable using <paramref name="tierConfig"/> and the variants using <paramref name="variantConfig"/>
        /// </summary>
        /// <param name="variantPacks">The VariantPacks to add</param>
        /// <param name="tierConfig">The config file for VariantTiers</param>
        /// <param name="variantConfig">The config file for VariantDefs</param>
        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks, ConfigFile tierConfig, ConfigFile variantConfig)
        {
            ThrowIfInitialized();

            foreach (VariantPackDef packDef in variantPacks)
            {
                AddVariantPack(packDef, tierConfig, variantConfig);
            }
        }

        /// <summary>
        /// Adds a single VariantPack, the pack added cannot be configured
        /// </summary>
        /// <param name="packDef">The pack to add</param>
        public static void AddVariantPack(VariantPackDef packDef)
        {
            ThrowIfInitialized();

            AddPackInternal(packDef, default(ConfigPair));
        }

        /// <summary>
        /// Adds a single VariantPackDef, the pack's tiers and variants can be configured using <paramref name="configFile"/>
        /// </summary>
        /// <param name="packDef">The pack to add</param>
        /// <param name="configFile">The config file for the VariantPack</param>
        public static void AddVariantPack(VariantPackDef packDef, ConfigFile configFile)
        {
            ThrowIfInitialized();

            AddPackInternal(packDef, new ConfigPair(configFile));
        }

        /// <summary>
        /// Adds a single VariantPackDef, the tiers will be configurable using <paramref name="tierConfig"/> and the variants using <paramref name="variantConfig"/>
        /// </summary>
        /// <param name="packDef">The pack to add</param>
        /// <param name="tierConfig">The config file for VariantTiers</param>
        /// <param name="variantConfig">The config file for VariantDefs</param>
        public static void AddVariantPack(VariantPackDef packDef, ConfigFile tierConfig, ConfigFile variantConfig)
        {
            ThrowIfInitialized();

            AddPackInternal(packDef, new ConfigPair(tierConfig, variantConfig));
        }

        private static void AddPackInternal(VariantPackDef packDef, ConfigPair configPair)
        {
            if (!unregisteredPacks.ContainsKey(configPair))
            {
                unregisteredPacks[configPair] = new List<VariantPackDef>();
            }
            unregisteredPacks[configPair].Add(packDef);
        }
        #endregion

        #region Internal Methods
        [SystemInitializer]
        private static void SystemInit()
        {
            nameToIndex.Clear();

            registeredPacks = RegisterPacks();
            unregisteredPacks = null;

            VAPILog.Info("VariantPack Catalog Initialized");

            availability.MakeAvailable();
        }

        private static VariantPackDef[] RegisterPacks()
        {
            List<(VariantPackDef, ConfigPair)> packsToRegister = new List<(VariantPackDef, ConfigPair)>();

            foreach (var (configPair, packs) in unregisteredPacks)
            {
                var validatedPacks = new List<VariantPackDef>();
                validatedPacks = packs.Where(ValidatePack).ToList();

                packsToRegister.AddRange(validatedPacks.Select(x => (x, configPair)));
            }

            packsToRegister = packsToRegister.OrderBy(vpd => vpd.Item1.name).ToList();
            int packAmount = packsToRegister.ToArray().Length;
            for (VariantPackIndex packIndex = 0; (int)packIndex < packAmount; packIndex++)
            {
                RegisterPack(packsToRegister[(int)packIndex], packIndex);
            }
            VAPILog.Info($"Final Packs Registered: {packsToRegister.Count}");
            return packsToRegister.Select(x => x.Item1).ToArray();
        }

        private static bool ValidatePack(VariantPackDef packDef)
        {
            try
            {
                return true;
            }
            catch (Exception e)
            {
                VAPILog.Error($"Could not validate pack {packDef}: {e}");
                return false;
            }
        }

        private static void RegisterPack((VariantPackDef, ConfigPair) variantPack, VariantPackIndex index)
        {
            try
            {
                VariantPackDef packDef = variantPack.Item1;
                packDef.TierConfiguration = variantPack.Item2.tierConfig;
                packDef.VariantConfiguration = variantPack.Item2.variantConfig;
                VAPILog.Debug($"Registering {variantPack} (Index: {index})");
                packDef.VariantPackIndex = index;
                nameToIndex.Add(packDef.name, index);
            }
            catch (Exception e)
            {
                VAPILog.Error($"Could not register pack {variantPack}: {e}");
            }
        }

        private static void ThrowIfNotInitialized()
        {
            if (!availability.available)
                throw new InvalidOperationException("VariantPackCatalog not initialized");
        }

        private static void ThrowIfInitialized()
        {
            if (availability.available)
                throw new InvalidOperationException("VariantPackCatalog already initialized");
        }
        #endregion
    }
}