using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityStates.JellyfishMonster;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using VarianceAPI.Components;

namespace TheOriginal30.VariantEntityStates.Jellyfish.DeathStates
{
    public class SpawnJellies : GenericCharacterDeath
    {
		public static GameObject enterEffectPrefab;
		public static int jellies = 5;

		public override void OnEnter()
		{
			enterEffectPrefab = DeathState.enterEffectPrefab;
			base.OnEnter();
			DestroyModel();
			if (NetworkServer.active)
			{
				for (int i = 0; i < jellies; i++)
				{
					Vector3 position = base.characterBody.corePosition + (5 * UnityEngine.Random.insideUnitSphere);

					DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest((SpawnCard)Resources.Load(string.Format("SpawnCards/CharacterSpawnCards/cscJellyfish")), new DirectorPlacementRule
					{
						placementMode = DirectorPlacementRule.PlacementMode.Direct,
						minDistance = 0f,
						maxDistance = 0f,
						position = position
					}, RoR2Application.rng);

					directorSpawnRequest.summonerBodyObject = base.gameObject;

					GameObject jelly = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
					if (jelly)
					{

						Inventory inventory = jelly.GetComponent<Inventory>();
						inventory.SetEquipmentIndex(base.characterBody.inventory.currentEquipmentIndex);
						CharacterBody body = inventory.GetComponentInParent<CharacterMaster>().GetBody();
						body.AddTimedBuff(RoR2Content.Buffs.Immune, 1);
					}
				}
				DestroyBodyAsapServer();
			}
		}

		public override void CreateDeathEffects()
		{
			base.CreateDeathEffects();
			if ((bool)enterEffectPrefab)
			{
				EffectManager.SimpleEffect(enterEffectPrefab, base.transform.position, base.transform.rotation, transmit: false);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}
	}
}
