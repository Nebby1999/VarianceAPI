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
    public class VariantSpawnHandler : NetworkBehaviour
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
        public VariantInfo[] shuffledUniques;

        public List<VariantInfo> EnabledVariants = new List<VariantInfo>();

        public float SpawnRateMultiplier = 1;

        public SyncListInt EnabledVariantIndices = new SyncListInt();

        [SyncVar(hook = nameof(OnUniqueAssigned))]
        public int EnabledUniqueVariantIndex;

        [SyncVar(hook = nameof(OnFinishedCheckRolls))]
        public bool finishedCheckRolls = false;

        #region Networking
        public void Awake()
        {
            EnabledVariantIndices.Callback = OnAddedVariant;
        }
        private void OnUniqueAssigned(int index)
        {
            EnabledVariants.Add(UniqueVariantInfos[index]);
            ModifyComponents();
        }

        private void OnAddedVariant(SyncList<int>.Operation op, int itemIndex)
        {
            var trueIndex = EnabledVariantIndices[itemIndex];
            if(NotUniqueVariantInfos[trueIndex])
            {
                Debug.Log($"Adding variant {NotUniqueVariantInfos[trueIndex]}");
                EnabledVariants.Add(NotUniqueVariantInfos[trueIndex]);
            }
        }
        private void OnFinishedCheckRolls(bool boolean)
        {
            ModifyComponents();
        }
        #endregion Networking

        public void Start()
        {
            if (!NetworkServer.active)
                return;

            if (UniqueVariantInfos == null && NotUniqueVariantInfos == null)
            {
                return;
            }

            //If artifact is enabled, multiply spawn rates.
            if (RunArtifactManager.instance.IsArtifactEnabled(Assets.VAPIAssets.LoadAsset<ArtifactDef>("Variance")))
                SpawnRateMultiplier = ConfigLoader.VarianceMultiplier.Value;

            if (UniqueVariantInfos != null)
            {
                var rng = new WeightedSelection<int>();
                float notUniqueChance = 0f;
                for(int i = 0; i < UniqueVariantInfos.Length; i++)
                {
                    var chance = UniqueVariantInfos[i].spawnRate * SpawnRateMultiplier;
                    rng.AddChoice(i, Mathf.Min(100, chance));
                    notUniqueChance += Mathf.Max(0, 100 - chance);
                }
                rng.AddChoice(-1, notUniqueChance);

                var index = rng.Evaluate(Run.instance.runRNG.nextNormalizedFloat);
                if (index != -1)
                {
                    EnabledUniqueVariantIndex = index;
                    //OnUniqueAssigned(index);
                    return;
                }
            }

            if (NotUniqueVariantInfos != null)
            {
                for (int i = 0; i < NotUniqueVariantInfos.Length; i++)
                {
                    var variantInfo = NotUniqueVariantInfos[i];

                    var spawnRate = variantInfo.spawnRate * SpawnRateMultiplier;
                    if (spawnRate > 100)
                        spawnRate = 100;
                    if (spawnRate < 0)
                        spawnRate = 0;

                    Debug.Log($"{variantInfo}'s spawnrate with multiplier: {spawnRate}");

                    if (Util.CheckRoll(spawnRate))
                    {
                        Debug.Log($"Adding {variantInfo} to the enabled variant indices ({i}).");

                        EnabledVariantIndices.Add(i);

                        //EnabledVariants.Add(NotUniqueVariantInfos[i]);
                    }
                }
                finishedCheckRolls = true;
                //OnFinishedCheckRolls(true);
            }
        }
        private void ModifyComponents()
        {

            VariantHandler.VariantInfos = EnabledVariants.ToArray();
            VariantHandler.Modify();

            if (ConfigLoader.VariantsGiveRewards.Value)
            {
                VariantRewardHandler.VariantInfos = EnabledVariants.ToArray();
                VariantRewardHandler.Modify();
            }
        }
    }
}
