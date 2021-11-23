using EntityStates;
using EntityStates.LemurianBruiserMonster;
using EntityStates.LemurianMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace NebbysWrath.VariantEntityStates.ElderLemurian
{
    public class FireSingleFireball : BaseState
    {
        public static GameObject projectilePrefab;

        public static GameObject muzzleflashEffectPrefab;

        public static int projectileCount = 1;

        public static float totalYawSpread = 5f;

        public static float baseDuration = 2f;

        public static float baseFireDuration = 0.2f;

        public static float damageCoefficient = 1.2f;

        public static float projectileSpeed;

        public static float force = 20f;

        public static string attackString;

        private float duration;

        private float fireDuration;

        private int projectilesFired;

        public override void OnEnter()
        {
            baseDuration = FireFireball.baseDuration;
            baseFireDuration = FireMegaFireball.baseFireDuration;
            damageCoefficient = FireFireball.damageCoefficient;
            projectilePrefab = FireFireball.projectilePrefab;
            muzzleflashEffectPrefab = FireMegaFireball.muzzleflashEffectPrefab;
            projectileSpeed = FireMegaFireball.projectileSpeed;
            attackString = FireMegaFireball.attackString;
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireDuration = baseFireDuration / attackSpeedStat;
            PlayAnimation("Gesture, Additive", "FireMegaFireball", "FireMegaFireball.playbackRate", duration);
            Util.PlaySound(attackString, base.gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            string muzzleName = "MuzzleMouth";
            if (base.isAuthority)
            {
                int num = Mathf.FloorToInt(base.fixedAge / fireDuration * (float)projectileCount);
                if (projectilesFired <= num && projectilesFired < projectileCount)
                {
                    if ((bool)muzzleflashEffectPrefab)
                    {
                        EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleName, transmit: false);
                    }
                    Ray aimRay = GetAimRay();
                    float speedOverride = projectileSpeed;
                    float bonusYaw = (float)Mathf.FloorToInt((float)projectilesFired - (float)(3 - 1) / 2f) / (float)(3 - 1) * totalYawSpread;
                    Vector3 forward = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, bonusYaw);
                    ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, speedOverride);
                    projectilesFired++;
                }
            }
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
