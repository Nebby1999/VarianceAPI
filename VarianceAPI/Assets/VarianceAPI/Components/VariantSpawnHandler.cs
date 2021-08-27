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
        
        internal VariantInfo[] variantInfos = Array.Empty<VariantInfo>();

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
                var toReturn = variantInfos.Where(variantInfo => variantInfo.unique == true).ToArray();
                Debug.Log(toReturn);
                return toReturn;
            }
        }
        public VariantInfo[] NotUniqueVariantInfos
        {
            get
            {
                var toReturn = variantInfos.Where(variantInfo => variantInfo.unique == false).ToArray();
                Debug.Log(toReturn);
                return toReturn;
            }
        }
        public VariantInfo[] EnabledVariantInfos = Array.Empty<VariantInfo>();

        public void Start()
        {
            var spawnRateMult = 1f;
            //If artifact is enabled, multiply spawn rates.
            if (RunArtifactManager.instance.IsArtifactEnabled(Assets.VAPIAssets.LoadAsset<ArtifactDef>("Variance")))
                spawnRateMult = ConfigLoader.VarianceMultiplier.Value;
            Debug.Log("Creating list.");
            List<VariantInfo> enabledVariants = new List<VariantInfo>();
            Debug.Log("made the list");
            Debug.Log(UniqueVariantInfos.Length);
            if (UniqueVariantInfos.Length != 0)
            {
                Debug.Log("Entering Foreach...");
                foreach (VariantInfo v in UniqueVariantInfos)
                {
                    Debug.Log("Foreach!");
                    if (Util.CheckRoll(v.spawnRate * spawnRateMult))
                    {
                        enabledVariants.Add(v);
                        EnabledVariantInfos = enabledVariants.ToArray();
                        return;
                    }
                }
            }

            if (NotUniqueVariantInfos.Length != 0)
            {
                for (int i = 0; i < NotUniqueVariantInfos.Length; i++)
                {
                    var variantInfo = NotUniqueVariantInfos[i];
                    if (Util.CheckRoll(variantInfo.spawnRate * spawnRateMult))
                    {
                        enabledVariants.Add(variantInfo);
                    }
                }
            }
            EnabledVariantInfos = enabledVariants.ToArray();
            ModifyComponents();
        }
        public void ModifyComponents()
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
