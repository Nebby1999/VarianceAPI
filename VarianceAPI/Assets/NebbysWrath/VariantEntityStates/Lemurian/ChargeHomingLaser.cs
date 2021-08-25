/*using EntityStates.LemurianMonster;
using EntityStates.GolemMonster;
using UnityEngine;
using EntityStates;
using RoR2;
using EntityStates.Wisp1Monster;

namespace NebbysWrath.VariantEntityStates.Lemurian
{
    public class ChargeHomingLaser : BaseState
    {
		public static float baseDuration;

		public static GameObject chargeVfxPrefab;

		public static string attackString;

		private float duration;

		private GameObject chargeVfxInstance;

		public override void OnEnter()
		{
			baseDuration = ChargeFireball.baseDuration;
			chargeVfxPrefab = ChargeLaser.effectPrefab;
			attackString = ChargeEmbers.attackString;
			base.OnEnter();
			duration = baseDuration / attackSpeedStat;
			GetModelAnimator();
			Transform modelTransform = GetModelTransform();
			Util.PlayAttackSpeedSound(attackString, base.gameObject, duration);
			if ((bool)modelTransform)
			{
				ChildLocator component = modelTransform.GetComponent<ChildLocator>();
				if ((bool)component)
				{
					Transform transform = component.FindChild("MuzzleMouth");
					if ((bool)transform && (bool)chargeVfxPrefab)
					{
						chargeVfxInstance = Object.Instantiate(chargeVfxPrefab, transform.position, transform.rotation);
						chargeVfxInstance.transform.parent = transform;
						chargeVfxInstance.transform.localScale = new Vector3(0.25f,0.25f,0.25f);
					}
				}
			}
			PlayAnimation("Gesture", "ChargeFireball", "ChargeFireball.playbackRate", duration);
		}

		public override void OnExit()
		{
			base.OnExit();
			if ((bool)chargeVfxInstance)
			{
				EntityState.Destroy(chargeVfxInstance);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= duration && base.isAuthority)
			{
				FireHomingLaser nextState = new FireHomingLaser();
				outer.SetNextState(nextState);
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}
*/