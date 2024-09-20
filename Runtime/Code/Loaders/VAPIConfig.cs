using BepInEx;
using BepInEx.Configuration;
using MSU.Config;
using RiskOfOptions.OptionConfigs;
using VAPI.RuleSystem;
using UnityEngine;
using VAPI.Modules;
using static MSU.Config.ConfiguredVariable;

namespace VAPI
{
    public class VAPIConfig
    {
        public const string PREFIX = "VAPI.";
        public const string GENERAL = PREFIX + "General";
        public const string REWARDS = PREFIX + "Rewards";

        internal static ConfigFactory configFactory { get; private set; }
        
        public static ConfigFile generalConfig { get; private set; }
        [AutoConfig]
        internal static ConfiguredBool _showVariantRuleCategory;
        [AutoConfig]
        internal static ConfiguredBool _enableArtifactOfVariance;
        [AutoConfig]
        internal static ConfiguredBool _activateMeshReplacementSystem;
        [AutoConfig]
        internal static ConfiguredBool _sendArrivalMesssages;
        [AutoConfig]
        internal static ConfiguredColor _variantHealthBarColor;

        public static ConfigFile rewardsConfig { get; private set; }
        [AutoConfig]
        internal static ConfiguredBool _enableRewards;
        [AutoConfig]
        internal static ConfiguredBool _luckAffectsItemRewards;
        [AutoConfig]
        internal static ConfiguredBool _itemRewardsSpawnOnPlayer;
        [AutoConfig]
        internal static ConfiguredFloat _hiddenRealmsItemRollChance;

        internal VAPIConfig(BaseUnityPlugin plugin)
        {
            configFactory = new ConfigFactory(plugin);
            generalConfig = configFactory.CreateConfigFile(GENERAL, false);
            rewardsConfig = configFactory.CreateConfigFile(GENERAL, false);

            SetConfigs();
        }

        private void SetConfigs()
        {
            _showVariantRuleCategory = new ConfiguredBool(false)
            {
                section = "General",
                key = "Show Variant Rule Category",
                description = "Uncovers the Variant rule category, allowing you to enable or disable variant spawning from the lobby.",
                configFile = generalConfig,
            };

            _enableArtifactOfVariance = new ConfiguredBool(true)
            {
                section = "General",
                key = "Enable Artifact of Variance",
                description = "Wether the artifact of Variance is enabled",
                configFile = generalConfig,
            }.WithConfigChange(b =>
            {
                var ruleDef = RuleBookExtras._varianceArtifactRuleDef;
                ruleDef.FindChoice("On").excludeByDefault = !b;
                ruleDef.FindChoice("Off").excludeByDefault = !b;
                ruleDef.forceLobbyDisplay = b;

                InfiniteTower.AddOrRemoveWave(b);
            });


            _activateMeshReplacementSystem = new ConfiguredBool(true)
            {
                section = "General",
                key = "Activate Mesh Replacecment Systems",
                description = "Activates the Mesh Replacement System, allowing for some Variants to have different meshes.\nExtremely jank, may not work at all, and could tank performance.",
                configFile = generalConfig
            };

            _sendArrivalMesssages = new ConfiguredBool(true)
            {
                section = "General",
                key = "Send Arrival Messages",
                description = "Wether variants which tier's send messages on arrival send said messages.",
                configFile = generalConfig,
            };

            _variantHealthBarColor = new ConfiguredColor(new Color32(0, 255, 144, byte.MaxValue))
            {
                section = "General",
                key = "Variant Healthbar Color",
                description = "The Healthbar Colour for Variants",
                configFile = generalConfig
            };

            _enableRewards = new ConfiguredBool(true)
            {
                section = "Rewards",
                key = "Activate Rewards Systems",
                description = "Activates the Rewards Systems, when enabled, variants drop extra gold and experiencee, alongside a chance for an item.",
                configFile = rewardsConfig,
                checkBoxConfig = new CheckBoxConfig
                {
                    restartRequired = true
                }
            };

            _luckAffectsItemRewards = new ConfiguredBool(false)
            {
                section = "Rewards",
                key = "Luck affects item rewards",
                description = "If true, the Luck stat will influence the chance for an Item Reward",
                configFile = rewardsConfig,
                checkBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableRewards
                }
            };

            _itemRewardsSpawnOnPlayer = new ConfiguredBool(false)
            {
                section = "Rewards",
                key = "Item Rewards Spawn on Player",
                description = "Setting this to true makes item rewards spawn on the player that dealt the killing blow to a variant, instead of the variant's position.",
                configFile = rewardsConfig,
                checkBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableRewards
                }
            };

            _hiddenRealmsItemRollChance = new ConfiguredFloat(100f)
            {
                section = "Rewards",
                key = "Chance for ItemDrops in Hidden Realms",
                description = "The chance for an Item drop in a hidden realm, this check must pass before the variant even has a chance to drop an item.\nSet this to 0 for no item drops in hidden realms.",
                configFile = rewardsConfig,
                sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                sliderConfig = new SliderConfig
                {
                    checkIfDisabled = () => !_enableRewards,
                    min = 0,
                    max = 100
                }
            };
        }
    }
}
