using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Config;
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
        public static int variantTierCount => registeredTiers.Length;
        /// <summary>
        /// Utilize this to execute an Action as soon as the VariantTierCatalog becomes available
        /// </summary>
        public static ResourceAvailability availability = default(ResourceAvailability);

        private static VariantTierDef[] registeredTiers;
        private static readonly Dictionary<VariantTierIndex, VariantTierDef> tierToDef = new Dictionary<VariantTierIndex, VariantTierDef>();


        #region Find Methods
        /// <summary>
        /// Gets the VariantTierDef tied to the given VariantTierIndex
        /// </summary>
        /// <param name="variantTier">The VariantTierIndex</param>
        /// <returns>The VariantTierDef, null if <paramref name="variantTier"/> is invalid</returns>
        public static VariantTierDef GetVariantTierDef(VariantTierIndex variantTier)
        {
            ThrowIfNotInitialized();
            if (tierToDef.TryGetValue(variantTier, out var def))
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
            foreach (VariantTierDef tierDef in registeredTiers)
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
            tierToDef.Clear();

            registeredTiers = RegisterTiersFromPacks(VariantPackCatalog.registeredPacks);

            VAPILog.Info("VariantTierCatalog Initialized");
            availability.MakeAvailable();
        }

        private static VariantTierDef[] RegisterTiersFromPacks(VariantPackDef[] packs)
        {
            VAPILog.Info($"Registering VariantTierDefs from {VariantPackCatalog.VariantPackCount} VariantPacks.");

            List<VariantTierDef> tiersToRegister = new List<VariantTierDef>();

            foreach (VariantPackDef pack in packs)
            {
                ConfigFile configFile = pack.TierConfiguration;
                BepInPlugin plugin = pack.BepInPlugin;
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
                if (tierDef.Tier == VariantTierIndex.AssignedAtRuntime)
                {
                    tierDef.Tier = (VariantTierIndex)(++num + 10);
                }
                if (tierToDef.ContainsKey(tierDef.Tier))
                {
                    VAPILog.Error($"Duplicate TierDef for tier {tierDef.Tier}");
                }
                else
                {
                    tierToDef.Add(tierDef.Tier, tierDef);
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
                    tierDef.goldMultiplier = new ConfigurableFloat(tierDef.goldMultiplier)
                    {
                        Section = $"{tierDef.name} Tier",
                        Key = "Gold Multiplier",
                        Description = "The Gold Multiplier for this tier",
                        ConfigFile = configFile,
                        ModName = plugin.Name,
                        ModGUID = plugin.GUID,
                        UseStepSlider = false,
                        SliderConfig = new SliderConfig
                        {
                            formatString = "{0:0.0}",
                            min = 0,
                            max = 100,
                            checkIfDisabled = () => !VAPIConfig.enableRewards
                        },
                    }.AddOnConfigChanged(f =>
                    {
                        tierDef.goldMultiplier = f;
                    }).DoConfigure();

                    tierDef.experienceMultiplier = new ConfigurableFloat(tierDef.experienceMultiplier)
                    {
                        Section = $"{tierDef.name} Tier",
                        Key = "Experience Multiplier",
                        Description = "The Experience Multiplier for this tier",
                        ConfigFile = configFile,
                        ModName = plugin.Name,
                        ModGUID = plugin.GUID,
                        UseStepSlider = false,
                        SliderConfig = new SliderConfig
                        {
                            formatString = "{0:0.0}",
                            min = 0,
                            max = 100,
                            checkIfDisabled = () => !VAPIConfig.enableRewards
                        },
                    }.AddOnConfigChanged(f =>
                    {
                        tierDef.experienceMultiplier = f;
                    }).DoConfigure();

                    tierDef.whiteItemDropChance = new ConfigurableFloat(tierDef.whiteItemDropChance)
                    {
                        Section = $"{tierDef.name} Tier",
                        Key = "White Item Drop Chance",
                        Description = "The Chance for variants of this tier to drop a White Item",
                        ConfigFile = configFile,
                        ModName = plugin.Name,
                        ModGUID = plugin.GUID,
                        UseStepSlider = false,
                        SliderConfig = new SliderConfig
                        {
                            min = 0,
                            max = 100,
                            checkIfDisabled = () => !VAPIConfig.enableRewards
                        },
                    }.AddOnConfigChanged(f =>
                    {
                        tierDef.whiteItemDropChance = f;
                    }).DoConfigure();

                    tierDef.greenItemDropChance = new ConfigurableFloat(tierDef.greenItemDropChance)
                    {
                        Section = $"{tierDef.name} Tier",
                        Key = "Green Item Drop Chance",
                        Description = "The Chance for variants of this tier to drop a Green Item",
                        ConfigFile = configFile,
                        ModName = plugin.Name,
                        ModGUID = plugin.GUID,
                        UseStepSlider = false,
                        SliderConfig = new SliderConfig
                        {
                            min = 0,
                            max = 100,
                            checkIfDisabled = () => !VAPIConfig.enableRewards
                        },
                    }.AddOnConfigChanged(f =>
                    {
                        tierDef.greenItemDropChance = f;
                    }).DoConfigure();

                    tierDef.redItemDropChance = new ConfigurableFloat(tierDef.redItemDropChance)
                    {
                        Section = $"{tierDef.name} Tier",
                        Key = "Red Item Drop Chance",
                        Description = "The Chance for variants of this tier to drop a Red Item",
                        ConfigFile = configFile,
                        ModName = plugin.Name,
                        ModGUID = plugin.GUID,
                        UseStepSlider = false,
                        SliderConfig = new SliderConfig
                        {
                            min = 0,
                            max = 100,
                            checkIfDisabled = () => !VAPIConfig.enableRewards
                        },
                    }.AddOnConfigChanged(f =>
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
