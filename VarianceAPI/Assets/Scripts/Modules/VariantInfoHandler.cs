using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VarianceAPI.Scriptables;
using Logger = VarianceAPI.MainClass;

namespace VarianceAPI.Modules
{
    public class VariantInfoHandler
    {
        public AssetBundle assetBundle;
        public List<VariantConfig> variantConfigs;
        public VariantInfo[] variantInfos;

        public virtual void Init(AssetBundle assets, ConfigFile config)
        {
            assetBundle = assets;
            if(assetBundle == null)
            {
                Logger.Log.LogError("AssetBundle is Null! Aborting...");
                return;
            }
            variantInfos = assetBundle.LoadAllAssets<VariantInfo>();
            if(variantInfos == null)
            {
                Logger.Log.LogError("VariantInfo Array is Empty! Aborting...");
                return;
            }     
            foreach(VariantInfo i in variantInfos)
            {
                GenerateConfigs(i, config);
            }
        }
        private void GenerateConfigs(VariantInfo variantInfo, ConfigFile config)
        {
            var variantConfig = variantInfo.variantConfig;
            variantInfo.spawnRate = variantConfig.spawnRate;
            variantInfo.unique = variantConfig.isUnique;

            Logger.Log.LogMessage("Creating Config Entries for " + variantInfo.identifierName);
            var variantSpawnRateConfig = config.Bind<float>(new ConfigDefinition(variantInfo.bodyName + " Variants", variantInfo.identifierName + " Spawn Rate"), variantConfig.spawnRate, new ConfigDescription("Chance for the " + variantInfo.identifierName + " variant to Spawn\n(Percentage, 0-100)"));
            Logger.Log.LogMessage("Created SpawnRate config entry for " + variantInfo.identifierName);
            var variantIsUniqueConfig = config.Bind<bool>(new ConfigDefinition(variantInfo.bodyName + " Variants", variantInfo.identifierName + " is Unique"), variantConfig.isUnique, new ConfigDescription("Wether or not " + variantInfo.identifierName + " is Unique"));
            Logger.Log.LogMessage("Created IsUnique config entry for " + variantInfo.identifierName);

            variantInfo.spawnRate = variantSpawnRateConfig.Value;
            variantInfo.unique = variantIsUniqueConfig.Value;
            RegisterVariant(variantInfo);
        }
        public virtual void Init(List<VariantInfo> variantInfos, ConfigFile config)
        {
            foreach (VariantInfo i in variantInfos)
            {
                GenerateConfigs(i, config);
            }
        }
        public virtual void RegisterVariant(VariantInfo variantInfo)
        {
            if(variantInfo.isModded)
            {
                var checkForMod = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(variantInfo.variantConfig.modGUID);
                if(checkForMod)
                {
                    Helpers.AddModdedVariant(variantInfo);
                }
                else
                {
                    Logger.Log.LogError("Cannot add " + variantInfo.identifierName + " Modded Variant! youre missing a mod with the following GUID: " + variantInfo.variantConfig.modGUID);
                }
            }
            else
            {
                Helpers.AddVariant(variantInfo);
            }
        }
    }
}
