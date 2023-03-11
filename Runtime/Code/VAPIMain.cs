using BepInEx;
using Moonstorm;
using R2API.Utils;
using RoR2;
using VAPI.RuleSystem;

namespace VAPI
{
    /// <summary>
    /// VarianceAPI's Main class
    /// </summary>
    [BepInDependency(DebugToolkit.DebugToolkit.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(Moonstorm.MoonstormSharedUtils.GUID, BepInDependency.DependencyFlags.HardDependency)]
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
        public const string VERSION = "2.1.3";

        /// <summary>
        /// The instancee class of the Main class
        /// </summary>
        public static VAPIMain Instance { get; private set; }
        private void Awake()
        {
            Instance = this;

            new VAPILog(Logger);
            new VAPIAssets().Init();
            new VAPIConfig().Init();
            new VAPILang().Init();
            new VAPIContent().Init();

            ConfigurableFieldManager.AddMod(this);
            SystemInitializerInjector.InjectDependency<RuleBook>(typeof(RuleBookExtras));
        }
    }
}