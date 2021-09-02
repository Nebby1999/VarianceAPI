using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using VarianceAPI.Components;
using EntityStates.RoboBallBoss.Weapon;
using VarianceAPI;

namespace NebbysWrath.VariantEntityStates.SCU
{
	public class DeploySwarm : BaseState
	{
		public static float baseDuration = 6f;

		public static string attackSoundString = "Play_roboBall_attack2_createMini";

		public static string summonSoundString = "Play_roboBall_attack2_mini_spawn";

		public static int maxSummonCount = 5;

		public static float summonDuration = 2;

		public static string summonMuzzleString = "SummonMuzzle";

		public static GameObject roboBallMiniMaster = Resources.Load<GameObject>("prefabs/charactermasters/roboballminimaster");

		public static string swarmerIdentifier = "NW_SwarmerProbe";

		private Animator animator;

		private Transform modelTransform;

		private ChildLocator childLocator;

		private float duration;

		private float summonInterval;

		private float summonTimer;

		private int summonCount;

		private bool isSummoning;

		public override void OnEnter()
		{
			base.OnEnter();
			animator = GetModelAnimator();
			modelTransform = GetModelTransform();
			childLocator = modelTransform.GetComponent<ChildLocator>();
			duration = baseDuration;
			PlayCrossfade("Gesture, Additive", "DeployMinions", "DeployMinions.playbackRate", duration, 0.1f);
			Util.PlaySound(attackSoundString, base.gameObject);
			summonInterval = summonDuration / (float)maxSummonCount;
		}

		private Transform FindTargetClosest(Vector3 point, TeamIndex enemyTeam)
		{
			ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(enemyTeam);
			float num = 99999f;
			Transform result = null;
			for (int i = 0; i < teamMembers.Count; i++)
			{
				float num2 = Vector3.SqrMagnitude(teamMembers[i].transform.position - point);
				if (num2 < num)
				{
					num = num2;
					result = teamMembers[i].transform;
				}
			}
			return result;
		}

		private void SummonMinion()
		{
			var summon = new MasterSummon();
			summon.position = childLocator.FindChild(summonMuzzleString).position;
			summon.masterPrefab = roboBallMiniMaster;
			summon.summonerBodyObject = characterBody.gameObject;
			var roboBallMaster = summon.Perform();
			if (roboBallMaster)
			{
				var roboBody = roboBallMaster.GetBody();

				Destroy(roboBody.gameObject.GetComponent<VariantSpawnHandler>());
				var rewardHandler = roboBody.gameObject.GetComponent<VariantRewardHandler>();
				if (rewardHandler)
				{
					Destroy(rewardHandler);
				}

				var handler = roboBody.GetComponent<VariantHandler>();
				if(handler)
                {
					var roboBallVariants = VariantRegister.RegisteredVariants["RoboBallMiniBody"];

					var swarmer = roboBallVariants.SingleOrDefault(x => x.identifier == swarmerIdentifier);
					HG.ArrayUtils.ArrayAppend(ref handler.VariantInfos, swarmer);
					roboBallVariants.Where(x => x.identifier != swarmerIdentifier)
						.ToList()
						.ForEach(variant =>
						{
							if(Util.CheckRoll(variant.spawnRate))
							{
								HG.ArrayUtils.ArrayAppend(ref handler.VariantInfos, variant);
							}
						});

					handler.Modify();
                }
            }
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			bool flag = animator.GetFloat("DeployMinions.active") > 0.9f;
			if (isSummoning)
			{
				summonTimer += Time.fixedDeltaTime;
				if (NetworkServer.active && summonTimer > 0f && summonCount < maxSummonCount)
				{
					summonCount++;
					summonTimer -= summonInterval;
					SummonMinion();
				}
			}
			isSummoning = flag;
			if (base.fixedAge >= duration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}