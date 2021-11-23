using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using VarianceAPI.Components;

namespace NebbysWrath.VariantEntityStates.WanderingVagrant.DeathStates
{
    public class JellyDeploy : EntityStates.VagrantMonster.DeathState
    {

        public static GameObject masterPrefab = Resources.Load<GameObject>("prefabs/charactermasters/jellyfishmaster");

        public static EquipmentIndex index;
        private Vector3 spawnPossition;

        public override void OnEnter()
        {
            spawnPossition = characterBody.corePosition;
            index = characterBody.equipmentSlot.equipmentIndex;

            base.OnEnter();

            if (NetworkServer.active)
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Nebby.TheOriginal30"))
                {
                    SpawnMOAJ();
                }
                else
                {
                    SpawnJellies();
                }
            }
        }

        private void SpawnMOAJ()
        {
            for (int i = 0; i < 3; i++)
            {
                var summon = new MasterSummon();
                summon.position = spawnPossition;
                summon.masterPrefab = masterPrefab;
                summon.summonerBodyObject = this.gameObject;
                var jellyMaster = summon.Perform();
                if (jellyMaster)
                {
                    var jelly = jellyMaster.GetBody();
                    jelly.AddTimedBuff(RoR2Content.Buffs.Immune, 1);
                    jelly.inventory.SetEquipmentIndex(index);

                    var spawnHandler = jelly.gameObject.GetComponent<VariantSpawnHandler>();
                    if (spawnHandler)
                    {
                        spawnHandler.customSpawning = true;

                        int[] index = new int[] { spawnHandler.variantInfos.ToList().FindIndex(x => x.identifier == "TO30_MOAJ") };
                        spawnHandler.RpcModifyComponents(index, VariantSpawnHandler.RPCVariantInfo.All);
                    }
                }
            }
        }

        private void SpawnJellies()
        {
            for (int i = 0; i < 10; i++)
            {
                var summon = new MasterSummon();
                summon.position = spawnPossition;
                summon.masterPrefab = masterPrefab;
                summon.summonerBodyObject = this.gameObject;
                var jellyMaster = summon.Perform();
                if (jellyMaster)
                {
                    var jelly = jellyMaster.GetBody();
                    jelly.AddTimedBuff(RoR2Content.Buffs.Immune, 1);

                    jelly.inventory.SetEquipmentIndex(index);

                    Destroy(jelly.gameObject.GetComponent<VariantSpawnHandler>());
                }
            }
        }
    }
}
