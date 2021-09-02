using EntityStates.LemurianBruiserMonster;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;

namespace NebbysWrath.VariantEntityStates.ElderLemurian
{
	public class SummoningRoar : BaseState
	{
		public static GameObject flamethrowerEffectPrefab;

		public static GameObject impactEffectPrefab;

		public static GameObject tracerEffectPrefab;

		public static float maxDistance;

		public static float radius;

		public static float baseEntryDuration = 1f;

		public static float baseExitDuration = 0.5f;

		public static float baseFlamethrowerDuration = 2f;

		public static float totalDamageCoefficient = 1.2f;

		public static float procCoefficientPerTick;

		public static float tickFrequency;

		public static float force = 20f;

		public static string startAttackSoundString;

		public static string roarSoundString;

		public static string endAttackSoundString;

		public static float ignitePercentChance;

		public static float maxSpread;

		private float tickDamageCoefficient;

		private float flamethrowerStopwatch;

		private float stopwatch;

		private float entryDuration;

		private float exitDuration;

		private float flamethrowerDuration;

		private bool hasBegunFlamethrower;

		private ChildLocator childLocator;

		private Transform flamethrowerEffectInstance;

		private Transform muzzleTransform;

		private bool isCrit;

		private const float flamethrowerEffectBaseDistance = 16f;

		private BullseyeSearch enemySearch;

		public static SpawnCard lemSpawnCard = Resources.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscLemurian");

		public override void OnEnter()
		{
			flamethrowerEffectPrefab = Flamebreath.flamethrowerEffectPrefab;
			impactEffectPrefab = Flamebreath.impactEffectPrefab;
			tracerEffectPrefab = Flamebreath.tracerEffectPrefab;
			maxDistance = Flamebreath.maxDistance;
			radius = Flamebreath.radius;
			baseEntryDuration = Flamebreath.baseEntryDuration;
			baseExitDuration = Flamebreath.baseExitDuration;
			baseFlamethrowerDuration = Flamebreath.baseFlamethrowerDuration;
			totalDamageCoefficient = Flamebreath.totalDamageCoefficient;
			procCoefficientPerTick = Flamebreath.procCoefficientPerTick;
			tickFrequency = Flamebreath.tickFrequency;
			force = Flamebreath.force;
			startAttackSoundString = Flamebreath.startAttackSoundString;
			roarSoundString = EntityStates.MagmaWorm.DeathState.deathSoundString;
			endAttackSoundString = Flamebreath.endAttackSoundString;
			ignitePercentChance = Flamebreath.ignitePercentChance;
			maxSpread = Flamebreath.maxSpread;
			base.OnEnter();
			stopwatch = 0f;
			entryDuration = baseEntryDuration / attackSpeedStat;
			exitDuration = baseExitDuration / attackSpeedStat;
			flamethrowerDuration = baseFlamethrowerDuration;
			Transform modelTransform = GetModelTransform();
			if ((bool)base.characterBody)
			{
				base.characterBody.SetAimTimer(entryDuration + flamethrowerDuration + 1f);
			}
			if ((bool)modelTransform)
			{
				childLocator = modelTransform.GetComponent<ChildLocator>();
				modelTransform.GetComponent<AimAnimator>().enabled = true;
			}
			float num = flamethrowerDuration * tickFrequency;
			tickDamageCoefficient = totalDamageCoefficient / num;
			if (base.isAuthority && (bool)base.characterBody)
			{
				isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			}
			PlayAnimation("Gesture, Override", "PrepFlamebreath", "PrepFlamebreath.playbackRate", entryDuration);
			if(NetworkServer.active)
            {
				enemySearch = new BullseyeSearch();
				enemySearch.filterByDistinctEntity = false;
				enemySearch.filterByLoS = false;
				enemySearch.maxDistanceFilter = float.PositiveInfinity;
				enemySearch.minDistanceFilter = 0;
				enemySearch.minAngleFilter = 0;
				enemySearch.maxAngleFilter = 180f;
				enemySearch.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
				enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
				enemySearch.viewer = base.characterBody;
            }
		}

		public override void OnExit()
		{
			Util.PlaySound(endAttackSoundString, base.gameObject);
			PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
			if ((bool)flamethrowerEffectInstance)
			{
				EntityState.Destroy(flamethrowerEffectInstance.gameObject);
			}
			base.OnExit();
		}

