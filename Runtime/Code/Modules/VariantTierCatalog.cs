using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPI
{
    public static class VariantTierCatalog
    {
        public static int variantTierCount => registeredTiers.Length;
        public static bool Initialized { get; private set; } = false;

        private static VariantTierDef[] unregisteredTiers;
        private static VariantTierDef[] registeredTiers;
        private static readonly Dictionary<VariantTierIndex, VariantTierDef> tierToDef = new Dictionary<VariantTierIndex, VariantTierDef>();

        #region Add Methods
        public static void AddTiers(VariantTierDef[] tierDef)
        {
            ThrowIfInitialized();
            tierDef.ToList().ForEach(vtd => AddTier(vtd));
        }

        public static void AddTier(VariantTierDef tierDef)
        {
            ThrowIfInitialized();
            HG.ArrayUtils.ArrayAppend(ref unregisteredTiers, tierDef); 
        }
        #endregion

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
        [SystemInitializer]
        private static void SystemInit()
        {
            tierToDef.Clear();

            unregisteredTiers = unregisteredTiers.OrderBy(vtd => vtd.name).ToArray();

            registeredTiers = RegisterTiers(unregisteredTiers);
            unregisteredTiers = null;
            Initialized = true;
        }

        private static VariantTierDef[] RegisterTiers(VariantTierDef[] tiersToRegister)
        {
            List<VariantTierDef> validTiers = new List<VariantTierDef>();

            foreach(VariantTierDef tierDef in tiersToRegister)
            {
                try
                {
                    //Validate
                    validTiers.Add(tierDef);
                }
                catch(Exception e)
                {
                    VAPILog.Error(e);
                }
            }

            int num = 0;
            foreach(VariantTierDef tierDef in validTiers)
            {
                if(tierDef.Tier == VariantTierIndex.AssignedAtRuntime)
                {
                    tierDef.Tier = (VariantTierIndex)(++num + 10);
                }
                if(tierToDef.ContainsKey(tierDef.Tier))
                {
                    VAPILog.Error($"Duplicate TierDef for tier {tierDef.Tier}");
                }
                else
                {
                    tierToDef.Add(tierDef.Tier, tierDef);
                }
            }
            return validTiers.ToArray();
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
