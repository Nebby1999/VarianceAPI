﻿using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Modules
{
    public static class ConfigLoader
    {
        // Global Settings
        internal static ConfigEntry<bool> VariantsGiveRewards;

        //VariantRewardHandler settings
        internal static ConfigEntry<bool> ItemRewardsSpawnsOnPlayer;
        internal static ConfigEntry<float> CommonVariantGoldMultiplier;
        internal static ConfigEntry<float> CommonVariantXPMultiplier;
        internal static ConfigEntry<float> CommonVariantWhiteItemDropChance;
        internal static ConfigEntry<float> CommonVariantGreenItemDropChance;
        internal static ConfigEntry<float> CommonVariantRedItemDropChance;

        internal static ConfigEntry<float> UncommonVariantGoldMultiplier;
        internal static ConfigEntry<float> UncommonVariantXPMultiplier;
        internal static ConfigEntry<float> UncommonVariantWhiteItemDropChance;
        internal static ConfigEntry<float> UncommonVariantGreenItemDropChance;
        internal static ConfigEntry<float> UncommonVariantRedItemDropChance;

        internal static ConfigEntry<float> RareVariantGoldMultiplier;
        internal static ConfigEntry<float> RareVariantXPMultiplier;
        internal static ConfigEntry<float> RareVariantWhiteItemDropChance;
        internal static ConfigEntry<float> RareVariantGreenItemDropChance;
        internal static ConfigEntry<float> RareVariantRedItemDropChance;

        internal static ConfigEntry<float> LegendaryVariantGoldMultiplier;
        internal static ConfigEntry<float> LegendaryVariantXPMultiplier;
        internal static ConfigEntry<float> LegendaryVariantWhiteItemDropChance;
        internal static ConfigEntry<float> LegendaryVariantGreenItemDropChance;
        internal static ConfigEntry<float> LegendaryVariantRedItemDropChance;
        public static void SetupConfigLoader(ConfigFile config)
        {
            //Global Settings
            VariantsGiveRewards = config.Bind<bool>("1 - Global Settings", "All Variants Give Rewards", true, "When this is set to True, all variants who have the \"givesRewards\" bool have extra rewards.\nVariants who dont have the givesRewards bool active never drop rewards");

            //VariantRewardHandlerSettings
            ItemRewardsSpawnsOnPlayer = config.Bind<bool>("2 - VariantRewardHandler Settings", "Item Rewards Spawns on Player", false, "Normally the item reward's droplet spawns from the center of the slain Variant.\nThis can cause some issues with killing Variants that are on top of the death plane, or get knocked back onto it, Since the item will be lost in the process.\nSetting this to True causes all Item Rewards to be spawned at the center of the Player who killed the variant.");
            InitializeVariantRewardsHandlerConfigs(config);
        }
        public static void InitializeVariantRewardsHandlerConfigs(ConfigFile config)
        {
            CommonVariantGoldMultiplier = DeathRewardConfig("Gold", config, 1.3f, "Common");
            CommonVariantXPMultiplier = DeathRewardConfig("XP", config, 1.3f, "Common");
            CommonVariantWhiteItemDropChance = ItemRewardConfig(config, 3f, "Common", "White");
            CommonVariantGreenItemDropChance = ItemRewardConfig(config, 0f, "Common", "Green");
            CommonVariantRedItemDropChance = ItemRewardConfig(config, 0f, "Common", "Red");

            UncommonVariantGoldMultiplier = DeathRewardConfig("Gold", config, 1.6f, "Uncommon");
            UncommonVariantXPMultiplier = DeathRewardConfig("XP", config, 1.6f, "Uncommon");
            UncommonVariantWhiteItemDropChance = ItemRewardConfig(config, 5f, "Uncommon", "White");
            UncommonVariantGreenItemDropChance = ItemRewardConfig(config, 1f, "Uncommon", "Green");
            UncommonVariantRedItemDropChance = ItemRewardConfig(config, 0f, "Uncommon", "Red");

            RareVariantGoldMultiplier = DeathRewardConfig("Gold", config, 2.0f, "Rare");
            RareVariantXPMultiplier = DeathRewardConfig("XP", config, 2.0f, "Rare");
            RareVariantWhiteItemDropChance = ItemRewardConfig(config, 10f, "Rare", "White");
            RareVariantGreenItemDropChance = ItemRewardConfig(config, 5f, "Rare", "Green");
            RareVariantRedItemDropChance = ItemRewardConfig(config, 1f, "Rare", "Red");

            LegendaryVariantGoldMultiplier = DeathRewardConfig("Gold", config, 3.0f, "Legendary");
            LegendaryVariantXPMultiplier = DeathRewardConfig("Gold", config, 3.0f, "Legendary");
            LegendaryVariantWhiteItemDropChance = ItemRewardConfig(config, 25f, "Legendary", "White");
            LegendaryVariantGreenItemDropChance = ItemRewardConfig(config, 10f, "Legendary", "Green");
            LegendaryVariantRedItemDropChance = ItemRewardConfig(config, 5f, "Legendary", "Red");
        }

        private static ConfigEntry<float> DeathRewardConfig(string rewardType, ConfigFile config, float defaultValue, string variantTier)
        {
            if(rewardType == "Gold")
            {
                return config.Bind<float>(new ConfigDefinition("2 - VariantRewardHandler Settings", variantTier + " Variant's " + rewardType + " Multiplier"), defaultValue, new ConfigDescription("Multiplier that's applied to the Gold reward for killing a " + variantTier + "Variant.\n(Set this value to 1.0 to disable)"));
            }
            else if(rewardType == "XP")
            {
                return config.Bind<float>(new ConfigDefinition("2 - VariantRewardHandler Settings", variantTier + " Variant's " + rewardType + " Multiplier"), defaultValue, new ConfigDescription("Multiplier that's applied to the XP reward for killing a " + variantTier + "Variant.\n(Set this value to 1.0 to disable)"));
            }
            Debug.LogError("Variance API: DeathRewardConfig's rewardType goes out of bounds!");
            return null;
        }

        private static ConfigEntry<float> ItemRewardConfig(ConfigFile config, float defaultValue, string variantTier, string itemTier)
        {
            return config.Bind<float>(new ConfigDefinition("2 - VariantRewardHandler Settings", variantTier + " Variant's " + itemTier + " Item Drop Chance"), defaultValue, new ConfigDescription("The chance for a " + variantTier + " Variant drops a single " + itemTier + " Item on death.\nAccepted values range from 0 to 100.\n(Set this value to 0 to Disable)"));
        }
    }
}
