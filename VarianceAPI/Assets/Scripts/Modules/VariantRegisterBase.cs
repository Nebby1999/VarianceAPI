using System;
using System.Collections.Generic;
using UnityEngine;
using VarianceAPI.Scriptables;

namespace VarianceAPI.Modules
{
    public class VariantRegisterBase
    {
        public AssetBundle assetBundle;
        public VariantInfo[] variantInfos;
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
                Debug.Log("VarianceAPI: Adding " + i.identifierName + " VariantHandler for the bodyPrefab of name " + i.bodyName + "Body!");
                Helpers.AddVariant(i);
            }
        }
    }
}