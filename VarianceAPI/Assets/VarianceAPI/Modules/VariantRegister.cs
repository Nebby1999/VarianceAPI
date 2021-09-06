using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VarianceAPI.Components;
using VarianceAPI.ScriptableObjects;
using BepInEx.Configuration;
using R2API;
using UnityEditor;

namespace VarianceAPI
{
    public static class VariantRegister
    {
        public static Dictionary<string, List<VariantInfo>> RegisteredVariants = new Dictionary<string, List<VariantInfo>>();

        public static bool variantsRegistered = false;

        public static void Initialize()
        {
            RoR2Application.onLoad += RegisterVariants;
            VAPILog.LogI("Subscribed to RoR2Application.onLoad");
        }

        private static void RegisterVariants()
        {
            variantsRegistered = true;
            if(VariantMaterialGrabber.vanillaMaterials.Count != 0)
            {
                VariantMaterialGrabber.SwapMaterials();
            }
            else
            {
                VAPILog.LogI("Variant Material Grabber's vanilla materials dictionary is empty, skipping swapping materials.");
            }
            if(RegisteredVariants.Count != 0)
            {
                VAPILog.LogI("Modifying CharacterBody prefabs...");
                foreach (var kvp in RegisteredVariants)
                {
                    var bodyPrefab = BodyCatalog.FindBodyPrefab(kvp.Key);
                    if ((bool)bodyPrefab)
                    {
                        var spawnHandler = bodyPrefab.AddComponent<VariantSpawnHandler>();
                        var variantHandler = bodyPrefab.AddComponent<VariantHandler>();
                        VariantRewardHandler rewardHandler = null;
                        if (ConfigLoader.VariantsGiveRewards.Value)
                        {
                            rewardHandler = bodyPrefab.AddComponent<VariantRewardHandler>();
                        }

                        spawnHandler.VariantInfos = kvp.Value.ToArray();

                        VAPILog.LogI($"Added components {spawnHandler.GetType().Name}, {variantHandler.GetType().Name}, {rewardHandler.GetType().Name} to the bodyPrefab {kvp.Key}");
                        VAPILog.LogD($"Available {kvp.Key} variants:");
                        kvp.Value.ForEach(variant =>
                        {
                            VAPILog.LogD($"{variant}. Unique? {variant.unique}");
                        });
                    }
                    else
                    {
                        VAPILog.LogW($"Could not find a CharacterBody game object of name {kvp.Key}.");
                        continue;
                    }
                }
            }
            else
            {
                VAPILog.LogI("No variants where registered, skipping modifying any bodies inside the body catalog.");
            }
            
        }
        #region AddVariant Methods
        //Adds a variantInfo to the registered variants dictionary.
        public static void AddVariant(VariantInfo variantInfo, ConfigFile configFile = null)
        {
            if(!variantsRegistered)
            {
                //Create config only if configFile is not null.
                if(configFile != null)
                {
                    VAPILog.LogI($"Creating spawn rate and is unique configs for {variantInfo.identifier}");
                    var spawnRate = configFile.Bind<float>(
                        $"{variantInfo.bodyName} Variants",
                        $"{variantInfo.identifier} Spawn Rate",
                        variantInfo.spawnRate,
                        $"Chance for the {variantInfo.identifier} variant to spawn\n(Percentage, 0-100)");

                    var isUnique = configFile.Bind<bool>(
                        $"{variantInfo.bodyName} Variants",
                        $"{variantInfo.identifier} is Unique",
                        variantInfo.unique,
                        $"Wether or not {variantInfo.identifier} is Unique");

                    variantInfo.spawnRate = spawnRate.Value;
                    variantInfo.unique = isUnique.Value;
                }

                if(!RegisteredVariants.ContainsKey(variantInfo.bodyName))
                {
                    RegisteredVariants.Add(variantInfo.bodyName, new List<VariantInfo>());
                }
                RegisteredVariants[variantInfo.bodyName].Add(variantInfo);
                VAPILog.LogD($"Added {variantInfo.identifier} to the list for {variantInfo.bodyName}");
            }
            else
            {
                VAPILog.LogI($"Tried to add {variantInfo.identifier} as a variant after the variants have been registered. this is not allowed.\nVariants must be registered before RoR2Application.onLoad runs");
            }
        }

        //Adds all the variantInfos found in the AssetBundle.
        public static void AddVariant(AssetBundle assetBundle, ConfigFile configFile = null)
        {
            VariantInfo[] variantInfos = assetBundle.LoadAllAssets<VariantInfo>();
            for(int i = 0; i < variantInfos.Length; i++)
            {
                AddVariant(variantInfos[i], configFile);
            }
        }

        //Adds all the variantInfos inside a list of VariantInfos.
        public static void AddVariant(IEnumerable<VariantInfo> variantInfos, ConfigFile configFile = null)
        {
            foreach(VariantInfo info in variantInfos)
            {
                AddVariant(info, configFile);
            }
        }
        #endregion
    }
}
