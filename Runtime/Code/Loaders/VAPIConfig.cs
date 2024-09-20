using BepInEx;
using BepInEx.Configuration;
using MSU.Loaders;
using MSU.Config;
using RiskOfOptions.OptionConfigs;
using VAPI.RuleSystem;
using UnityEngine;
using VAPI.Modules;

namespace VAPI
{
    /// <summary>
    /// VAPI's ConfigLoader
    /// </summary>
    public class VAPIConfig : ConfigLoader<VAPIConfig>
    {
        /// <summary>
        /// general config's identifier
        /// </summary>
        public const string general = "VAPI.General";
        /// <summary>
        /// Rewards config identifier
        /// </summary>
        public const string rewards = "VAPI.Rewards";
        public override BaseUnityPlugin MainClass => VAPIMain.instance;
        public override bool CreateSubFolder => true;

        /// <summary>
        /// The general config file
        /// </summary>
        public static ConfigFile generalConfig;
        internal static ConfiguredBool addVariantEvents;
        internal static ConfiguredBool showVariantRuleCategory;
        internal static ConfiguredBool enableArtifactOfVariance;
        internal static ConfiguredBool activateMeshReplacementSystem;
        internal static ConfiguredBool sendArrivalMesssages;
        internal static ConfigurableColor variantHealthBarColor;

        /// <summary>
        /// The rewards config file
        /// </summary>
        public static ConfigFile rewardsConfig;
        internal static ConfiguredBool enableRewards;
        internal static ConfiguredBool luckAffectsItemRewards;
        internal static ConfiguredBool itemRewardsSpawnOnPlayer;
        internal static ConfiguredFloat hiddenRealmsItemRollChance;

        public void Init()
        {
            generalConfig = CreateConfigFile(general, false);
            rewardsConfig = CreateConfigFile(rewards, false);

            SetConfigs();
        }
        private void SetConfigs()
        {
            addVariantEvents = MakeConfiguredBool(true, b =>
            {
                b.ConfigFile = generalConfig;
                b.Section = "General";
                b.Key = "Add Variant Events";
                b.Description = "Adds Variant related Events using the MSU Event Director";
                b.CheckBoxConfig = new CheckBoxConfig
                {
                    restartRequired = true
                };
            }).DoConfigure();

            showVariantRuleCategory = new ConfiguredBool(false)
            {
                Section = "General",
                Key = "Show Variant Rule Category",
                Description = "Uncovers the Variant rule category, allowing you to enable or disable variant spawning from the lobby.",
                ConfigFile = generalConfig,
            };

            enableArtifactOfVariance = new ConfiguredBool(true)
            {
                Section = "General",
                Key = "Enable Artifact of Variance",
                Description = "Wether the artifact of Variance is enabled",
                ConfigFile = generalConfig,
            }.AddOnConfigChanged(b =>
            {
                var ruleDef = RuleBookExtras._varianceArtifactRuleDef;
                ruleDef.FindChoice("On").excludeByDefault = !b;
                ruleDef.FindChoice("Off").excludeByDefault = !b;
                ruleDef.forceLobbyDisplay = b;

                InfiniteTower.AddOrRemoveWave(b);
            });
            

            activateMeshReplacementSystem = new ConfiguredBool(true)
            {
                Section = "General",
                Key = "Activate Mesh Replacecment Systems",
                Description = "Activates the Mesh Replacement System, allowing for some Variants to have different meshes.\nExtremely jank, may not work at all, and could tank performance.",
                ConfigFile = generalConfig
            };

            sendArrivalMesssages = new ConfiguredBool(true)
            {
                Section = "General",
                Key = "Send Arrival Messages",
                Description = "Wether variants which tier's send messages on arrival send said messages.",
                ConfigFile = generalConfig,
            };

            variantHealthBarColor = new ConfigurableColor(new Color32(0, 255, 144, byte.MaxValue))
            {
                Section = "General",
                Key = "Variant Healthbar Color",
                Description = "The Healthbar Colour for Variants",
                ConfigFile = generalConfig
            };

            enableRewards = new ConfiguredBool(true)
            {
                Section = "Rewards",
                Key = "Activate Rewards Systems",
                Description = "Activates the Rewards Systems, when enabled, variants drop extra gold and experiencee, alongside a chance for an item.",
                ConfigFile = rewardsConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    restartRequired = true
                }
            };

            luckAffectsItemRewards = new ConfiguredBool(false)
            {
                Section = "Rewards",
                Key = "Luck affects item rewards",
                Description = "If true, the Luck stat will influence the chance for an Item Reward",
                ConfigFile = rewardsConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !enableRewards
                }
            };

            itemRewardsSpawnOnPlayer = new ConfiguredBool(false)
            {
                Section = "Rewards",
                Key = "Item Rewards Spawn on Player",
                Description = "Setting this to true makes item rewards spawn on the player that dealt the killing blow to a variant, instead of the variant's position.",
                ConfigFile = rewardsConfig,
                CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !enableRewards
                }
            };

            hiddenRealmsItemRollChance = new ConfiguredFloat(100f)
            {
                Section = "Rewards",
                Key = "Chance for ItemDrops in Hidden Realms",
                Description = "The chance for an Item drop in a hidden realm, this check must pass before the variant even has a chance to drop an item.\nSet this to 0 for no item drops in hidden realms.",
                ConfigFile = rewardsConfig,
                UseStepSlider = false,
                SliderConfig = new SliderConfig
                {
                    checkIfDisabled = () => !enableRewards,
                    min = 0,
                    max = 100
                }
            };
        }
    }
}
