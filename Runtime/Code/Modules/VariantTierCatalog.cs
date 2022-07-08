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
        public static bool Initialized { get; private set; }

        private static VariantTierDef[] unregisteredTiers;
        private static VariantTierDef[] registeredTiers;
        private static readonly Dictionary<string, VariantTierIndex> nameToIndex = new Dictionary<string, VariantTierIndex>();

        #region Get Methods
        public static VariantTierDef GetVariantDef(VariantTierIndex tierIndex)
        {
            ThrowIfNotInitialized();
            return HG.ArrayUtils.GetSafe(registeredTiers, (int)tierIndex);
        }

        public static VariantTierIndex FindVariantIndex(string variantName)
        {
            if(nameToIndex.TryGetValue(variantName, out VariantTierIndex index))
            {
                return index;
            }
            return VariantTierIndex.None;
        }
        #endregion

        #region Add methods
        public static void AddVariantTiers(VariantTierDef[] tierDefs)
        {
            ThrowIfInitialized();
            tierDefs.ToList().ForEach(vd => AddVariant(vd));
        }

        public static void AddVariant(VariantTierDef tierDef)
        {
            ThrowIfInitialized();
            HG.ArrayUtils.ArrayAppend(ref tierDef, tierDef);
        }
        #endregion

        #region internal methods
        [SystemInitializer(typeof(BodyCatalog))]
        private static void SystemInitializer()
        {
            nameToIndex.Clear();

            unregisteredTiers = unregisteredTiers.OrderBy(vd => vd.name).ToArray();

            registeredTiers = RegisterVariants(unregisteredTiers).ToArray();
            unregisteredTiers = null;
        }

        private static List<VariantTierDef> RegisterVariants(VariantTierDef[] tiers)
        {
            List<VariantTierDef> validVariants = new List<VariantTierDef>();
            for(int i = 0; i < tiers.Length; i++)
            {
                try
                {
                    VariantTierDef variant = tiers[i];
                }
                catch(Exception e)
                {
                    VAPILog.Error($"{e}\n(VariantDef: {tiers[i]})");
                }
            }

            int variantAmount = validVariants.ToArray().Length;
            for(VariantIndex variantIndex = (VariantIndex)0; (int)variantIndex < variantAmount; variantIndex++)
            {
                try
                {
                    RegisterVariant(validVariants[(int)variantIndex], variantIndex);
                }
                catch(Exception e)
                {
                    VAPILog.Error(e);
                }
            }

            return validVariants;
        }

        private static void RegisterVariant(VariantDef variant, VariantIndex index)
        {
            variant.VariantIndex = index;
            nameToIndex.Add(variant.name, index);
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
