#nullable enable
using BepInEx;
using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// VarianceAPI's main class
    /// </summary>
    [BepInDependency(MSU.MSUMain.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.AddressablesPlugin.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(DebugToolkit.DebugToolkit.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(GUID, MOD_NAME, VERSION)]
    public sealed class VAPIMain : BaseUnityPlugin
    {
        /// <summary>
        /// VarianceAPI's GUID. Which is "com.Nebby.VAPI"
        /// </summary>
        public const string GUID = "com.Nebby.VAPI";
        /// <summary>
        /// The Mod's name. Which is "VarianceAPI"
        /// </summary>
        public const string MOD_NAME = "VarianceAPI";
        /// <summary>
        /// The Mod's version
        /// </summary>
        public const string VERSION = "3.0.0";

        /// <summary>
        /// Access to the instance of VAPI's main class
        /// </summary>
        public static VAPIMain? instance { get; private set; }

        private void Awake()
        {
            instance = this;

            new VAPILog(Logger);

            new VAPIContent();


            SystemInitializerInjector.InjectDependency<RuleBook>(typeof(VAPIRuleBook));
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if <see cref="instance"/> is null
        /// </summary>
        public static void ThrowIfUninitialized()
        {
            if(!instance)
            {
                throw new InvalidOperationException("Do not interact with VarianceAPI before it's BaseUnityPlugin's Awake!!!");
            }
        }
    }
}
