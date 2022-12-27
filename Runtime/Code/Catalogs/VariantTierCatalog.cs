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
        public static event Action OnTiersAssigned;

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

        #region Internal Methods
        [SystemInitializer(typeof(VariantPackCatalog))]
        private static void SystemInit()
        {
            tierToDef.Clear();

            registeredTiers = RegisterTiersFromPacks(VariantPackCatalog.registeredPacks);
            Initialized = true;
            OnTiersAssigned?.Invoke();
        }

        private static VariantTierDef[] RegisterTiersFromPacks(VariantPackDef[] packs)
        {
            List<VariantTierDef> tiersToRegister = new List<VariantTierDef>();
            
            foreach(VariantPackDef pack in packs)
            {
                ConfigFile configFile = pack.tierConfiguration;
                VariantTierDef[] tiers = pack.variantTiers;
                if (tiers.Length == 0)
                    continue;

                tiers = tiers.Where(ValidateTier).ToArray();

                if(configFile != null)
                    ConfigureTiersThatPassedFilter(configFile, tiers);
    
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
            return tiersToRegister.ToArray();
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

        private static void ConfigureTiersThatPassedFilter(ConfigFile configFile, IEnumerable<VariantTierDef> tierDefs)
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
