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
                Debug.LogError("AssetBundle is null!");
                return;
            }
            variantInfos = assetBundle.LoadAllAssets<VariantInfo>();
            if(variantInfos == null)
            {
                Debug.LogError("VariantInfo array is empty!");
                return;
            }
            foreach(VariantInfo i in variantInfos)
            {
                Debug.Log("Adding " + i.identifierName + "VariantHandler for the bodyPrefab of name " + i.bodyName + "Body!");
                Helpers.AddVariant(i);
            }
        }
    }
    /*public abstract class VariantRegisterBase<T> : VariantRegisterBase where T : VariantRegisterBase<T>
    {
        public static T instance { get; private set; }
    }
    public abstract class VariantRegisterBase
    {
        public abstract AssetBundle AssetBundle { get; }
        protected List<VariantInfo> variantList;
        public abstract void Register();
        protected void GetVariantInfos()
        {
            if (AssetBundle == null)
            {
                Debug.LogError("Asset Bundle is null! Aborting!");
                return;
            }
            var variantInfos = AssetBundle.LoadAllAssets<VariantInfo>();
            foreach (VariantInfo varInfo in variantInfos)
            {
                variantList.Add(varInfo);
            }
        }
        protected void RegisterVariantInfos()
        {
            if(variantList == null)
            {
                Debug.LogError("Variant List is null! aborting the creation of variants from " + AssetBundle.name);
            }
            foreach (VariantInfo variant in variantList)
            {
                if(variant == null)
                {
                    Debug.LogError("Variant info is null! Aborting!");
                    continue;
                }
                Helpers.AddVariant(variant);
            }
        }
    }*/
}