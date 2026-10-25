#nullable enable
using BepInEx;
using BepInEx.Configuration;
using MSU.Config;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using AutoConfig = MSU.Config.ConfiguredVariable.AutoConfigAttribute;

namespace VAPI
{
    /// <summary>
    /// VarianceAPI's config class
    /// </summary>
    public sealed class VAPIConfig
    {
        /// <summary>
        /// Base prefix for VarianceAPI's config file
        /// </summary>
        public const string PREFIX = "VAPI.";
        /// <summary>
        /// Identifier for VarianceAPI's General config file
        /// </summary>
        public const string GENERAL = PREFIX + "General";
        /// <summary>
        /// Identifier for VarianceAPI's Rewards config file
        /// </summary>
        public const string REWARDS = PREFIX + "Rewards";

        internal static ConfigFactory? configFactory { get; private set; }

        /// <summary>
        /// VarianceAPI's general config file
        /// </summary>
        public static ConfigFile? generalConfig { get; private set; }
        [AutoConfig]
        internal static ConfiguredBool? _showVariantRuleCategory, _enableArtifactOfVariance, _activateMeshReplacementSystem, _sendArrivalMessages, _playArrivalSFX, _modifyGupDeathStates;
        [AutoConfig]
        internal static ConfiguredColor? _variantHealthBarColor;

        /// <summary>
        /// VarianceAPI's rewards config file
        /// </summary>
        public static ConfigFile? rewardsConfig { get; private set; }
        [AutoConfig]
        internal static ConfiguredBool? _enableRewards, _luckAffectsItemRewards, _itemRewardsSpawnOnKiller;
        [AutoConfig]
        internal static ConfiguredFloat? _hiddenRealmsRollChance;

        private bool _initialized;

        internal IEnumerator InitializeAsync(BaseUnityPlugin bup)
        {
            if (_initialized) //Don't initialize twice
                yield break;

            _initialized = true;

            configFactory = new ConfigFactory(bup);
            generalConfig = configFactory.CreateConfigFile(GENERAL, false);
            rewardsConfig = configFactory.CreateConfigFile(REWARDS, false);

            //Set stuff for ROO
            using(var iconRequest = VAPIAssets.LoadAssetAsync<Sprite>("texVAPIIcon"))
            {
                while(!iconRequest.isComplete)
                {
                    yield return null;
                }

                ModSettingsManager.SetModIcon(iconRequest.asset!, bup.Info.Metadata.GUID, bup.Info.Metadata.Name);
                ModSettingsManager.SetModDescription("General configuration for VarianceAPI.", bup.Info.Metadata.GUID, bup.Info.Metadata.Name);
            }

            //Initialize configs
            SetConfigs();
        }

        private void SetConfigs()
        {
            _showVariantRuleCategory = new ConfiguredBool(false)
            {
                section = "General",
                key = "Show Variant Rule Category",
                description = "Shows the Variant Rule Category, allowing you to enable or disable variant spawning from the lobby.",
                configFile = generalConfig,
            };

            _enableArtifactOfVariance = new ConfiguredBool(true)
            {
                section = "General",
                key = "Enable Artifact of Variance",
                description = "Wether the Artifact of Variance is shown in the Artifacts section",
                configFile = generalConfig,
                checkBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => Run.instance,
                }
            }.WithConfigChange(b =>
            {
                var ruleDef = VAPIRuleBook._varianceArtifactRuleDef;
                ruleDef.FindChoice("On").excludeByDefault = !b;
                ruleDef.FindChoice("Off").excludeByDefault = !b;

                InfiniteTower.AddOrRemoveWave(b);

                if (PreGameController.instance && NetworkServer.active)
                    PreGameController.instance.RecalculateModifierAvailability();
            });

            _activateMeshReplacementSystem = new ConfiguredBool(true)
            {
                section = "General",
                key = "Activate Mesh Replacement Systems",
                description = "Activates the Mesh Replacement System, allowing for some Variants to have different meshes.\nExtremely jank, may not work at all, and could tank performance.",
                configFile = generalConfig,
            };

            _sendArrivalMessages = new ConfiguredBool(true)
            {
                section = "General",
                key = "Send Arrival Messages",
                description = "Wether variants which tier's send messages on arrival send said messages.",
                configFile = generalConfig,
            };

            _playArrivalSFX = new ConfiguredBool(true)
            {
                section = "General",
                key = "Play Arrival Sound Effects",
                description = "Wether variants which tier's play sound effects on arrival play said sound effects.",
                configFile = generalConfig,
            };

            _modifyGupDeathStates = new ConfiguredBool(true)
            {
                section = "General",
                key = "Modify Gup/Geep Death States",
                description = "Modifies the Death state of Gup and Geep so that the split enemies retain some variant logic. For example, A variant gup will split into Geeps that only have the parent's variant defs. And a variant geep will not spawn from a normal gup.",
                configFile = generalConfig,
            }
            .WithConfigChange(b =>
            {   
                if(b)
                {
                    IL.EntityStates.Gup.BaseSplitDeath.FixedUpdate -= GupVariantHelper.HandleDeathState;
                    IL.EntityStates.Gup.BaseSplitDeath.FixedUpdate += GupVariantHelper.HandleDeathState;
                }
                else
                {
                    IL.EntityStates.Gup.BaseSplitDeath.FixedUpdate -= GupVariantHelper.HandleDeathState;
                }
            });

            _variantHealthBarColor = new ConfiguredColor(new Color32(0, 255, 144, byte.MaxValue))
            {
                section = "General",
                key = "Variant Healthbar Color",
                description = "The Healthbar Color for Variants",
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

            _itemRewardsSpawnOnKiller = new ConfiguredBool(false)
            {
                section = "Rewards",
                key = "Item Rewards Spawn on Killer",
                description = "Setting this to true makes item rewards spawn on the body that dealt the killing blow to a variant, instead of the variant's position. If the body has an owner (such as a drone), it'll spawn on top of said owner.",
                configFile = rewardsConfig,
                checkBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !_enableRewards
                }
            };

            _hiddenRealmsRollChance = new ConfiguredFloat(100f)
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