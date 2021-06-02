using System;
using System.Collections.Generic;
using UnityEngine;
using VarianceAPI.Scriptables;

namespace VarianceAPI.Modules
{
    /// <summary>
    /// A class for simplifying the registration of Variants created in Thunderkit.
    /// </summary>
    public class VariantRegisterBase
    {
        /// <summary>
        /// The Asset bundle where the VariantsInfo are located in.
        /// </summary>
        public AssetBundle assetBundle;

        /// <summary>
        /// All the VariantInfos located in the AssetBundle
        /// </summary>
        public VariantInfo[] variantInfos;

        /// <summary>
        /// The main loading method
        /// </summary>
        /// <param name="assets">This must be your custom assetBundle.</param>
        public virtual void Init(AssetBundle assets)
        {
            assetBundle = assets;
            if(assetBundle == null)
            {
                Debug.LogError("VarianceAPI: AssetBundle is null! Aborting");
                return;
            }
            variantInfos = assetBundle.LoadAllAssets<VariantInfo>();
            if(variantInfos == null)
            {
                Debug.LogError("VarianceAPI: VariantInfo array is empty! Aborting");
                return;
            }
            foreach(VariantInfo i in variantInfos)
            {
                if(i.isModded)
                {
                    Helpers.AddModdedVariant(i);
                }
                else
                {
                    Helpers.AddVariant(i);
                }
            }
        }
    }
}