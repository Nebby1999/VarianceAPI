using EntityStates;
using EntityStates.LemurianBruiserMonster;
using RoR2;
using UnityEngine;

namespace NebbysWrath.VariantEntityStates.ElderLemurian
{
    public class ChargeSingleFireball : BaseState
    {
        public static float baseDuration = 1f;

        public static GameObject chargeEffectPrefab;

        public static string attackString;

        private float duration;

        private GameObject chargeInstance;

        public override void OnEnter()
        {
            baseDuration = ChargeMegaFireball.baseDuration;
            chargeEffectPrefab = ChargeMegaFireball.chargeEffectPrefab;
            attackString = ChargeMegaFireball.attackString;
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Animator modelAnimator = GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
            if ((bool)modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    Transform transform = component.FindChild("MuzzleMouth");
                    if ((bool)transform && (bool)chargeEffectPrefab)
                    {
                        chargeInstance = Object.Instantiate(chargeEffectPrefab, transform.position, transform.rotation);
                        chargeInstance.transform.parent = transform;
                        ScaleParticleSystemDuration component2 = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
                        if ((bool)component2)
                        {
                            component2.newDuration = duration;
                        }
                    }
                }
            }
            if ((bool)modelAnimator)
            {
                PlayCrossfade("Gesture, Additive", "ChargeMegaFireball", "ChargeMegaFireball.playbackRate", duration, 0.1f);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if ((bool)chargeInstance)
            {
                EntityState.Destroy(chargeInstance);
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                FireSingleFireball nextState = new FireSingleFireball();
                outer.SetNextState(nextState);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