		private void FireFlame(string muzzleString)
		{
			GetAimRay();
			if (base.isAuthority && (bool)muzzleTransform)
			{
				BulletAttack bulletAttack = new BulletAttack();
				bulletAttack.owner = base.gameObject;
				bulletAttack.weapon = base.gameObject;
				bulletAttack.origin = muzzleTransform.position;
				bulletAttack.aimVector = muzzleTransform.forward;
				bulletAttack.minSpread = 0f;
				bulletAttack.maxSpread = maxSpread;
				bulletAttack.damage = tickDamageCoefficient * damageStat;
				bulletAttack.force = force;
				bulletAttack.muzzleName = muzzleString;
				bulletAttack.hitEffectPrefab = impactEffectPrefab;
				bulletAttack.isCrit = isCrit;
				bulletAttack.radius = radius;
				bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
				bulletAttack.stopperMask = LayerIndex.world.mask;
				bulletAttack.procCoefficient = procCoefficientPerTick;
				bulletAttack.maxDistance = maxDistance;
				bulletAttack.smartCollision = true;
				bulletAttack.damageType = (Util.CheckRoll(ignitePercentChance, base.characterBody.master) ? DamageType.PercentIgniteOnHit : DamageType.Generic);
				bulletAttack.Fire();
			}
		}
		public void SpawnLemurian()
		{
			Vector3 searchOrigin = GetAimRay().origin;
			if ((bool)base.inputBank && base.inputBank.GetAimRaycast(float.PositiveInfinity, out var hitInfo))
			{
				searchOrigin = hitInfo.point;
			}
			if (enemySearch == null)
			{
				return;
			}
			enemySearch.searchOrigin = searchOrigin;
			enemySearch.RefreshCandidates();
			HurtBox hurtBox = enemySearch.GetResults().FirstOrDefault();
			Transform transform = (((bool)hurtBox && (bool)hurtBox.healthComponent) ? hurtBox.healthComponent.body.coreTransform : base.characterBody.coreTransform);
			if ((bool)transform)
			{
				DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(lemSpawnCard, new DirectorPlacementRule
				{
					placementMode = DirectorPlacementRule.PlacementMode.Approximate,
					minDistance = 3f,
					maxDistance = 20f,
					spawnOnTarget = transform
				}, RoR2Application.rng);
				directorSpawnRequest.summonerBodyObject = base.gameObject;
				directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate (SpawnCard.SpawnResult spawnResult)
				{
				});
				var lem = DirectorCore.instance?.TrySpawnObject(directorSpawnRequest);
				if(lem)
                {
					var Inventory = lem.GetComponent<Inventory>();
					if(Inventory)
                    {
						Inventory.GiveItemString("HealthDecay", 20);
						var characterBody = Inventory.GetComponentInParent<CharacterMaster>().GetBody();
						if (characterBody)
						{
							characterBody.baseMaxHealth /= 2;
							var characterModel = characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
							if(characterModel)
							{
								characterModel.baseRendererInfos[0].defaultMaterial = Resources.Load<Material>("Materials/matGhostEffect");
							}							
						}
					}
                }
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.fixedDeltaTime;
			if (stopwatch >= entryDuration && stopwatch < entryDuration + flamethrowerDuration && !hasBegunFlamethrower)
			{
				hasBegunFlamethrower = true;
				Util.PlaySound(roarSoundString, base.gameObject);
				PlayAnimation("Gesture, Override", "Flamebreath", "Flamebreath.playbackRate", flamethrowerDuration);
				if ((bool)childLocator)
				{
					muzzleTransform = childLocator.FindChild("MuzzleMouth");
					flamethrowerEffectInstance = UnityEngine.Object.Instantiate(flamethrowerEffectPrefab, muzzleTransform).transform;
					flamethrowerEffectInstance.transform.localPosition = Vector3.zero;
					flamethrowerEffectInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = flamethrowerDuration;
				}
			}
			if (stopwatch >= entryDuration + flamethrowerDuration && hasBegunFlamethrower)
			{
				hasBegunFlamethrower = false;
				PlayCrossfade("Gesture, Override", "ExitFlamebreath", "ExitFlamebreath.playbackRate", exitDuration, 0.1f);
			}
			if (hasBegunFlamethrower)
			{
				flamethrowerStopwatch += Time.deltaTime;
				if (flamethrowerStopwatch > 1f / tickFrequency)
				{
					flamethrowerStopwatch -= 1f / tickFrequency;
					FireFlame("MuzzleCenter");
					SpawnLemurian();
				}
			}
			else if ((bool)flamethrowerEffectInstance)
			{
				EntityState.Destroy(flamethrowerEffectInstance.gameObject);
			}
			if (stopwatch >= flamethrowerDuration + entryDuration + exitDuration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}
