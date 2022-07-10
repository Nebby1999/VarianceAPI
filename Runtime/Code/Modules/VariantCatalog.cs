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
    public static class VariantCatalog
    {
        public static int variantCount => registeredVariants.Length;
        public static bool Initialized { get; private set; } = false;

        private static Dictionary<ConfigFile, List<VariantDef>> unregisteredVariants = new Dictionary<ConfigFile, List<VariantDef>>();
        private static VariantDef[] registeredVariants = Array.Empty<VariantDef>();
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
        public static void AddVariants(AssetBundle assetBundle, [NotNull] ConfigFile configFile)
        {
            ThrowIfInitialized();

            if (configFile == null)
                throw new NullReferenceException("configFile");

            AddVariants(assetBundle.LoadAllAssets<VariantDef>(), configFile);
        }

        public static void AddVariants(IEnumerable<VariantDef> variants, [NotNull]ConfigFile configFile)
        {
            ThrowIfInitialized();
            
            if(configFile == null)
                throw new NullReferenceException("configFile");

            variants.ToList().ForEach(vd => AddVariant(vd, configFile));
        }

        public static void AddVariant(VariantDef variant, [NotNull]ConfigFile configFile)
        {
            ThrowIfInitialized();
            
            if(configFile == null)
                throw new NullReferenceException("configFile");

            if(!unregisteredVariants.ContainsKey(configFile))
            {
                unregisteredVariants.Add(configFile, new List<VariantDef>());
            }
            unregisteredVariants[configFile].Add(variant);
        }
        #endregion

        #region internal methods
        [SystemInitializer(typeof(BodyCatalog), typeof(VariantTierCatalog))]
        private static void SystemInitializer()
        {
            nameToIndex.Clear();

            foreach(KeyValuePair<ConfigFile, List<VariantDef>> configAndVariants in unregisteredVariants)
            {
                unregisteredVariants[configAndVariants.Key] = configAndVariants.Value.OrderBy(vd => vd.name).ToList();
            }

            registeredVariants = RegisterVariants(unregisteredVariants).ToArray();
            unregisteredVariants = null;
            Initialized = true;
        }

        private static List<VariantDef> RegisterVariants(Dictionary<ConfigFile, List<VariantDef>> unregisteredVariants)
        {
            List<VariantDef> variantsToRegister = new List<VariantDef>();

            foreach(KeyValuePair<ConfigFile, List<VariantDef>> configVariantsPair in unregisteredVariants)
            {
                ConfigFile config = configVariantsPair.Key;
                List<VariantDef> variants = configVariantsPair.Value;

                variants = variants.Where(ValidateVariant).ToList();
                ConfigureVariantsThatPassedFilter(config, variants);
                variantsToRegister.AddRange(variants);
            }

            variantsToRegister = variantsToRegister.OrderBy(vd => vd.name).ToList();
            int variantAmount = variantsToRegister.ToArray().Length;
            for (VariantIndex variantIndex = 0; (int)variantIndex < variantAmount; variantIndex++)
            {
                RegisterVariant(variantsToRegister[(int)variantIndex], variantIndex);
            }
            return variantsToRegister;
        }

        private static bool ValidateVariant(VariantDef variant)
        {
            try
            {
                if (!BodyCatalog.bodyNames.Contains(variant.name))
                {
                    VAPILog.Warning($"Variant {variant} tries to modify a body with the name {variant.name}, but no such body exists in the catalog.");
                    return false;
                }
                return true;
            }
            catch(Exception e)
            {
                VAPILog.Error($"Could not validate variant {variant}: {e}");
                return false;
            }
        }

        private static void ConfigureVariantsThatPassedFilter(ConfigFile configFile, List<VariantDef> variants)
        {
            foreach(VariantDef variant in variants)
            {
                try
                {
                    variant.spawnRate = BindInternal(variant,
                        "Spawn Rate",
                        variant.spawnRate,
                        $"Chance for the {variant.name} variant to spawn\n(Percentage, 0-100)");

                    variant.isUnique = BindInternal<bool>(variant,
                        "Is Unique",
                        variant.isUnique,
                        $"Wether or not {variant.name} is Unique");
                }
                catch(Exception e)
                {
                    VAPILog.Error($"Error Configuring Variant {variant}: {e}\n(ConfigFile: {configFile}, Variant: {variant})");
                }
            }

            T BindInternal<T>(VariantDef variant, string key, T val, string desc)
            {
                return configFile.Bind<T>($"{variant.bodyName} Variants",
                    $"{variant.name} {key}",
                    val,
                    desc).Value;
            }
        }

        private static void RegisterVariant(VariantDef variant, VariantIndex index)
        {
            try
            {
                variant.VariantIndex = index;
                nameToIndex.Add(variant.name, index);
            }
            catch(Exception e)
            {
                VAPILog.Error($"Could not register variant {variant}: {e}");
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
