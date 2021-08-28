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
    [DisallowMultipleComponent]
    public class VariantSpawnHandler : MonoBehaviour
    {
        [SerializeField]
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
                var toReturn = variantInfos.Where(variantInfo => variantInfo.unique == true).ToArray();
                if (toReturn.Length == 0)
                {
                    return null;
                }
                else
                    return toReturn;
            }
        }
        public VariantInfo[] shuffledUniques;
        public VariantInfo[] NotUniqueVariantInfos
        {
            get
            {
                var toReturn = variantInfos.Where(variantInfo => variantInfo.unique == false).ToArray();
                if (toReturn.Length == 0)
                {
                    return null;
                }
                else
                    return toReturn;
            }
        }
        public VariantInfo[] EnabledVariantInfos = Array.Empty<VariantInfo>();



        public void Start()
        {
            ShuffleUniques();

            if (shuffledUniques == null && NotUniqueVariantInfos == null)
            {
                Destroy(VariantHandler);
                Destroy(VariantRewardHandler);
                Destroy(this);
                return;
            }

            var spawnRateMult = 10f;
            //If artifact is enabled, multiply spawn rates.
            if (RunArtifactManager.instance.IsArtifactEnabled(Assets.VAPIAssets.LoadAsset<ArtifactDef>("Variance")))
                spawnRateMult = ConfigLoader.VarianceMultiplier.Value;

            List<VariantInfo> enabledVariants = new List<VariantInfo>();

            if (shuffledUniques != null)
            {
                for(int i = 0; i < shuffledUniques.Length; i++)
                {
                    var variantInfo = shuffledUniques[i];
                    if(Util.CheckRoll(variantInfo.spawnRate * spawnRateMult))
                    {
                        enabledVariants.Add(variantInfo);
                        EnabledVariantInfos = enabledVariants.ToArray();
                        ModifyComponents();
                        return;
                    }
                }
            }

            if (NotUniqueVariantInfos != null)
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

            if(enabledVariants.Count == 0)
            {
                DestroyComponents();
                return;
            }

            EnabledVariantInfos = enabledVariants.ToArray();
            ModifyComponents();
        }
        private void ModifyComponents()
        {

            VariantHandler.VariantInfos = EnabledVariantInfos;
            VariantHandler.Modify();

            if (ConfigLoader.VariantsGiveRewards.Value)
            {
                VariantRewardHandler.VariantInfos = EnabledVariantInfos;
                VariantRewardHandler.Modify();
            }
        }
        private void DestroyComponents()
        {
            Destroy(VariantHandler);
            Destroy(VariantRewardHandler);
            Destroy(this);
        }
        private void ShuffleUniques()
        {
            shuffledUniques = UniqueVariantInfos;
            VariantInfo tempVariantInfo;
            for (int i = 0; i < shuffledUniques.Length - 1; i++)
            {
                int rng = UnityEngine.Random.Range(i, shuffledUniques.Length);
                tempVariantInfo = shuffledUniques[rng];
                shuffledUniques[i] = tempVariantInfo;
            }
        }
    }
}
