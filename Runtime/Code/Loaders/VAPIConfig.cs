using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Loaders;

namespace VAPI
{
    public class VAPIConfig : ConfigLoader<VAPIConfig>
    {
        public const string general = "VAPI.General";
        public const string rewards = "VAPI.Rewards";
        public override BaseUnityPlugin MainClass => VAPIMain.Instance;
        public override bool CreateSubFolder => true;

        public static ConfigFile generalConfig;
        internal static ConfigEntry<bool> enableDebugFeatures;

        public static ConfigFile rewardsConfig;
        public void Init()
        {
            generalConfig = CreateConfigFile(general, false);
            rewardsConfig = CreateConfigFile(rewards, false);
        }

        private void SetConfigs()
        {
            enableDebugFeatures = generalConfig.Bind<bool>("VarianceAPI :: Debug Features",
                "Enable Debug",
                false,
                "Enables Debug systems for VarianceAPI.");
        }
    }
}
