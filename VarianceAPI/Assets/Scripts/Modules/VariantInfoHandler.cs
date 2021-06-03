using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VarianceAPI.Scriptables;

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
                Debug.LogError("VarianceAPI: AssetBundle is Null! Aborting.");
                return;
            }
            variantInfos = assetBundle.LoadAllAssets<VariantInfo>();
            if(variantInfos == null)
            {
                Debug.LogError("VarianceAPI: VariantInfo Array is Empty! Aborting.");
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

            Debug.Log("VarianceAPI: Creating config entries for " + variantInfo.identifierName);
            var variantSpawnRateConfig = config.Bind<float>(new ConfigDefinition("Config Entries for " + variantInfo.overrideName, variantInfo.overrideName + " Spawn Rate"), variantConfig.spawnRate, new ConfigDescription("Chance for the " + variantInfo.overrideName + " variant to Spawn\n(Percentage, 0-100)"));
            Debug.Log("VarianceAPI: Created SpawnRate config entry for " + variantInfo.identifierName);
            var variantIsUniqueConfig = config.Bind<bool>(new ConfigDefinition("Config Entries for " + variantInfo.overrideName, variantInfo.overrideName + " is Unique"), variantConfig.isUnique, new ConfigDescription("Wether or not " + variantInfo.overrideName + "is Unique"));
            Debug.Log("VarianceAPI: Created Is Unique config entry for " + variantInfo.identifierName);

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
                Helpers.AddModdedVariant(variantInfo);
            }
            else
            {
                Helpers.AddVariant(variantInfo);
            }
        }
    }
}
