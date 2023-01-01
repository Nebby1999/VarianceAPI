using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using RoR2;
using R2API.Utils;
using Moonstorm;
using VAPI.RuleSystem;

namespace VAPI
{
    [BepInDependency(DebugToolkit.DebugToolkit.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(Moonstorm.MoonstormSharedUtils.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class VAPIMain : BaseUnityPlugin
    {
        public const string GUID = "com.Nebby.VAPI";
        public const string MODNAME = "VarianceAPI";
        public const string VERSION = "2.0.0";

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