﻿using EntityStates.ImpMonster;//.DoubleSlash
using System;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using R2API;

namespace NebbysWrath.VariantEntityStates.Imp
{
    public class IchorClaws : BaseState
    {
        public static float baseDuration = 1.0f;

        public static float damageCoefficient = 0.4f;

        public static float procCoefficient;

        public static float selfForce;

        public static float forceMagnitude = 16f;

        public static GameObject hitEffectPrefab;

        public static GameObject swipeEffectPrefab;

        public static string enterSoundString;

        public static string slashSoundString;

        public static float walkSpeedPenaltyCoefficient;

        private OverlapAttack attack;

        private Animator modelAnimator;

        private float duration;

        private int slashCount;

        private Transform modelTransform;

        public override void OnEnter()
        {
            procCoefficient = DoubleSlash.procCoefficient;
            selfForce = DoubleSlash.selfForce;
            hitEffectPrefab = DoubleSlash.hitEffectPrefab;
            swipeEffectPrefab = DoubleSlash.swipeEffectPrefab;
            enterSoundString = DoubleSlash.enterSoundString;
            slashSoundString = DoubleSlash.slashSoundString;
            walkSpeedPenaltyCoefficient = DoubleSlash.walkSpeedPenaltyCoefficient;
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            modelAnimator = GetModelAnimator();
            modelTransform = GetModelTransform();
            base.characterMotor.walkSpeedPenaltyCoefficient = walkSpeedPenaltyCoefficient;
            attack = new OverlapAttack();
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
            attack.damage = damageCoefficient * damageStat;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
            attack.procCoefficient = procCoefficient;
            attack.damageType = DamageType.Generic;

            DamageAPI.AddModdedDamageType(attack, DamageTypes.PulverizeOnHit.pulverizeOnHit);

            Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, attackSpeedStat);
            if ((bool)modelAnimator)
            {
                PlayAnimation("Gesture, Additive", "DoubleSlash", "DoubleSlash.playbackRate", duration);
                PlayAnimation("Gesture, Override", "DoubleSlash", "DoubleSlash.playbackRate", duration);
            }
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(duration + 2f);
            }
        }

        public override void OnExit()
        {
            base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
            base.OnExit();
        }

        private void HandleSlash(string animatorParamName, string muzzleName, string hitBoxGroupName)
        {
            if (!(modelAnimator.GetFloat(animatorParamName) > 0.1f))
            {
                return;
            }
            Util.PlaySound(slashSoundString, base.gameObject);
            EffectManager.SimpleMuzzleFlash(swipeEffectPrefab, base.gameObject, muzzleName, transmit: true);
            slashCount++;
            if ((bool)modelTransform)
            {
                attack.hitBoxGroup = Array.Find(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitBoxGroupName);
            }
            if ((bool)base.healthComponent)
            {
                base.healthComponent.TakeDamageForce(base.characterDirection.forward * selfForce, alwaysApply: true);
            }
            attack.ResetIgnoredHealthComponents();
            if ((bool)base.characterDirection)
            {
                attack.forceVector = base.characterDirection.forward * forceMagnitude;
            }
            attack.Fire();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && (bool)modelAnimator)
            {
                switch (slashCount)
                {
                    case 0:
                        HandleSlash("HandR.hitBoxActive", "SwipeRight", "HandR");
                        break;
                    case 1:
                        HandleSlash("HandL.hitBoxActive", "SwipeLeft", "HandL");
                        break;
                }
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