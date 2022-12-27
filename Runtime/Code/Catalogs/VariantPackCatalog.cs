﻿using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Moonstorm;

namespace VAPI
{
    public static class VariantPackCatalog
    {
        private struct ConfigPair
        {
            public readonly ConfigFile tierConfig;
            public readonly ConfigFile variantConfig;

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ConfigPair))
                    return false;

                ConfigPair other = (ConfigPair)obj;

                bool tierEquals = false;
                if(tierConfig != null && other.tierConfig != null)
                {
                    tierEquals = tierConfig.Equals(other.tierConfig);
                }
                else if(tierConfig == null && other.tierConfig == null)
                {
                    tierEquals = true; //Both are null, so true.
                }

                bool variantEquals = false;
                if (variantConfig != null && other.variantConfig != null)
                {
                    variantEquals = tierConfig.Equals(other.tierConfig);
                }
                else if (variantConfig == null && other.variantConfig == null)
                {
                    variantEquals = true; //Both are null, so true.
                }

                return variantEquals && tierEquals;
            }

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
        }

        public static int VariantPackCount => registeredPacks.Length;
        public static bool Initialized { get; private set; } = false;
        public static event Action OnPacksRegistered;

        private static Dictionary<ConfigPair, List<VariantPackDef>> unregisteredPacks = new Dictionary<ConfigPair, List<VariantPackDef>>();
        internal static VariantPackDef[] registeredPacks = Array.Empty<VariantPackDef>();
        private static readonly Dictionary<string, VariantPackIndex> nameToIndex = new Dictionary<string, VariantPackIndex>();

        #region Find Methods
        public static VariantPackDef GetVariantPackDef(VariantPackIndex variantPackIndex)
        {
            ThrowIfNotInitialized();
            return HG.ArrayUtils.GetSafe(registeredPacks, (int)variantPackIndex);
        }

        public static VariantPackIndex FindVariantPackIndex(string packName)
        {
            ThrowIfNotInitialized();
            if(nameToIndex.TryGetValue(packName, out var index))
            {
                return index;
            }
            return VariantPackIndex.None;
        }

        public static VariantPackDef FindVariantPackDef(VariantDef variant)
        {
            ThrowIfNotInitialized();
            foreach (VariantPackDef packDef in registeredPacks)
            {
                if(packDef.variants.Contains(variant))
                {
                    return packDef;
                }
            }
            return null;
        }

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

        #region Add Methods
        public static void AddVariantPacks(AssetBundle assetBundle)
        {
            ThrowIfInitialized();

            AddVariantPacks(assetBundle.LoadAllAssets<VariantPackDef>());
        }
        public static void AddVariantPacks(AssetBundle assetBundle, ConfigFile configFile)
        {
            ThrowIfInitialized();

            if (configFile == null)
                throw new ArgumentNullException("configFile");

            AddVariantPacks(assetBundle.LoadAllAssets<VariantPackDef>(), configFile);
        }
        public static void AddVariantPacks(AssetBundle assetBundle, ConfigFile tierConfig, ConfigFile variantConfig)
        {
            ThrowIfInitialized();

            if (tierConfig == null)
                throw new ArgumentNullException("tierConfig");

            if (variantConfig == null)
                throw new ArgumentNullException("variantConfig");

            AddVariantPacks(assetBundle.LoadAllAssets<VariantPackDef>(), tierConfig, variantConfig);
        }

        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks)
        {
            ThrowIfInitialized();

            foreach(VariantPackDef packDef in variantPacks)
            {
                AddVariantPack(packDef);
            }
        }

        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks, ConfigFile configFile)
        {
            ThrowIfInitialized();

            if (configFile == null)
                throw new ArgumentNullException("configFile");

            foreach (VariantPackDef packDef in variantPacks)
            {
                AddVariantPack(packDef, configFile);
            }
        }

        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks, ConfigFile tierConfig, ConfigFile variantConfig)
        {
            ThrowIfInitialized();

            if (tierConfig == null)
                throw new ArgumentNullException("tierConfig");

            if (variantConfig == null)
                throw new ArgumentNullException("variantConfig");

            foreach (VariantPackDef packDef in variantPacks)
            {
                AddVariantPack(packDef, tierConfig, variantConfig);
            }
        }

        public static void AddVariantPack(VariantPackDef packDef)
        {
            ThrowIfInitialized();

            AddPackInternal(packDef, default(ConfigPair));
        }

        public static void AddVariantPack(VariantPackDef packDef, ConfigFile configFile)
        {
            ThrowIfInitialized();

            if (configFile == null)
                throw new ArgumentNullException("configFile");

            AddPackInternal(packDef, new ConfigPair(configFile));
        }

        public static void AddVariantPack(VariantPackDef packDef, ConfigFile tierConfig, ConfigFile variantConfig)
        {
            ThrowIfInitialized();

            if (tierConfig == null)
                throw new ArgumentNullException("tierConfig");

            if (variantConfig == null)
                throw new ArgumentNullException("variantConfig");

            AddPackInternal(packDef, new ConfigPair(tierConfig, variantConfig));
        }

        private static void AddPackInternal(VariantPackDef packDef, ConfigPair configPair)
        {
            if(!unregisteredPacks.ContainsKey(configPair))
            {
                unregisteredPacks[configPair] = new List<VariantPackDef>();
            }
            unregisteredPacks[configPair].Add(packDef);
        }
        #endregion

        #region Internal Methods
        [SystemInitializer]
        private static void SystemInit()
        {
            nameToIndex.Clear();

            registeredPacks = RegisterPacks(unregisteredPacks);
            unregisteredPacks = null;
            Initialized = true;
            OnPacksRegistered?.Invoke();
        }

        private static VariantPackDef[] RegisterPacks(Dictionary<ConfigPair, List<VariantPackDef>> unregisteredPacks)
        {
            List<(VariantPackDef, ConfigPair)> packsToRegister = new List<(VariantPackDef, ConfigPair)>();

            foreach(var (configFile, packs) in unregisteredPacks)
            {
                var validatedPacks = new List<VariantPackDef>();
                validatedPacks = packs.Where(ValidatePack).ToList();

                packsToRegister.AddRange(validatedPacks.Select(x => (x, configFile)));
            }

            packsToRegister = packsToRegister.OrderBy(vpd => vpd.Item1.name).ToList();
            int packAmount = packsToRegister.ToArray().Length;
            for(VariantPackIndex packIndex = 0; (int)packIndex < packAmount; packIndex++)
            {
                RegisterPack(packsToRegister[(int)packIndex], packIndex);
            }
            VAPILog.Info($"Final Packs Registered: {packsToRegister.Count}");
            return packsToRegister.Select(x => x.Item1).ToArray();
        }

        private static bool ValidatePack(VariantPackDef packDef)
        {
            try
            {
                return true;
            }
            catch(Exception e)
            {
                VAPILog.Error($"Could not validate pack {packDef}: {e}");
                return false;
            }
        }

        private static void RegisterPack((VariantPackDef, ConfigPair) variantPack, VariantPackIndex index)
        {
            try
            {
                VariantPackDef packDef = variantPack.Item1;
                packDef.tierConfiguration = variantPack.Item2.tierConfig;
                packDef.variantConfiguration = variantPack.Item2.variantConfig;
                VAPILog.Debug($"Registering {variantPack} (Index: {index})");
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
            if (!Initialized)
                throw new InvalidOperationException("VariantPackCatalog not initialized");
        }

        private static void ThrowIfInitialized()
        {
            if (Initialized)
                throw new InvalidOperationException("VariantPackCatalog already initialized");
        }
        #endregion
    }
}