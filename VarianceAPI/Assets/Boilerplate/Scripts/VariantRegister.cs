using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Modules;
using VarianceAPI.Scriptables;

namespace YourPackNameHere
{
    /// <summary>
    /// This class contains methods and communicates with VarianceAPI
    /// <para>It's how you load your Variants from your AssetBundle using VarianceAPI's base classes</para>
    /// </summary>

    public class VariantRegister : VariantInfoHandler //<--- Do not remove this inheritance.
    {
        /// <summary>
        /// Calling this method registers all your Variants that are in the AssetBundle.
        /// <para>Does not work for Variants created in Code.</para>
        /// </summary>
        /// <param name="assets">This needs to be your Assetbundle</param>
        /// <param name="config">This needs to be your Mod's ConfigFile.</param>
        public void RegisterConfigs(AssetBundle assets, ConfigFile config)
        {
            Init(assets, config);
        }

        /// <summary>
        /// Calling this method registers all your Variants that where created in Code.
        /// <para>Only call this method where you're creating your Variants in code.</para>
        /// </summary>
        /// <param name="variantInfos">This needs to be a List of type VariantInfo that contains all your custom variants.</param>
        /// <param name="config">"This needs to be your Main Class' config file."</param>
        public void RegisterConfigs(List<VariantInfo> variantInfos, ConfigFile config)
        {
            Init(variantInfos, config);
        }
    }
}
