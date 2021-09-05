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
        public VariantInfo[] NotUniqueVariantInfos { get; internal set; }

        [SerializeField]
        public VariantInfo[] UniqueVariantInfos { get; internal set; }

        public VariantHandler VariantHandler { get => gameObject.GetComponent<VariantHandler>(); }

        public VariantRewardHandler VariantRewardHandler { get => gameObject.GetComponent<VariantRewardHandler>(); }

        public List<VariantInfo> EnabledVariantInfos;

        public float SpawnRateMultiplier = 1;

        [Server]
        public void Start()
        {
            //Spawning logic only ran by host.
            if (!NetworkServer.active)
                return;

            //Artifact enabled? set multiplier.
            if (RunArtifactManager.instance.IsArtifactEnabled(Assets.VAPIAssets.LoadAsset<ArtifactDef>("Variance")))
                SpawnRateMultiplier = ConfigLoader.VarianceMultiplier.Value;

            List<int> enabledIndexes = new List<int>();

            //roll for uniques only if the length is not 0.
            if(UniqueVariantInfos.Length != 0)
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
                    EnabledVariantInfos.Add(UniqueVariantInfos[index]);

                    //Modifies the client's components.
                    RpcModifyComponents(enabledIndexes.ToArray(), true);

                    //Modifies the host's components.
                    ModifyHostComponents();

                    return;
                }
            }
            if(NotUniqueVariantInfos.Length != 0)
            { 
                for(int i = 0; i < NotUniqueVariantInfos.Length; i++)
                {
                    var currentInfo = NotUniqueVariantInfos[i];

                    var spawnRate = Mathf.Min(100, currentInfo.spawnRate * SpawnRateMultiplier);
                    if(Util.CheckRoll(spawnRate))
                    {
                        enabledIndexes.Add(i);
                        EnabledVariantInfos.Add(NotUniqueVariantInfos[i]);
                    }
                }
            }
            if(enabledIndexes.Count != 0)
            {
                RpcModifyComponents(enabledIndexes.ToArray(), false);
                ModifyHostComponents();
            }
        }

        //If my gut feeling is correct, calling this on the server will call the method on the clients
        [ClientRpc]
        public void RpcModifyComponents(int[] indexes, bool unique)
        {
            List<VariantInfo> enabled = new List<VariantInfo>();
            if (unique)
                for (int i = 0; i < indexes.Length; i++)
                    enabled.Add(UniqueVariantInfos[indexes[i]]);
            else
                for (int i = 0; i < indexes.Length; i++)
                    enabled.Add(NotUniqueVariantInfos[indexes[i]]);

            EnabledVariantInfos = enabled;

            VariantHandler.VariantInfos = enabled.ToArray(); ;
            VariantHandler.Modify();

            if(ConfigLoader.VariantsGiveRewards.Value)
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

            if(ConfigLoader.VariantsGiveRewards.Value)
            {
                VariantRewardHandler.VariantInfos = EnabledVariantInfos.ToArray();
                VariantRewardHandler.Modify();
            }
        }
    }
}
