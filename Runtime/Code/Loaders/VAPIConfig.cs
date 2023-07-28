using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Loaders;
using Moonstorm.Config;
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
        public override BaseUnityPlugin MainClass => VAPIMain.Instance;
        public override bool CreateSubFolder => true;

        /// <summary>
        /// The general config file
        /// </summary>
        public static ConfigFile generalConfig;
        internal static ConfigurableBool addVariantEvents;
        internal static ConfigurableBool showVariantRuleCategory;
        internal static ConfigurableBool enableArtifactOfVariance;
        internal static ConfigurableBool activateMeshReplacementSystem;
        internal static ConfigurableBool sendArrivalMesssages;
        internal static ConfigurableColor variantHealthBarColor;

        /// <summary>
        /// The rewards config file
        /// </summary>
        public static ConfigFile rewardsConfig;
        internal static ConfigurableBool enableRewards;
        internal static ConfigurableBool luckAffectsItemRewards;
        internal static ConfigurableBool itemRewardsSpawnOnPlayer;
        internal static ConfigurableFloat hiddenRealmsItemRollChance;

        public void Init()
        {
            generalConfig = CreateConfigFile(general, false);
            rewardsConfig = CreateConfigFile(rewards, false);

            SetConfigs();
        }
        private void SetConfigs()
        {
            addVariantEvents = MakeConfigurableBool(true, b =>
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

            showVariantRuleCategory = new ConfigurableBool(false)
            {
                Section = "General",
                Key = "Show Variant Rule Category",
                Description = "Uncovers the Variant rule category, allowing you to enable or disable variant spawning from the lobby.",
                ConfigFile = generalConfig,
            };

            enableArtifactOfVariance = new ConfigurableBool(true)
            {
                Section = "General",
                Key = "Enable Artifact of Variance",
                Description = "Wether the artifact of Variance is enabled",
                ConfigFile = generalConfig,
            }.AddOnConfigChanged(b =>
            {
                var ruleDef = RuleBookExtras.varianceArtifactRuleDef;
                ruleDef.FindChoice("On").excludeByDefault = !b;
                ruleDef.FindChoice("Off").excludeByDefault = !b;
                ruleDef.forceLobbyDisplay = b;

                InfiniteTower.AddOrRemoveWave(b);
            });
            

            activateMeshReplacementSystem = new ConfigurableBool(true)
            {
                Section = "General",
                Key = "Activate Mesh Replacecment Systems",
                Description = "Activates the Mesh Replacement System, allowing for some Variants to have different meshes.\nExtremely jank, may not work at all, and could tank performance.",
                ConfigFile = generalConfig
            };

            sendArrivalMesssages = new ConfigurableBool(true)
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

            enableRewards = new ConfigurableBool(true)
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

            luckAffectsItemRewards = new ConfigurableBool(false)
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

            itemRewardsSpawnOnPlayer = new ConfigurableBool(false)
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

            hiddenRealmsItemRollChance = new ConfigurableFloat(100f)
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
