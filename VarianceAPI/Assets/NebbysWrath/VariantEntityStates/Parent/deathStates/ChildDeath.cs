using EntityStates;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace NebbysWrath.VariantEntityStates.Parent.DeathStates
{
	public class ChildDeath : GenericCharacterDeath
	{
		[SerializeField]
		public float timeBeforeDestealth = 2f;

		[SerializeField]
		public float destealthDuration = 0.1f;

		[SerializeField]
		public Material destealthMaterial;

		[SerializeField]
		public GameObject effectPrefab;

		[SerializeField]
		public string effectMuzzleString = "SlamZone";

		public static Vector3 SpawnPosition;

		public static GameObject MasterPrefab = Resources.Load<GameObject>("prefabs/charactermasters/ParentMaster");

		public static EntityStates.ParentMonster.DeathState og;

		private bool destealth;

		public override bool shouldAutoDestroy
		{
			get
			{
				if (destealth)
				{
					return base.fixedAge > timeBeforeDestealth + destealthDuration;
				}
				return false;
			}
		}

		public override void OnEnter()
		{
			SpawnPosition = characterBody.corePosition;
			effectPrefab = og.effectPrefab;
			destealthMaterial = og.destealthMaterial;
			base.OnEnter();
		}

		public override void OnExit()
		{
			DestroyModel();
			SpawnParents();
			base.OnExit();
		}

		private void SpawnParents()
        {
			if(NetworkServer.active)
            {
				for (int i = 0; i < 2; i++)
				{
					var summon = new MasterSummon();
					summon.position = SpawnPosition;
					summon.masterPrefab = MasterPrefab;
					summon.summonerBodyObject = this.gameObject;
					var parentMaster = summon.Perform();
					if (parentMaster)
					{
						var parentBody = parentMaster.GetBody();
						parentBody.AddBuff(RoR2Content.Buffs.WarCryBuff);
					}
				}
            }
		}
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge > timeBeforeDestealth && !destealth)
			{
				DoDestealth();
			}
			if (destealth && base.fixedAge > timeBeforeDestealth + destealthDuration)
			{
				DestroyModel();
			}
		}

		private void DoDestealth()
		{
			destealth = true;
			if ((bool)effectPrefab)
			{
				EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, effectMuzzleString, transmit: false);
			}
			Transform modelTransform = GetModelTransform();
			if ((bool)modelTransform)
			{
				CharacterModel component = modelTransform.gameObject.GetComponent<CharacterModel>();
				if ((bool)destealthMaterial)
				{
					TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
					temporaryOverlay.duration = destealthDuration;
					temporaryOverlay.destroyComponentOnEnd = true;
					temporaryOverlay.originalMaterial = destealthMaterial;
					temporaryOverlay.inspectorCharacterModel = component;
					temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlay.animateShaderAlpha = true;
					PrintController component2 = base.modelLocator.modelTransform.gameObject.GetComponent<PrintController>();
					component2.enabled = false;
					component2.printTime = destealthDuration;
					component2.startingPrintHeight = 0f;
					component2.maxPrintHeight = 20f;
					component2.startingPrintBias = 0f;
					component2.maxPrintBias = 2f;
					component2.disableWhenFinished = false;
					component2.printCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
					component2.enabled = true;
				}
				Transform transform = FindModelChild("CoreLight");
				if ((bool)transform)
				{
					transform.gameObject.SetActive(value: false);
				}
			}
		}
	}

}