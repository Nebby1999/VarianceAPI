#nullable enable
using RoR2;
using RoR2.CharacterAI;
using System;
using UnityEngine;

namespace VAPI
{
    public interface IVariantMasterModifier : IValidatable
    {
        public void ModifyMaster(CharacterMaster master);
    }

    [Serializable]
    public sealed class UnstableAIModifier : IVariantMasterModifier
    {
        public float minTargetHealthFractionOverride = Mathf.NegativeInfinity;
        public float maxTargetHealthFractionOverride = Mathf.Infinity;
        public float minUserHealthFractionOverride = Mathf.NegativeInfinity;
        public float maxUserHealthFractionOverride = Mathf.Infinity;

        public void ModifyMaster(CharacterMaster master)
        {
            if (master.aiComponents == null)
            {
                return;
            }

            foreach (BaseAI? baseAI in master.AiComponents)
            {
                if(!baseAI)
                {
                    continue;
                }

                if (baseAI.skillDrivers == null)
                    continue;

                SetThresholds(baseAI.skillDrivers);
            }
        }

        private void SetThresholds(AISkillDriver[] drivers)
        {
            foreach(AISkillDriver? skillDriver in drivers)
            {
                if(!skillDriver)
                {
                    continue;
                }

                SetThreshold(skillDriver);
            }
        }

        private void SetThreshold(AISkillDriver driver)
        {
            driver.minTargetHealthFraction = Mathf.NegativeInfinity;
            driver.maxTargetHealthFraction = Mathf.Infinity;
            driver.minUserHealthFraction = Mathf.NegativeInfinity;
            driver.maxUserHealthFraction = Mathf.Infinity;
        }

        public void Validate() { }
    }

    [Serializable]
    public struct AlwaysSprintAIModifier : IVariantMasterModifier
    {
        public void ModifyMaster(CharacterMaster master)
        {
            if (master.aiComponents == null)
            {
                return;
            }

            foreach (BaseAI? baseAI in master.AiComponents)
            {
                if (!baseAI)
                {
                    continue;
                }

                if (baseAI.skillDrivers == null)
                    continue;

                ForceSprint(baseAI.skillDrivers);
            }
        }

        private void ForceSprint(AISkillDriver[] drivers)
        {
            foreach (AISkillDriver? skillDriver in drivers)
            {
                if (!skillDriver)
                {
                    continue;
                }

                skillDriver.shouldSprint = true;
            }
        }

        public void Validate() { }
    }

    [Serializable]
    public sealed class BaseAIDampModifier : IVariantMasterModifier
    {
        public float baseAIDampBonus = 0;
        [Min(0)]
        public float baseAIDampMultiplier = 1;
        public void ModifyMaster(CharacterMaster master)
        {
            if (master.AiComponents == null)
                return;

            foreach(BaseAI? baseAI in master.AiComponents)
            {
                if(!baseAI)
                {
                    continue;
                }

                baseAI.aimVectorDampTime += baseAIDampBonus;
                baseAI.aimVectorMaxSpeed *= baseAIDampMultiplier;
            }
        }

        public void Validate() { }
    }
}