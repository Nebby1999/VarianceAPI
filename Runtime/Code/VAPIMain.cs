using BepInEx;
using MSU;
using MSU.Config;
using R2API.Utils;
using RoR2;
using VAPI.RuleSystem;
using RiskOfOptions;
using UnityEngine;
using VAPI.Modules;

namespace VAPI
{
    /// <summary>
    /// VarianceAPI's Main class
    /// </summary>
    [BepInDependency(DebugToolkit.DebugToolkit.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MSU.MSUMain.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.AddressablesPlugin.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class VAPIMain : BaseUnityPlugin
    {
        /// <summary>
        /// VAPI's Main GUID
        /// </summary>
        public const string GUID = "com.Nebby.VAPI";
        /// <summary>
        /// VAPI's Mod name
        /// </summary>
        public const string MODNAME = "VarianceAPI";
        /// <summary>
        /// VAPI's Version
        /// </summary>
        public const string VERSION = "2.2.0";

        /// <summary>
        /// The instancee class of the Main class
        /// </summary>
        public static VAPIMain instance { get; private set; }
        private void Awake()
        {
            instance = this;
            new VAPIConfig().Init();

            new VAPILog(Logger);
            new VAPIAssets().Init();
            new VAPILang().Init();
            new VAPIContent().Init();

            ModSettingsManager.SetModIcon(VAPIAssets.LoadAsset<Sprite>("ExpansionIcon"));
            SystemInitializerInjector.InjectDependency<RuleBook>(typeof(RuleBookExtras));
            InfiniteTower.Init();
        }
    }
}