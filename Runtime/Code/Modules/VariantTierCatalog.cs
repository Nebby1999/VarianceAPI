using BepInEx.Configuration;
using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VAPI
{
    public static class VariantTierCatalog
    {
        public static int variantTierCount => registeredTiers.Length;
        public static bool Initialized { get; private set; } = false;

        private static Dictionary<ConfigFile, List<VariantTierDef>> unregisteredTiers = new Dictionary<ConfigFile, List<VariantTierDef>>();
        private static VariantTierDef[] registeredTiers;
        private static readonly Dictionary<VariantTierIndex, VariantTierDef> tierToDef = new Dictionary<VariantTierIndex, VariantTierDef>();


        #region Find Methods
        public static VariantTierDef GetVariantTierDef(VariantTierIndex variantTier)
        {
            ThrowIfNotInitialized();
            if (tierToDef.TryGetValue(variantTier, out var def))
                return def;
            return null;
        }

        public static VariantTierDef FindVariantTierDef(string tierName)
        {
            ThrowIfNotInitialized();
            foreach(VariantTierDef tierDef in registeredTiers)
            {
                if(tierDef.name == tierName)
                {
                    return tierDef;
                }
            }
            return null;
        }
        #endregion

        #region Add Methods
        public static void AddTiers(AssetBundle assetBundle, [NotNull] ConfigFile configFile)
        {
            ThrowIfInitialized();

            if (configFile == null)
                throw new NullReferenceException("configFile");

            AddTiers(assetBundle.LoadAllAssets<VariantTierDef>(), configFile);
        }

        public static void AddTiers(IEnumerable<VariantTierDef> tierDefs, ConfigFile configFile)
        {
            ThrowIfInitialized();

            if (configFile == null)
                throw new NullReferenceException("configFile");

            tierDefs.ToList().ForEach(vtd => AddTier(vtd, configFile));
        }

        public static void AddTier(VariantTierDef tierDef, ConfigFile configFile)
        {
            ThrowIfInitialized();

            if (configFile == null)
                throw new NullReferenceException("configFile");

            if(unregisteredTiers.ContainsKey(configFile))
            {
                unregisteredTiers.Add(configFile, new List<VariantTierDef>());
            }
            unregisteredTiers[configFile].Add(tierDef);
        }
        #endregion

        #region Internal Methods
        [SystemInitializer]
        private static void SystemInit()
        {
            tierToDef.Clear();

            foreach(KeyValuePair<ConfigFile, List<VariantTierDef>> configAndVariants in unregisteredTiers)
            {
                unregisteredTiers[configAndVariants.Key] = configAndVariants.Value.OrderBy(vd => vd.name).ToList();
            }

            registeredTiers = RegisterTiers(unregisteredTiers).ToArray();
            unregisteredTiers = null;
            Initialized = true;
        }

        private static List<VariantTierDef> RegisterTiers(Dictionary<ConfigFile, List<VariantTierDef>> unregisteredTiers)
        {
            List<VariantTierDef> tiersToRegister = new List<VariantTierDef>();
            
            foreach(KeyValuePair<ConfigFile, List<VariantTierDef>> configTiersPair in unregisteredTiers)
            {
                ConfigFile config = configTiersPair.Key;
                List<VariantTierDef> tiers = configTiersPair.Value;

                tiers = tiers.Where(ValidateTier).ToList();
                ConfigureTiersThatPassedFilter(config, tiers);
                tiersToRegister.AddRange(tiers);
            }

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
            return tiersToRegister;
        }

        private static bool ValidateTier(VariantTierDef tierDef)
        {
            try
            {
                return true;
            }
            catch(Exception e)
            {
                VAPILog.Error($"Could not validate tier {tierDef}: {e}");
                return false;
            }
        }

        private static void ConfigureTiersThatPassedFilter(ConfigFile configFile, List<VariantTierDef> tierDefs)
        {
            foreach(VariantTierDef tierDef in tierDefs)
            {
                try
                {
                    tierDef.goldMultiplier = BindInternal<float>(tierDef, 
                        "Gold Multiplier", 
                        tierDef.goldMultiplier, 
                        "The Gold Multiplier for this tier");

                    tierDef.experienceMultiplier = BindInternal<float>(tierDef,
                        "XP Multiplier",
                        tierDef.experienceMultiplier,
                        "The Experience Multiplier for this tier");

                    tierDef.whiteItemDropChance = BindInternal<float>(tierDef,
                        "White Item Drop Chance",
                        tierDef.whiteItemDropChance,
                        "The Chance for variants of this tier to drop a white item");

                    tierDef.greenItemDropChance = BindInternal<float>(tierDef,
                        "Green Item Drop Chance",
                        tierDef.greenItemDropChance,
                        "The Chance for variants of this tier to drop a green item");

                    tierDef.redItemDropChance = BindInternal<float>(tierDef,
                        "Red Item Drop Chance",
                        tierDef.redItemDropChance,
                        "The Chance for variants of this tier to drop a red item");
                }
                catch(Exception e)
                {
                    VAPILog.Error($"Error configuring tier {tierDef}: {e}\n(ConfigFile: {configFile}, Tier: {tierDef}");
                }
            }

            T BindInternal<T>(VariantTierDef tierDef, string key, T val, string desc)
            {
                return configFile.Bind<T>($"{tierDef.name} Tier",
                    $"{tierDef.name} {key}",
                    val,
                    desc).Value;
            }
        }

        private static void ThrowIfNotInitialized()
        {
            if(!Initialized)
                throw new InvalidOperationException($"VariantCatalog not initialized");
        }

        private static void ThrowIfInitialized()
        {
            if(Initialized)
                throw new InvalidOperationException("VariantCatalog already initialized.");
        }
        #endregion
    }
}
