using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPI
{
    public static class VariantCatalog
    {
        public static int variantCount => registeredVariants.Length;
        public static bool Initialized { get; private set; } = false;

        private static VariantDef[] unregisteredVariants;
        private static VariantDef[] registeredVariants;
        private static readonly Dictionary<string, VariantIndex> nameToIndex = new Dictionary<string, VariantIndex>();

        #region Get Methods
        public static VariantDef GetVariantDef(VariantIndex variantIndex)
        {
            ThrowIfNotInitialized();
            return HG.ArrayUtils.GetSafe(registeredVariants, (int)variantIndex);
        }

        public static VariantIndex FindVariantIndex(string variantName)
        {
            ThrowIfNotInitialized();
            if(nameToIndex.TryGetValue(variantName, out VariantIndex index))
            {
                return index;
            }
            return VariantIndex.None;
        }
        #endregion

        #region Add methods
        public static void AddVariants(VariantDef[] variants)
        {
            ThrowIfInitialized();
            variants.ToList().ForEach(vd => AddVariant(vd));
        }

        public static void AddVariant(VariantDef variant)
        {
            ThrowIfInitialized();
            HG.ArrayUtils.ArrayAppend(ref unregisteredVariants, variant);
        }
        #endregion

        #region internal methods
        [SystemInitializer(typeof(BodyCatalog), typeof(VariantTierCatalog))]
        private static void SystemInitializer()
        {
            nameToIndex.Clear();

            unregisteredVariants = unregisteredVariants.OrderBy(vd => vd.name).ToArray();

            registeredVariants = RegisterVariants(unregisteredVariants).ToArray();
            unregisteredVariants = null;
            Initialized = true;
        }

        private static List<VariantDef> RegisterVariants(VariantDef[] variants)
        {
            List<VariantDef> validVariants = new List<VariantDef>();
            foreach(VariantDef v in variants)
            {
                try
                {
                    //validate
                    validVariants.Add(v);
                }
                catch(Exception e)
                {
                    VAPILog.Error($"{e}\n(VariantDef: {v})");
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
