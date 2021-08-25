using EntityStates;
using EntityStates.GolemMonster;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NebbysWrath.VariantEntityStates.GreaterWisp
{
    public class FireDoubleStoneLaser : BaseState
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

        public Vector3 leftLaserDirection;

        private Ray leftModifiedAimRay;

        public Vector3 rightLaserDirection;

        private Ray rightModifiedAimRay;

        private float duration;

        public override void OnEnter()
        {
            baseDuration = FireLaser.baseDuration;
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
            leftModifiedAimRay = GetAimRay();
            leftModifiedAimRay.direction = leftLaserDirection;
            rightModifiedAimRay = GetAimRay();
            rightModifiedAimRay.direction = leftLaserDirection;
            GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            Util.PlaySound(attackSoundString, base.gameObject);
            string leftHandMuzzle = "MuzzleLeft";
            string rightHandMuzzle = "MuzzleRight";
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
            PlayAnimation("Gesture", "FireCannons", "FireCannons.playbackRate", duration);
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, leftHandMuzzle, transmit: false);
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, rightHandMuzzle, transmit: false);
            }
            if (!base.isAuthority)
            {
                return;
            }
            float num = 1000f;
            //Left Hand
            {
                Vector3 leftVector = leftModifiedAimRay.origin + leftModifiedAimRay.direction * num;
                if (Physics.Raycast(leftModifiedAimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.defaultLayer.mask | (int)LayerIndex.entityPrecise.mask))
                {
                    leftVector = hitInfo.point;
                }
                BlastAttack leftBlastAttack = new BlastAttack();
                leftBlastAttack.attacker = base.gameObject;
                leftBlastAttack.inflictor = base.gameObject;
                leftBlastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
                leftBlastAttack.baseDamage = damageStat * damageCoefficient;
                leftBlastAttack.baseForce = force * 0.2f;
                leftBlastAttack.position = leftVector;
                leftBlastAttack.radius = blastRadius;
                leftBlastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                leftBlastAttack.bonusForce = force * leftModifiedAimRay.direction;
                leftBlastAttack.Fire();
                _ = leftModifiedAimRay.origin;
                if (!modelTransform)
                {
                    return;
                }
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    int childIndex = component.FindChildIndex(leftHandMuzzle);
                    if ((bool)tracerEffectPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = leftVector,
                            start = leftModifiedAimRay.origin
                        };
                        effectData.SetChildLocatorTransformReference(base.gameObject, childIndex);
                        EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
                        EffectManager.SpawnEffect(hitEffectPrefab, effectData, transmit: true);
                    }
                }
            }
            //Right Hand
            {
                Vector3 rightVector = rightModifiedAimRay.origin + rightModifiedAimRay.direction * num;
                if (Physics.Raycast(rightModifiedAimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.defaultLayer.mask | (int)LayerIndex.entityPrecise.mask))
                {
                    rightVector = hitInfo.point;
                }
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.attacker = base.gameObject;
                blastAttack.inflictor = base.gameObject;
                blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
                blastAttack.baseDamage = damageStat * damageCoefficient;
                blastAttack.baseForce = force * 0.2f;
                blastAttack.position = rightVector;
                blastAttack.radius = blastRadius;
                blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                blastAttack.bonusForce = force * rightModifiedAimRay.direction;
                blastAttack.Fire();
                _ = rightModifiedAimRay.origin;
                if (!modelTransform)
                {
                    return;
                }
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    int childIndex = component.FindChildIndex(rightHandMuzzle);
                    if ((bool)tracerEffectPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = rightVector,
                            start = rightModifiedAimRay.origin
                        };
                        effectData.SetChildLocatorTransformReference(base.gameObject, childIndex);
                        EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
                        EffectManager.SpawnEffect(hitEffectPrefab, effectData, transmit: true);
                    }
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
