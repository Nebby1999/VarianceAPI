using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VarianceAPI.Scriptables;

namespace VarianceAPI.Modules
{
    public class VariantConfigBase
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
            variantConfigs = assetBundle.LoadAllAssets<VariantConfig>().ToList();
            if(variantConfigs == null)
            {
                Debug.LogError("VarianceAPI: VariantConfigs array is empty! Aborting.");
                return;
            }
            foreach(VariantInfo i in variantInfos)
            {
                GenerateConfigs(i, variantConfigs, config);
            }
        }
        private void GenerateConfigs(VariantInfo variantInfo, List<VariantConfig> variantConfigs, ConfigFile config)
        {
            var i = 1;
            foreach (VariantConfig varConfig in variantConfigs)
            {
                if(variantInfo.identifierName != varConfig.identifier)
                {
                    continue;
                }
                else if(variantInfo.identifierName == varConfig.identifier)
                {
                    i++;
                    variantInfo.spawnRate = varConfig.spawnRate;
                    variantInfo.unique = varConfig.isUnique;

                    Debug.Log("VarianceAPI: Creating config entries for " + variantInfo.identifierName);
                    config.Bind<float>(new ConfigDefinition("[" + i + "] " + "Config Entries for " + variantInfo.overrideName, variantInfo.overrideName + " Spawn Rate"), varConfig.spawnRate, new ConfigDescription("Chance for the " + variantInfo.overrideName + " variant to Spawn\n(Percentage, 0-100)"));
                    Debug.Log("VarianceAPI: Created SpawnRate config entry for " + variantInfo.identifierName);
                    config.Bind<bool>(new ConfigDefinition("[" + i + "] " + "Config Entries for " + variantInfo.overrideName, variantInfo.overrideName + " is Unique"), varConfig.isUnique, new ConfigDescription("Wether or not " + variantInfo.overrideName + "is Unique"));
                    variantConfigs.Remove(varConfig);
                }
            }
        }
    }
}
