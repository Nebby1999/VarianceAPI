using BepInEx.Configuration;
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
        public static int VariantPackCount => registeredPacks.Length;
        public static bool Initialized { get; private set; } = false;
        public static event Action OnPacksRegistered;

        private static Dictionary<ConfigFile, List<VariantPackDef>> unregisteredPacks = new Dictionary<ConfigFile, List<VariantPackDef>>();
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
        public static void AddVariantPacks(AssetBundle assetBundle, ConfigFile configFile)
        {
            ThrowIfInitialized();

            if (configFile == null)
                throw new ArgumentNullException("configFile");

            AddVariantPacks(assetBundle.LoadAllAssets<VariantPackDef>(), configFile);
        }

        public static void AddVariantPacks(IEnumerable<VariantPackDef> variantPacks, ConfigFile configFile)
        {
            ThrowIfInitialized();

            if (configFile == null)
                throw new ArgumentNullException("configFile");

            foreach(VariantPackDef packDef in variantPacks)
            {
                AddVariantPack(packDef, configFile);
            }
        }

        public static void AddVariantPack(VariantPackDef packDef, ConfigFile configFile)
        {
            ThrowIfInitialized();

            if (configFile == null)
                throw new ArgumentNullException("configFile");

            if(!unregisteredPacks.ContainsKey(configFile))
            {
                unregisteredPacks[configFile] = new List<VariantPackDef>();
            }
            unregisteredPacks[configFile].Add(packDef);
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

        private static VariantPackDef[] RegisterPacks(Dictionary<ConfigFile, List<VariantPackDef>> unregisteredPacks)
        {
            List<(VariantPackDef, ConfigFile)> packsToRegister = new List<(VariantPackDef, ConfigFile)>();

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
                if(!packDef.packIcon)
                {
                    throw new NullReferenceException($"{packDef} doesnt contain a pack icon! the field packIcon is null.");
                }
                return true;
            }
            catch(Exception e)
            {
                VAPILog.Error($"Could not validate pack {packDef}: {e}");
                return false;
            }
        }

        private static void RegisterPack((VariantPackDef, ConfigFile) variantPack, VariantPackIndex index)
        {
            try
            {
                VariantPackDef packDef = variantPack.Item1;
                packDef.configurationFile = variantPack.Item2;
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