using EntityStates;
using EntityStates.GolemMonster;
using RoR2;
using UnityEngine;

namespace NebbysWrath.VariantEntityStates.LesserWisp
{
    public class FireStoneLaser : BaseState
    {
        public static GameObject effectPrefab;

        public static GameObject hitEffectPrefab;

        public static GameObject tracerEffectPrefab;

        public static float damageCoefficient;

        public static float blastRadius;

        public static float force;

        public static float minSpread;

        public static float maxSpread;

        public static int bulletCount;

        public static float baseDuration = 2f;

        public static string attackSoundString;

        public Vector3 laserDirection;

        private float duration;

        private Ray modifiedAimRay;

        public override void OnEnter()
        {
            effectPrefab = FireLaser.effectPrefab;
            hitEffectPrefab = FireLaser.hitEffectPrefab;
            tracerEffectPrefab = FireLaser.tracerEffectPrefab;
            damageCoefficient = FireLaser.damageCoefficient;
            blastRadius = FireLaser.blastRadius;
            force = FireLaser.force;
            minSpread = FireLaser.minSpread;
            maxSpread = FireLaser.maxSpread;
            bulletCount = FireLaser.bulletCount;
            attackSoundString = FireLaser.attackSoundString;
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            modifiedAimRay = GetAimRay();
            modifiedAimRay.direction = laserDirection;
            GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            Util.PlaySound(attackSoundString, base.gameObject);
            string muzzleName = "Muzzle";
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
            PlayAnimation("Body", "FireAttack1", "FireAttack1.playbackRate", duration);
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
            }
            if (!base.isAuthority)
            {
                return;
            }
            float num = 1000f;
            Vector3 vector = modifiedAimRay.origin + modifiedAimRay.direction * num;
            if (Physics.Raycast(modifiedAimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.defaultLayer.mask | (int)LayerIndex.entityPrecise.mask))
            {
                vector = hitInfo.point;
            }
            BlastAttack blastAttack = new BlastAttack();
            blastAttack.attacker = base.gameObject;
            blastAttack.inflictor = base.gameObject;
            blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
            blastAttack.baseDamage = damageStat * 5.72f * damageCoefficient;
            blastAttack.baseForce = force * 0.2f;
            blastAttack.position = vector;
            blastAttack.radius = blastRadius;
            blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            blastAttack.bonusForce = force * modifiedAimRay.direction;
            blastAttack.Fire();
            _ = modifiedAimRay.origin;
            if (!modelTransform)
            {
                return;
            }
            ChildLocator component = modelTransform.GetComponent<ChildLocator>();
            if ((bool)component)
            {
                int childIndex = component.FindChildIndex(muzzleName);
                if ((bool)tracerEffectPrefab)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = vector,
                        start = modifiedAimRay.origin
                    };
                    effectData.SetChildLocatorTransformReference(base.gameObject, childIndex);
                    EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
                    EffectManager.SpawnEffect(hitEffectPrefab, effectData, transmit: true);
                }
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