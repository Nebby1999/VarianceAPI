using EntityStates;
using EntityStates.ClaymanMonster;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace NebbysWrath.VariantEntityStates.ClayMen
{
    public class SuperBleedSwipe : BaseState
    {
        public static float baseDuration = 1f;

        public static float damageCoefficient = 1.4f;

        public static float forceMagnitude = 16f;

        public static float selfForceMagnitude = 1800;

        public static float radius = 3f;

        public static GameObject hitEffectPrefab;

        public static GameObject swingEffectPrefab;

        public static string attackString = "Play_merc_sword_swing";

        private OverlapAttack attack;

        private Animator modelAnimator;

        private float duration;

        private bool hasSlashed;

        public override void OnEnter()
        {
            hitEffectPrefab = SwipeForward.hitEffectPrefab;
            swingEffectPrefab = SwipeForward.swingEffectPrefab;
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            modelAnimator = GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            attack = new OverlapAttack();
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
            attack.damage = damageCoefficient * damageStat;
            attack.damageType = DamageType.SuperBleedOnCrit;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
            Util.PlaySound(attackString, base.gameObject);
            if ((bool)modelTransform)
            {
                attack.hitBoxGroup = Array.Find(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Sword");
            }
            if ((bool)modelAnimator)
            {
                PlayAnimation("Gesture, Override", "SwipeForward", "SwipeForward.playbackRate", duration);
                PlayAnimation("Gesture, Additive", "SwipeForward", "SwipeForward.playbackRate", duration);
            }
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && (bool)modelAnimator && modelAnimator.GetFloat("SwipeForward.hitBoxActive") > 0.1f)
            {
                if (!hasSlashed)
                {
                    EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, "SwingCenter", transmit: true);
                    HealthComponent healthComponent = base.characterBody.healthComponent;
                    CharacterDirection component = base.characterBody.GetComponent<CharacterDirection>();
                    if ((bool)healthComponent)
                    {
                        healthComponent.TakeDamageForce(selfForceMagnitude * component.forward, alwaysApply: true);
                    }
                    hasSlashed = true;
                }
                attack.forceVector = base.transform.forward * forceMagnitude;
                attack.Fire();
            }
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
