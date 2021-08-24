using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VarianceAPI.ScriptableObjects;
using RoR2;

namespace VarianceAPI.Components
{
    [RequireComponent(typeof(VariantHandler), typeof(VariantRewardHandler))]
    [DisallowMultipleComponent]
    public class VariantSpawnHandler : MonoBehaviour
    {
        [HideInInspector]
        internal VariantInfo[] variantInfos;

        public VariantHandler VariantHandler
        {
            get
            {
                return gameObject.GetComponent<VariantHandler>();
            }
        }

        public VariantRewardHandler VariantRewardHandler
        {
            get
            {
                return gameObject.GetComponent<VariantRewardHandler>();
            }
        }

        public VariantInfo[] UniqueVariantInfos
        {
            get
            {
                return variantInfos.Where(variantInfo => variantInfo.unique).ToArray();
            }
        }
        public VariantInfo[] NotUniqueVariantInfos
        {
            get
            {
                return variantInfos.Where(variantInfo => !variantInfo.unique).ToArray();
            }
        }
        public VariantInfo[] EnabledVariantInfos;

        public void Awake()
        {
            var spawnRateMult = 1f;
            //If artifact is enabled, multiply spawn rates.
            if (RunArtifactManager.instance.IsArtifactEnabled(Assets.VAPIAssets.LoadAsset<ArtifactDef>("VarianceDef")))
                spawnRateMult = ConfigLoader.VarianceMultiplier.Value;

            List<VariantInfo> enabledVariants = new List<VariantInfo>();
            for(int i = 0; i < UniqueVariantInfos.Length; i++)
            {
                var variantInfo = UniqueVariantInfos[i];
                if(Util.CheckRoll(variantInfo.spawnRate * spawnRateMult))
                {
                    enabledVariants.Add(variantInfo);
                    EnabledVariantInfos = enabledVariants.ToArray();
                    return;
                }
            }

            for(int i = 0; i < NotUniqueVariantInfos.Length; i++)
            {
                var variantInfo = NotUniqueVariantInfos[i];
                if(Util.CheckRoll(variantInfo.spawnRate * spawnRateMult))
                {
                    enabledVariants.Add(variantInfo);
                }
            }
            EnabledVariantInfos = enabledVariants.ToArray();
        }
        public void Start()
        {

            VariantHandler.VariantInfos = EnabledVariantInfos;
            VariantHandler.Modify();

            if (ConfigLoader.VariantsGiveRewards.Value)
            {
                VariantRewardHandler.VariantInfos = EnabledVariantInfos;
                VariantRewardHandler.Modify();
            }
        }
    }
}
