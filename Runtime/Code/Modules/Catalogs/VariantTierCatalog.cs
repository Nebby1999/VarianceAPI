using BepInEx;
using BepInEx.Configuration;
using MSU.Config;
using RiskOfOptions.OptionConfigs;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAPI
{
    /// <summary>
    /// VarianceAPI's VariantTierCatalog
    /// </summary>
    public static class VariantTierCatalog
    {
        /// <summary>
        /// The total amount of registered tiers
        /// </summary>
        public static int variantTierCount => _registeredTiers.Length;
        /// <summary>
        /// Utilize this to execute an Action as soon as the VariantTierCatalog becomes available
        /// </summary>
        public static ResourceAvailability availability = default(ResourceAvailability);

        private static VariantTierDef[] _registeredTiers;
        private static readonly Dictionary<VariantTierIndex, VariantTierDef> _tierToDef = new Dictionary<VariantTierIndex, VariantTierDef>();


        #region Find Methods
        /// <summary>
        /// Gets the VariantTierDef tied to the given VariantTierIndex
        /// </summary>
        /// <param name="variantTier">The VariantTierIndex</param>
        /// <returns>The VariantTierDef, null if <paramref name="variantTier"/> is invalid</returns>
        public static VariantTierDef GetVariantTierDef(VariantTierIndex variantTier)
        {
            ThrowIfNotInitialized();
            if (_tierToDef.TryGetValue(variantTier, out var def))
                return def;
            return null;
        }

        /// <summary>
        /// Finds the VariantTierDef with the given name
        /// </summary>
        /// <param name="tierName">The name of the VariantTierDef</param>
        /// <returns>The VariantTierDef, null if none could be found</returns>
        public static VariantTierDef FindVariantTierDef(string tierName)
        {
            ThrowIfNotInitialized();
            foreach (VariantTierDef tierDef in _registeredTiers)
            {
                if (tierDef.name == tierName)
                {
                    return tierDef;
                }
            }
            return null;
        }
        #endregion

        #region Internal Methods
        [SystemInitializer(typeof(VariantPackCatalog))]
        private static void SystemInit()
        {
            _tierToDef.Clear();

            _registeredTiers = RegisterTiersFromPacks(VariantPackCatalog._registeredPacks);

            VAPILog.Info("VariantTierCatalog Initialized");
            availability.MakeAvailable();
        }

        private static VariantTierDef[] RegisterTiersFromPacks(VariantPackDef[] packs)
        {
            VAPILog.Info($"Registering VariantTierDefs from {VariantPackCatalog.variantPackCount} VariantPacks.");

            List<VariantTierDef> tiersToRegister = new List<VariantTierDef>();

            foreach (VariantPackDef pack in packs)
            {
                ConfigFile configFile = pack.tierConfiguration;
                BepInPlugin plugin = pack.bepInPlugin;
                VariantTierDef[] tiers = pack.variantTiers;
                if (tiers.Length == 0)
                    continue;

                tiers = tiers.Where(ValidateTier).ToArray();

                if (configFile != null)
                    ConfigureTiersThatPassedFilter(configFile, plugin, tiers);

                tiersToRegister.AddRange(tiers);
            }

#if DEBUG
            VAPILog.Debug($"Registering a total of {tiersToRegister.Count} Tiers");
#endif
            tiersToRegister = tiersToRegister.OrderBy(vtd => vtd.name).ToList();
            int num = 0;
            foreach (VariantTierDef tierDef in tiersToRegister)
            {
                if (tierDef.tier == VariantTierIndex.AssignedAtRuntime)
                {
                    tierDef.tier = (VariantTierIndex)(++num + 10);
                }
                if (_tierToDef.ContainsKey(tierDef.tier))
                {
                    VAPILog.Error($"Duplicate TierDef for tier {tierDef.tier}");
                }
                else
                {
                    _tierToDef.Add(tierDef.tier, tierDef);
                }
            }
            return tiersToRegister.ToArray();
        }

        private static bool ValidateTier(VariantTierDef tierDef)
        {
            try
            {
                return true;
            }
            catch (Exception e)
            {
                VAPILog.Error($"Could not validate tier {tierDef}: {e}");
                return false;
            }
        }

        private static void ConfigureTiersThatPassedFilter(ConfigFile configFile, BepInPlugin plugin, IEnumerable<VariantTierDef> tierDefs)
        {
            foreach (VariantTierDef tierDef in tierDefs)
            {
                try
                {
                    tierDef.goldMultiplier = new ConfiguredFloat(tierDef.goldMultiplier)
                    {
                        section = $"{tierDef.name} Tier",
                        key = "Gold Multiplier",
                        description = "The Gold Multiplier for this tier",
                        configFile = configFile,
                        modName = plugin.Name,
                        modGUID = plugin.GUID,
                        sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                        sliderConfig = new SliderConfig
                        {
                            FormatString = "{0:0.0}",
                            min = 0,
                            max = 100,
                            checkIfDisabled = () => !VAPIConfig.enableRewards
                        },
                    }.WithConfigChange(f =>
                    {
                        tierDef.goldMultiplier = f;
                    }).DoConfigure();

                    tierDef.experienceMultiplier = new ConfiguredFloat(tierDef.experienceMultiplier)
                    {
                        section = $"{tierDef.name} Tier",
                        key = "Experience Multiplier",
                        description = "The Experience Multiplier for this tier",
                        configFile = configFile,
                        modName = plugin.Name,
                        modGUID = plugin.GUID,
                        sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                        sliderConfig = new SliderConfig
                        {
                            FormatString = "{0:0.0}",
                            min = 0,
                            max = 100,
                            checkIfDisabled = () => !VAPIConfig.enableRewards
                        },
                    }.WithConfigChange(f =>
                    {
                        tierDef.experienceMultiplier = f;
                    }).DoConfigure();

                    tierDef.whiteItemDropChance = new ConfiguredFloat(tierDef.whiteItemDropChance)
                    {
                        section = $"{tierDef.name} Tier",
                        key = "White Item Drop Chance",
                        description = "The Chance for variants of this tier to drop a White Item",
                        configFile = configFile,
                        modName = plugin.Name,
                        modGUID = plugin.GUID,
                        sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                        sliderConfig = new SliderConfig
                        {
                            min = 0,
                            max = 100,
                            checkIfDisabled = () => !VAPIConfig.enableRewards
                        },
                    }.WithConfigChange(f =>
                    {
                        tierDef.whiteItemDropChance = f;
                    }).DoConfigure();

                    tierDef.greenItemDropChance = new ConfiguredFloat(tierDef.greenItemDropChance)
                    {
                        section = $"{tierDef.name} Tier",
                        key = "Green Item Drop Chance",
                        description = "The Chance for variants of this tier to drop a Green Item",
                        configFile = configFile,
                        modName = plugin.Name,
                        modGUID = plugin.GUID,
                        sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                        sliderConfig = new SliderConfig
                        {
                            min = 0,
                            max = 100,
                            checkIfDisabled = () => !VAPIConfig.enableRewards
                        },
                    }.WithConfigChange(f =>
                    {
                        tierDef.greenItemDropChance = f;
                    }).DoConfigure();

                    tierDef.redItemDropChance = new ConfiguredFloat(tierDef.redItemDropChance)
                    {
                        section = $"{tierDef.name} Tier",
                        key = "Red Item Drop Chance",
                        description = "The Chance for variants of this tier to drop a Red Item",
                        configFile = configFile,
                        modName = plugin.Name,
                        modGUID = plugin.GUID,
                        sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                        sliderConfig = new SliderConfig
                        {
                            min = 0,
                            max = 100,
                            checkIfDisabled = () => !VAPIConfig.enableRewards
                        },
                    }.WithConfigChange(f =>
                    {
                        tierDef.redItemDropChance = f;
                    }).DoConfigure();
                }
                catch (Exception e)
                {
                    VAPILog.Error($"Error configuring tier {tierDef}: {e}\n(ConfigFile: {configFile}, Tier: {tierDef}");
                }
            }
        }

        private static void ThrowIfNotInitialized()
        {
            if (!availability.available)
                throw new InvalidOperationException($"VariantCatalog not initialized");
        }
        #endregion
    }
}
