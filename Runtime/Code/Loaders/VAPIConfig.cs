using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Loaders;

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
        internal static ConfigEntry<bool> showVariantRuleCategory;
        internal static ConfigEntry<bool> enableArtifactOfVariance;
        internal static ConfigEntry<bool> activateMeshReplacementSystem;
        internal static ConfigEntry<bool> sendArrivalMesssages;

        /// <summary>
        /// The rewards config file
        /// </summary>
        public static ConfigFile rewardsConfig;
        internal static ConfigEntry<bool> enableRewards;
        internal static ConfigEntry<bool> luckAffectsItemRewards;
        internal static ConfigEntry<bool> itemRewardsSpawnOnPlayer;
        internal static ConfigEntry<float> hiddenRealmsItemRollChance;

        public void Init()
        {
            generalConfig = CreateConfigFile(general, false);
            rewardsConfig = CreateConfigFile(rewards, false);

            SetConfigs();
        }

        internal static bool HiddenTestVariantRules() => showVariantRuleCategory.Value;
        private void SetConfigs()
        {
            showVariantRuleCategory = generalConfig.Bind<bool>("VarianceAPI :: General",
                "Show Variant Rule Category",
                false,
                "Uncovers the Variant rule category, allowing you to enable or disable variant spawning from the lobby.");

            enableArtifactOfVariance = generalConfig.Bind<bool>("VarianceAPI :: General",
                "Enable Artifact of Variance",
                true,
                "Wether the Artifact of Variance is enabled");

            activateMeshReplacementSystem = generalConfig.Bind<bool>("VarianceAPI :: General",
                "Activate Mesh Replacement Systems",
                false,
                "Activates the Mesh Replacement System, allowing for Variants to have different meshes.\nExtremely jank, may not work at all, and tanks performance.");

            sendArrivalMesssages = generalConfig.Bind<bool>("VarianceAPI :: General",
                "Send Arrival Messages",
                true,
                "Wether variants which tier's send messages on arrival send said messages.");

            enableRewards = rewardsConfig.Bind<bool>("VarianceAPI :: Rewards",
                "Activate Rewards Systems",
                true,
                "Activates the Rewards Systems, when enabled, variants drop extra gold and experience, alongside a chance for an item.");

            luckAffectsItemRewards = rewardsConfig.Bind<bool>("VarianceAPI :: Rewards",
                "Luck affects item rewards",
                false,
                "If true, the Luck stat will ifluence the chance for an Item Reward");

            itemRewardsSpawnOnPlayer = rewardsConfig.Bind<bool>("VarianceAPI :: Rewards",
                "Item Rewards Spawn On Player",
                false,
                "Setting this to true makes item rewards spawn on the player that dealt the killing blow, instead of the victim's position.");

            hiddenRealmsItemRollChance = rewardsConfig.Bind<float>("VarianceAPI :: Rewards",
                "Chance for ItemDrops in Hidden Realms",
                100,
                "The chance for an ItemDrop in a hidden realm, this check must pass before the variant even has a chance to drop an item." +
                "\nSet this to 0 for no item drops in hidden realms.");
        }
    }
}
