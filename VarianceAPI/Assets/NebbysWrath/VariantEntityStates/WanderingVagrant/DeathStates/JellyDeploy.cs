using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI;
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
			
			if(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Nebby.TheOriginal30"))
            {
				SpawnMOAJ();
            }
			else
            {
				SpawnJellies();
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

					Destroy(jelly.gameObject.GetComponent<VariantSpawnHandler>());
					var rewardHandler = jelly.gameObject.GetComponent<VariantRewardHandler>();
					if (rewardHandler)
					{
						Destroy(rewardHandler);
					}

					var handler = jelly.GetComponent<VariantHandler>();
					if (handler)
					{
						var roboBallVariants = VariantRegister.RegisteredVariants["JellyfishBody"];

						var moaj = roboBallVariants.SingleOrDefault(x => x.identifier == "TO30_MOAJ");
						HG.ArrayUtils.ArrayAppend(ref handler.VariantInfos, moaj);
						handler.Modify();
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
