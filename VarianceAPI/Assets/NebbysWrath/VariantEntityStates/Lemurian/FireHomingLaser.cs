/*using EntityStates.GolemMonster;
using EntityStates.TitanMonster;
using EntityStates.LemurianMonster;
using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using VarianceAPI.Components;
using System.Linq;
using System.Reflection;

namespace NebbysWrath.VariantEntityStates.Lemurian
{
    public class FireHomingLaser : BaseState
    {
		public static GameObject projectilePrefab;

		public static GameObject effectPrefab;

		public static float baseDuration = 2f;

		public static float damageCoefficient = 1.2f;

		public static float force = 20f;

		public static string attackString;

		private float duration;

		private TitanRockController titanRockController;

		public override void OnEnter()
		{
			if((bool)!titanRockController)
            {
				titanRockController = new TitanRockController();
            }
			baseDuration = FireFireball.baseDuration;
			damageCoefficient = FireFireball.damageCoefficient;
			force = titanRockController.damageForce;
			projectilePrefab = RechargeRocks.rockControllerPrefab.GetComponent<TitanRockController>().projectilePrefab;
			effectPrefab = FireLaser.effectPrefab;
			base.OnEnter();
			duration = baseDuration / attackSpeedStat;
			PlayAnimation("Gesture", "FireFireball", "FireFireball.playbackRate", duration);
			Util.PlaySound(attackString, base.gameObject);
			Ray aimRay = GetAimRay();
			string muzzleName = "MuzzleMouth";
			if ((bool)effectPrefab)
			{
				EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
			}
			if (base.isAuthority)
			{
				Fire();
			}
		}
		private void Fire()
        {
			if(projectilePrefab)
            {
				Vector3 position = GetModelTransform().GetComponent<ChildLocator>().FindChild("MuzzleMouth").position;
				Vector3 forward = inputBank.aimDirection;
				if(Util.CharacterRaycast(gameObject, new Ray(inputBank.aimOrigin, inputBank.aimDirection), out var hitInfo, float.PositiveInfinity, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
                {
					forward = hitInfo.point - position;
                }
				float num = (characterBody ? characterBody.damage : 1f);
				if(base.characterBody.gameObject.GetComponents<VariantHandler>().Where(VH => VH.identifierName == "TO30_FlameThrowerLemurian").ToList().FirstOrDefault().isVariant)
                {
					damageCoefficient /= 2; 
                }
				ProjectileManager.instance.FireProjectile(projectilePrefab, position, Util.QuaternionSafeLookRotation(forward), gameObject, damageCoefficient * num, force, RollCrit());
            }
        }

		public override void OnExit()
		{
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= duration && base.isAuthority)
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
*/