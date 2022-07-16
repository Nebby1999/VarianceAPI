using BepInEx.Configuration;
using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VAPI.Components;

namespace VAPI
{
    public static class VariantCatalog
    {
        public static int variantCount => registeredVariants.Length;
        public static bool Initialized { get; private set; } = false;

        private static Dictionary<ConfigFile, List<VariantDef>> unregisteredVariants = new Dictionary<ConfigFile, List<VariantDef>>();
        private static VariantDef[] registeredVariants = Array.Empty<VariantDef>();
        private static readonly Dictionary<string, VariantIndex> nameToIndex = new Dictionary<string, VariantIndex>();

        private static readonly Dictionary<BodyIndex, BodyVariantDefProvider> bodyIndexToDefProvider = new Dictionary<BodyIndex, BodyVariantDefProvider>();
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

        public static BodyVariantDefProvider GetBodyVariantDefProvider(BodyIndex index)
        {
            if(bodyIndexToDefProvider.TryGetValue(index, out var provider))
            {
                return provider;
            }
            return null;
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
            if(unregisteredVariants.Count <= 0)
            {
                VAPILog.Info($"No Variants where Added prior to VariantCatalog initialization, returning.");
                return;
            }

            nameToIndex.Clear();

            registeredVariants = RegisterVariants(unregisteredVariants).ToArray();
            unregisteredVariants = null;
            PopulateBodyIndexToVariants();

            VAPILog.Info("Variant Catalog Initialized");
            Initialized = true;
        }

        private static List<VariantDef> RegisterVariants(Dictionary<ConfigFile, List<VariantDef>> unregisteredVariants)
        {
            VAPILog.Info($"Trying to register a total of {unregisteredVariants.Values.SelectMany(k => k).Count()} Variants");
            List<VariantDef> variantsToRegister = new List<VariantDef>();

            foreach(KeyValuePair<ConfigFile, List<VariantDef>> configVariantsPair in unregisteredVariants)
            {
                ConfigFile config = configVariantsPair.Key;
                List<VariantDef> variants = configVariantsPair.Value;
                VAPILog.Debug($"Validating the following variants: {string.Join("\n", variants.Select(vd => vd.name))}");

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
            VAPILog.Info($"Final Variants Registered: {variantsToRegister.Count}");
            return variantsToRegister;
        }

        private static bool ValidateVariant(VariantDef variant)
        {
            try
            {
                if(string.IsNullOrEmpty(variant.name) || string.IsNullOrWhiteSpace(variant.name))
                {
                    VAPILog.Error($"Variant {variant} has no object name!");
                    return false;
                }

                if (!BodyCatalog.bodyNames.Contains(variant.bodyName))
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
                VAPILog.Debug($"Registering {variant} (Index: {index})");
                variant.VariantIndex = index;
                nameToIndex.Add(variant.name, index);
            }
            catch(Exception e)
            {
                VAPILog.Error($"Could not register variant {variant}: {e}");
            }
        }
        
        private static void PopulateBodyIndexToVariants()
        {
            VAPILog.Info("Creating BodyVariantDefProviders for registered variants");
            foreach(CharacterBody body in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                VariantDef[] variantsForBody = registeredVariants
                    .Where(vd => vd.bodyName.Equals(body.name, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if(variantsForBody.Length > 0)
                {
                    body.gameObject.AddComponent<BodyVariantManager>();
                    
                    if (VAPIConfig.enableRewards.Value)
                        body.gameObject.AddComponent<BodyVariantReward>();

                    bodyIndexToDefProvider.Add(body.bodyIndex, new BodyVariantDefProvider(variantsForBody, body.bodyIndex));

                    VAPILog.Debug($"Created a BodyVariantDefProvider for body {body.name}. (Variants: {string.Join("\n", variantsForBody.ToString())}");
                }
            }
        }

        internal static void ThrowIfNotInitialized()
        {
            if(!Initialized)
                throw new InvalidOperationException($"VariantCatalog not initialized");
        }

        internal static void ThrowIfInitialized()
        {
            if(Initialized)
                throw new InvalidOperationException("VariantCatalog already initialized.");
        }
        #endregion
    }
}
