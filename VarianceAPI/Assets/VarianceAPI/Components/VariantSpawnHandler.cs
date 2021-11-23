using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using VarianceAPI.ScriptableObjects;

namespace VarianceAPI.Components
{
    [DisallowMultipleComponent]
    public class VariantSpawnHandler : NetworkBehaviour
    {
        [SerializeField]
        public VariantInfo[] variantInfos;
        public VariantHandler VariantHandler { get => gameObject.GetComponent<VariantHandler>(); }

        public VariantRewardHandler VariantRewardHandler { get => gameObject.GetComponent<VariantRewardHandler>(); }

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

        public bool customSpawning = false;

        public List<VariantInfo> EnabledVariantInfos;

        public float SpawnRateMultiplier = 1;

        [ServerCallback]
        public void Start()
        {
            //Spawning logic only ran by host. or if custom spawning is used.
            if (!NetworkServer.active || customSpawning)
                return;

            if (UniqueVariantInfos == null && NotUniqueVariantInfos == null)
                return;

            //Artifact enabled? set multiplier.
            if (RunArtifactManager.instance.IsArtifactEnabled(Assets.VAPIAssets.LoadAsset<ArtifactDef>("Variance")))
                SpawnRateMultiplier = ConfigLoader.VarianceMultiplier.Value;

            List<int> enabledIndexes = new List<int>();

            //roll for uniques only if the length is not 0.
            if (UniqueVariantInfos != null)
            {
                //Dont reinvent the wheel neb, lol.
                var rng = new WeightedSelection<int>();
                float notUniqueChance = 0f;
                for (int i = 0; i < UniqueVariantInfos.Length; i++)
                {
                    var chance = UniqueVariantInfos[i].spawnRate * SpawnRateMultiplier;
                    rng.AddChoice(i, Mathf.Min(100, chance));
                    notUniqueChance += Mathf.Max(0, 100 - chance);
                }
                rng.AddChoice(-1, notUniqueChance);

                var index = rng.Evaluate(Run.instance.runRNG.nextNormalizedFloat);
                if (index != -1)
                {
                    enabledIndexes.Add(index);

                    //Modifies the client's components.
                    RpcModifyComponents(enabledIndexes.ToArray(), RPCVariantInfo.Uniques);

                    //Modifies the host's components.
                    if (!NetworkClient.active)
                        ModifyHostComponents();

                    return;
                }
            }
            if (NotUniqueVariantInfos != null)
            {
                for (int i = 0; i < NotUniqueVariantInfos.Length; i++)
                {
                    var currentInfo = NotUniqueVariantInfos[i];

                    var spawnRate = Mathf.Min(100, currentInfo.spawnRate * SpawnRateMultiplier);
                    if (Util.CheckRoll(spawnRate))
                    {
                        enabledIndexes.Add(i);
                    }
                }
            }
            if (enabledIndexes.Count != 0)
            {
                RpcModifyComponents(enabledIndexes.ToArray(), RPCVariantInfo.NotUniques);

                //Modifies the host's components.
                if (!NetworkClient.active)
                    ModifyHostComponents();
            }
        }

        //If my gut feeling is correct, calling this on the server will call the method on the clients
        [ClientRpc]
        public void RpcModifyComponents(int[] indexes, RPCVariantInfo variantInfoSearchType)
        {
            List<VariantInfo> enabled = new List<VariantInfo>();

            switch (variantInfoSearchType)
            {
                case RPCVariantInfo.Uniques:
                    for (int i = 0; i < indexes.Length; i++)
                        enabled.Add(UniqueVariantInfos[indexes[i]]);
                    break;
                case RPCVariantInfo.NotUniques:
                    for (int i = 0; i < indexes.Length; i++)
                        enabled.Add(NotUniqueVariantInfos[indexes[i]]);
                    break;
                case RPCVariantInfo.All:
                    for (int i = 0; i < indexes.Length; i++)
                        enabled.Add(variantInfos[indexes[i]]);
                    break;
            }

            EnabledVariantInfos = enabled;

            VariantHandler.VariantInfos = enabled.ToArray(); ;
            VariantHandler.Modify();

            if (ConfigLoader.VariantsGiveRewards.Value)
            {
                VariantRewardHandler.VariantInfos = enabled.ToArray();
                VariantRewardHandler.Modify();
            }
        }

        [Server]
        public void ModifyHostComponents()
        {
            VariantHandler.VariantInfos = EnabledVariantInfos.ToArray();
            VariantHandler.Modify();

            if (ConfigLoader.VariantsGiveRewards.Value)
            {
                VariantRewardHandler.VariantInfos = EnabledVariantInfos.ToArray();
                VariantRewardHandler.Modify();
            }
        }

        public enum RPCVariantInfo : int
        {
            Uniques,
            NotUniques,
            All
        }
    }
}
