using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VarianceAPI.Components;
using VarianceAPI.ScriptableObjects;

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
            if (VariantMaterialGrabber.vanillaMaterials.Count != 0)
            {
                VariantMaterialGrabber.SwapMaterials();
            }
            else
            {
                VAPILog.LogI("Variant Material Grabber's vanilla materials dictionary is empty, skipping swapping materials.");
            }
            if (RegisteredVariants.Count != 0)
            {
                VAPILog.LogI("Modifying CharacterBody prefabs...");
                foreach (var kvp in RegisteredVariants)
                {
                    List<string> builder = new List<string>();
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

                        spawnHandler.variantInfos = kvp.Value.ToArray();

                        builder.Add($"Added Variant related components to the bodyPrefab {kvp.Key}");

                        builder.Add($"Available {kvp.Key} variants:");
                        kvp.Value.ForEach(variant =>
                        {
                            builder.Add($"{variant}. || SpawnRate = {variant.spawnRate} || Unique = {variant.unique}");
                        });
                        VAPILog.LogD(string.Join("\n", builder));
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
        private static void AddVariant(VariantInfo variantInfo, ConfigFile configFile = null)
        {
            if (configFile != null)
            {
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

            if (!RegisteredVariants.ContainsKey(variantInfo.bodyName))
            {
                RegisteredVariants.Add(variantInfo.bodyName, new List<VariantInfo>());
            }

            if (!RegisteredVariants[variantInfo.bodyName].Contains(variantInfo))
            {
                RegisteredVariants[variantInfo.bodyName].Add(variantInfo);
                VAPILog.LogD($"Variant {variantInfo} succesfully added.");
            }
            else
            {
                VAPILog.LogW($"Tried to add a duplicate variantInfo: {variantInfo}");
            }
        }

        //Adds a single variantInfo to the registered variants dictionary.
        public static void AddSingleVariant(VariantInfo variantInfo, ConfigFile configFile = null)
        {
            if (!variantsRegistered)
            {
                if (configFile != null)
                    VAPILog.LogI($"Attempting to register {variantInfo} from {Assembly.GetCallingAssembly().GetName().Name} alongside IsUnique & SpawnRate Configurations...");
                else
                    VAPILog.LogI($"Attempting to register {variantInfo} from {Assembly.GetCallingAssembly().GetName().Name}");

                AddVariant(variantInfo, configFile);
            }
            else
            {
                VAPILog.LogW($"Tried to add VariantInfos from {Assembly.GetCallingAssembly().GetName().Name} after the variants have been registered. this is not allowed.\nVariants must be registered before RoR2Application.onLoad runs");
            }
        }

        //Adds all the variantInfos found in the AssetBundle.
        public static void AddVariant(AssetBundle assetBundle, ConfigFile configFile = null)
        {
            if (!variantsRegistered)
            {
                VariantInfo[] variantInfos = assetBundle.LoadAllAssets<VariantInfo>();

                if (configFile != null)
                    VAPILog.LogI($"Attempting to register {variantInfos.Length} variants from {Assembly.GetCallingAssembly().GetName().Name} alongside IsUnique & SpawnRate Configurations...");
                else
                    VAPILog.LogI($"Attempting to register {variantInfos.Length} variants from {Assembly.GetCallingAssembly().GetName().Name}");

                for (int i = 0; i < variantInfos.Length; i++)
                {
                    AddVariant(variantInfos[i], configFile);
                }
            }
            else
            {
                VAPILog.LogW($"Tried to add VariantInfos from {Assembly.GetCallingAssembly().GetName().Name} after the variants have been registered. this is not allowed.\nVariants must be registered before RoR2Application.onLoad runs");
            }
        }

        //Adds all the variantInfos inside a list of VariantInfos.
        public static void AddVariant(IEnumerable<VariantInfo> variantInfos, ConfigFile configFile = null)
        {
            if (!variantsRegistered)
            {
                if (configFile != null)
                    VAPILog.LogI($"Attempting to register {variantInfos.Count()} variants from {Assembly.GetCallingAssembly().GetName().Name} alongside IsUnique & SpawnRate Configurations...");
                else
                    VAPILog.LogI($"Attempting to register {variantInfos.Count()} variants from {Assembly.GetCallingAssembly().GetName().Name}");

                foreach (VariantInfo info in variantInfos)
                {
                    AddVariant(info, configFile);
                }
            }
            else
            {
                VAPILog.LogW($"Tried to add VariantInfos from {Assembly.GetCallingAssembly().GetName().Name} after the variants have been registered. this is not allowed.\nVariants must be registered before RoR2Application.onLoad runs");
            }
        }
        #endregion
    }
}
