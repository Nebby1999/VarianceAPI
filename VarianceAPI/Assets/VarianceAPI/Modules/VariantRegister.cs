﻿using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VarianceAPI.Components;
using VarianceAPI.ScriptableObjects;
using BepInEx.Configuration;

namespace VarianceAPI
{
    public static class VariantRegister
    {
        public static Dictionary<string, List<VariantInfo>> RegisteredVariants = new Dictionary<string, List<VariantInfo>>();

        public static void Initialize()
        {
            RoR2Application.onLoad += RegisterVariants;
            VAPILog.LogI("Subscribed to RoR2Application.onLoad");
        }

        private static void RegisterVariants()
        {
            VAPILog.LogI("Modifying CharacterBody prefabs...");
            foreach(var kvp in RegisteredVariants)
            {
                var bodyPrefab = BodyCatalog.FindBodyPrefab(kvp.Key);
                if((bool)bodyPrefab)
                {
                    Debug.Log("A");
                    var spawnHandler = bodyPrefab.AddComponent<VariantSpawnHandler>();
                    Debug.Log("B");
                    var variantHandler = bodyPrefab.AddComponent<VariantHandler>();
                    Debug.Log("C");
                    VariantRewardHandler rewardHandler = null;
                    Debug.Log("D");
                    if(ConfigLoader.VariantsGiveRewards.Value)
                    {
                        Debug.Log("E");
                        rewardHandler = bodyPrefab.AddComponent<VariantRewardHandler>();
                        Debug.Log("F");
                    }
                    Debug.Log("G");
                    spawnHandler.variantInfos = kvp.Value.ToArray();

                    VAPILog.LogI($"Added components {spawnHandler}, {variantHandler}, {rewardHandler} to the bodyPrefab {kvp.Key}");
                }
                else
                {
                    VAPILog.LogW($"Could not find a CharacterBody game object of name {kvp.Key}.");
                    continue;
                }
            }
        }
        #region AddVariant Methods
        //Adds a variantInfo to the registered variants dictionary.
        public static void AddVariant(VariantInfo variantInfo, ConfigFile configFile = null)
        {
            //Create config only if configFile is not null.
            if(configFile != null)
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

                VAPILog.LogI($"Created spawn rate and is unique configs for {variantInfo.identifier}");
            }

            if(!RegisteredVariants.ContainsKey(variantInfo.bodyName))
            {
                VAPILog.LogI($"Ass");
                RegisteredVariants.Add(variantInfo.bodyName, new List<VariantInfo>());
            }
            VAPILog.LogI("Ass2");
            RegisteredVariants[variantInfo.bodyName].Add(variantInfo);
            VAPILog.LogD($"Added {variantInfo.identifier} to the list for {variantInfo.bodyName}");
            /*else
            {
                VAPILog.LogW($"A VariantInfo with the identifier {variantInfo.identifier} has already been added to the VariantInfo list for the body of name {variantInfo.bodyName}. Aborting.");
                return;
            }*/
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
