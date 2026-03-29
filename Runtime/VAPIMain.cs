using BepInEx;
using R2API.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    [BepInDependency(MSU.MSUMain.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.AddressablesPlugin.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(GUID, MOD_NAME, VERSION)]
    public class VAPIMain : BaseUnityPlugin
    {
        public const string GUID = "com.Nebby.VAPI";
        public const string MOD_NAME = "VarianceAPI";
        public const string VERSION = "3.0.0";

        public static VAPIMain instance { get; private set; }

        private void Awake()
        {
            instance = this;

            new VAPILog(Logger);

            new VAPIContent();
        }
    }
}
