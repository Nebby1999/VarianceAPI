using BepInEx;
using BepInEx.Configuration;
using Moonstorm;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private static Dictionary<ConfigPair, List<(VariantPackDef, BepInPlugin)>> unregisteredPacks = new Dictionary<ConfigPair, List<(VariantPackDef, BepInPlugin)>>();
        internal static VariantPackDef[] registeredPacks = Array.Empty<VariantPackDef>();
        private static readonly Dictionary<string, VariantPackIndex> nameToIndex = new Dictionary<string, VariantPackIndex>(StringComparer.OrdinalIgnoreCase);

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

        //This is complete cancer and needs to be rectified in the next breaking update of the api.
        #region Add Methods
        /// <summary>
        /// Adds all the VariantPacks found in <paramref name="assetBundle"/>, the VariantPacks added this way will not be configurable
        /// </summary>
        /// <param name="assetBundle">The Assetbundle to load from</param>
        public static void AddVariantPacks(AssetBundle assetBundle)
        {
            ThrowIfInitialized();
            var cfg = default(ConfigPair);
            var plugin = GetBepInPlugin(Assembly.GetCallingAssembly());
            foreach (VariantPackDef packDef in assetBundle.LoadAllAssets<VariantPackDef>())
            {
                AddPackInternal(packDef, cfg, plugin);
            }
        }

        /// <summary>
        /// Adds all the VariantPacks found in <paramref name="assetBundle"/>, the VariantPacks added this way will not be configurable
        /// </summary>
        /// <param name="assetBundle">The Assetbundle to load from</param>
        /// <param name="ownerPlugin">The plugin that added the variant packs</param>
        public static void AddVariantPacks(AssetBundle assetBundle, BaseUnityPlugin ownerPlugin)
        {
            ThrowIfInitialized();

            var cfg = default(ConfigPair);
            var plugin = ownerPlugin.Info.Metadata;
            foreach(VariantPackDef packDef in assetBundle.LoadAllAssets<VariantPackDef>())
            {
                AddPackInternal(packDef, cfg, plugin);
            }

        }
        /// <summary>
        /// Adds all the VariantPacks found in <paramref name="assetBundle"/>, the VariantPack's tiers and variants will be configurable using <paramref name="configFile"/>
        /// </summary>
        /// <param name="assetBundle">The AssetBundle to load from</param>
        /// <param name="configFile">The configFile for the VariantPacks</param>
        public static void AddVariantPacks(AssetBundle assetBundle, ConfigFile configFile)
        {
            ThrowIfInitialized();

            var cfg = new ConfigPair(configFile);
            var plugin = GetBepInPlugin(Assembly.GetCallingAssembly());
            foreach (VariantPackDef packDef in assetBundle.LoadAllAssets<VariantPackDef>())
            {
                AddPackInternal(packDef, cfg, plugin);
            }
        }

        /// <summary>
        /// Adds all the VariantPacks found in <paramref name="assetBundle"/>, the VariantPack's tiers and variants will be configurable using <paramref name="configFile"/>
        /// </summary>
        /// <param name="assetBundle">The AssetBundle to load from</param>
        /// <param name="configFile">The configFile for the VariantPacks</param>
        /// <param name="ownerPlugin">The plugin that added the variant packs</param>
        public static void AddVariantPacks(AssetBundle assetBundle, ConfigFile configFile, BaseUnityPlugin ownerPlugin)
        {
            ThrowIfInitialized();
            var cfg = new ConfigPair(configFile);
            var plugin = ownerPlugin.Info.Metadata;
            foreach(VariantPackDef def in assetBundle.LoadAllAssets<VariantPackDef>())
            {
                AddPackInternal(def, cfg, plugin);
            }
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
            var cfg = new ConfigPair(tierConfig, variantConfig);
            var plugin = GetBepInPlugin(Assembly.GetCallingAssembly());
            foreach(VariantPackDef packDef in assetBundle.LoadAllAssets<VariantPackDef>())
            {
                AddPackInternal(packDef, cfg, plugin);
            }
        }

        /// <summary>
        /// Adds all the VariantPacks found in <paramref name="assetBundle"/>, the tiers will be configurable using <paramref name="tierConfig"/> and the variants using <paramref name="variantConfig"/>
        /// </summary>
        /// <param name="assetBundle">The AssetBundle to load from</param>
        /// <param name="tierConfig">The config file for VariantTiers</param>
        /// <param name="variantConfig">The config file for VariantDefs</param>
        /// <param name="ownerPlugin">The plugin that added the variant packs</param>
        public static void AddVariantPacks(AssetBundle assetBundle, ConfigFile tierConfig, ConfigFile variantConfig, BaseUnityPlugin ownerPlugin)
        {
            ThrowIfInitialized();
            var cfg = new ConfigPair(tierConfig, variantConfig);
            var plugin = ownerPlugin.Info.Metadata;
            foreach(VariantPackDef packDef in assetBundle.LoadAllAssets<VariantPackDef>())
            {
                AddPackInternal(packDef, cfg, plugin);
            }
        }

        /// <summary>
        /// Adds all the VariantPacks specified in <paramref name="variantPacks"/>, the VariantPacks added this way will not be configurable
        /// </summary>
        /// <param name="variantPacks">The VariantPacks to add</param>
        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks)
        {
            ThrowIfInitialized();
            var plugin = GetBepInPlugin(Assembly.GetCallingAssembly());
            foreach (VariantPackDef packDef in variantPacks)
            {
                AddPackInternal(packDef, default(ConfigPair), plugin);
            }
        }

        /// <summary>
        /// Adds all the VariantPacks specified in <paramref name="variantPacks"/>, the VariantPacks added this way will not be configurable
        /// </summary>
        /// <param name="variantPacks">The VariantPacks to add</param>
        /// <param name="ownerPlugin">The plugin that added the variant packs</param>
        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks, BaseUnityPlugin ownerPlugin)
        {
            ThrowIfInitialized();
            var plugin = ownerPlugin.Info.Metadata;
            foreach(VariantPackDef packDef in variantPacks)
            {
                AddPackInternal(packDef, default(ConfigPair), plugin);
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
            var cfg = new ConfigPair(configFile);
            var plugin = GetBepInPlugin(Assembly.GetCallingAssembly());
            foreach (VariantPackDef packDef in variantPacks)
            {
                AddPackInternal(packDef, cfg, plugin); ;
            }
        }

        /// <summary>
        /// Adds all the VariantPacks specified in <paramref name="variantPacks"/>, the VariantPack's tiers and variants will be configurable using <paramref name="configFile"/>
        /// </summary>
        /// <param name="variantPacks">The VariantPacks to add</param>
        /// <param name="configFile">The config file for the VariantPacks</param>
        /// <param name="ownerPlugin">The plugin that added the variant packs</param>
        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks, ConfigFile configFile, BaseUnityPlugin ownerPlugin)
        {
            ThrowIfInitialized();
            var cfg = new ConfigPair(configFile);
            var plugin = ownerPlugin.Info.Metadata;
            foreach (VariantPackDef packDef in variantPacks)
            {
                AddPackInternal(packDef, cfg, plugin); ;
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
            var cfg = new ConfigPair(tierConfig, variantConfig);
            var plugin = GetBepInPlugin(Assembly.GetCallingAssembly());
            foreach (VariantPackDef packDef in variantPacks)
            {
                AddPackInternal(packDef, cfg, plugin);
            }
        }

        /// <summary>
        /// Adds all the VariantPAcks specified in <paramref name="variantPacks"/>, the tiers will be configurable using <paramref name="tierConfig"/> and the variants using <paramref name="variantConfig"/>
        /// </summary>
        /// <param name="variantPacks">The VariantPacks to add</param>
        /// <param name="tierConfig">The config file for VariantTiers</param>
        /// <param name="variantConfig">The config file for VariantDefs</param>
        /// <param name="ownerPlugin">The plugin that added the variant packs</param>
        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks, ConfigFile tierConfig, ConfigFile variantConfig, BaseUnityPlugin ownerPlugin)
        {
            ThrowIfInitialized();
            var cfg = new ConfigPair(tierConfig, variantConfig);
            var plugin = ownerPlugin.Info.Metadata;
            foreach (VariantPackDef packDef in variantPacks)
            {
                AddPackInternal(packDef, cfg, plugin);
            }
        }

        /// <summary>
        /// Adds a single VariantPack, the pack added cannot be configured
        /// </summary>
        /// <param name="packDef">The pack to add</param>
        public static void AddVariantPack(VariantPackDef packDef)
        {
            ThrowIfInitialized();

            AddPackInternal(packDef, default(ConfigPair), GetBepInPlugin(Assembly.GetCallingAssembly()));
        }

        /// <summary>
        /// Adds a single VariantPack, the pack added cannot be configured
        /// </summary>
        /// <param name="packDef">The pack to add</param>
        /// <param name="ownerPlugin">The plugin that added the VariantPack</param>
        public static void AddVariantPack(VariantPackDef packDef, BaseUnityPlugin ownerPlugin)
        {
            ThrowIfInitialized();

            AddPackInternal(packDef, default(ConfigPair), ownerPlugin.Info.Metadata);
        }

        /// <summary>
        /// Adds a single VariantPackDef, the pack's tiers and variants can be configured using <paramref name="configFile"/>
        /// </summary>
        /// <param name="packDef">The pack to add</param>
        /// <param name="configFile">The config file for the VariantPack</param>
        public static void AddVariantPack(VariantPackDef packDef, ConfigFile configFile)
        {
            ThrowIfInitialized();

            AddPackInternal(packDef, new ConfigPair(configFile), GetBepInPlugin(Assembly.GetCallingAssembly()));
        }

        /// <summary>
        /// Adds a single VariantPackDef, the pack's tiers and variants can be configured using <paramref name="configFile"/>
        /// </summary>
        /// <param name="packDef">The pack to add</param>
        /// <param name="configFile">The config file for the VariantPack</param>
        /// <param name="ownerPlugin">The plugin that added the VariantPack</param>
        public static void AddVariantPack(VariantPackDef packDef, ConfigFile configFile, BaseUnityPlugin ownerPlugin)
        {
            ThrowIfInitialized();

            AddPackInternal(packDef, new ConfigPair(configFile), ownerPlugin.Info.Metadata);
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

            AddPackInternal(packDef, new ConfigPair(tierConfig, variantConfig), GetBepInPlugin(Assembly.GetCallingAssembly()));
        }

        /// <summary>
        /// Adds a single VariantPackDef, the tiers will be configurable using <paramref name="tierConfig"/> and the variants using <paramref name="variantConfig"/>
        /// </summary>
        /// <param name="packDef">The pack to add</param>
        /// <param name="tierConfig">The config file for VariantTiers</param>
        /// <param name="variantConfig">The config file for VariantDefs</param>
        /// <param name="ownerPlugin">The plugin that added the VariantPack</param>
        public static void AddVariantPack(VariantPackDef packDef, ConfigFile tierConfig, ConfigFile variantConfig, BaseUnityPlugin ownerPlugin)
        {
            ThrowIfInitialized();

            AddPackInternal(packDef, new ConfigPair(tierConfig, variantConfig), ownerPlugin.Info.Metadata);
        }
        private static BepInPlugin GetBepInPlugin(Assembly assembly)
        {
            return assembly.GetTypesSafe()
                .Where(t => t.GetCustomAttribute<BepInPlugin>() != null)
                .Select(t => t.GetCustomAttribute<BepInPlugin>())
                .FirstOrDefault();
        }

        private static void AddPackInternal(VariantPackDef packDef, ConfigPair configPair, BepInPlugin plugin)
        {
            if (!unregisteredPacks.ContainsKey(configPair))
            {
                unregisteredPacks[configPair] = new List<(VariantPackDef, BepInPlugin)>();
            }
            unregisteredPacks[configPair].Add((packDef, plugin));
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
            List<(VariantPackDef, ConfigPair, BepInPlugin)> packsToRegister = new List<(VariantPackDef, ConfigPair, BepInPlugin)>();

            foreach (var (configPair, packs) in unregisteredPacks)
            {
                var validatedPacks = new List<(VariantPackDef, BepInPlugin)>();
                validatedPacks = packs.Where(ValidatePack).ToList();

                packsToRegister.AddRange(validatedPacks.Select(x => (x.Item1, configPair, x.Item2)));
            }

            packsToRegister = packsToRegister.OrderBy(vpd => vpd.Item1.name).ToList();
            int packAmount = packsToRegister.ToArray().Length;
            for (VariantPackIndex packIndex = 0; (int)packIndex < packAmount; packIndex++)
            {
                RegisterPack(packsToRegister[(int)packIndex], packIndex);
            }
#if DEBUG
            VAPILog.Info($"Final Packs Registered: {packsToRegister.Count}");
#endif
            return packsToRegister.Select(x => x.Item1).ToArray();
        }

        private static bool ValidatePack((VariantPackDef packDef, BepInPlugin plugin) tuple)
        {
            try
            {
                return true;
            }
            catch (Exception e)
            {
                VAPILog.Error($"Could not validate pack {tuple.packDef}: {e}");
                return false;
            }
        }

        private static void RegisterPack((VariantPackDef packDef, ConfigPair pair, BepInPlugin plugin) variantPack, VariantPackIndex index)
        {
            try
            {
                VariantPackDef packDef = variantPack.packDef;
                packDef.TierConfiguration = variantPack.pair.tierConfig;
                packDef.VariantConfiguration = variantPack.pair.variantConfig;
                packDef.BepInPlugin = variantPack.plugin;
                ModSettingsManager.SetModIcon(packDef.packEnabledIcon, packDef.BepInPlugin.GUID, packDef.BepInPlugin.Name);
                ModSettingsManager.SetModDescriptionToken(packDef.descriptionToken, packDef.BepInPlugin.GUID, packDef.BepInPlugin.Name);
#if DEBUG
                VAPILog.Debug($"Registering {variantPack} (Index: {index})");
#endif
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