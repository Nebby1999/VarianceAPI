#nullable enable
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    public interface IUndoable
    {
        public void Undo();
    }
    public interface IVariantMasterModifier : IValidatable
    {
        public IUndoable? ModifyMaster(CharacterMaster master);
    }

    [Serializable]
    public sealed class UnstableAIModifier : IVariantMasterModifier
    {
        private struct UnstableAIModifierResult : IUndoable
        {
            private struct SkillDriverWithOriginalFractionOverrides
            {
                public AISkillDriver skillDriver;
                public float minTargetHealthFractionOriginal;
                public float maxTargetHealthFractionOriginal;
                public float minUserHealthFractionOriginal;
                public float maxUserHealthFractionOriginal;
            }

            private List<SkillDriverWithOriginalFractionOverrides> undoData;

            public void AddModification(AISkillDriver skillDriver, float minTargetFractionModifier, float maxTargetFractionModifier, float minUserFractionModifier, float maxUserFractionModifier)
            {
                undoData ??= new List<SkillDriverWithOriginalFractionOverrides>();
                undoData.Add(new SkillDriverWithOriginalFractionOverrides
                {
                    skillDriver = skillDriver,
                    minTargetHealthFractionOriginal = minTargetFractionModifier,
                    maxTargetHealthFractionOriginal = maxTargetFractionModifier,
                    maxUserHealthFractionOriginal = maxUserFractionModifier,
                    minUserHealthFractionOriginal = minUserFractionModifier
                });
            }

            public void Undo()
            {
                if (undoData == null)
                    return;

                for(int i = 0; i < undoData.Count; i++)
                {
                    AISkillDriver skillDriver = undoData[i].skillDriver;
                    skillDriver.minTargetHealthFraction = undoData[i].minTargetHealthFractionOriginal;
                    skillDriver.maxTargetHealthFraction = undoData[i].maxTargetHealthFractionOriginal;
                    skillDriver.minUserHealthFraction = undoData[i].minUserHealthFractionOriginal;
                    skillDriver.maxUserHealthFraction = undoData[i].maxUserHealthFractionOriginal;
                }
            }
        }
        public float minTargetHealthFractionOverride = Mathf.NegativeInfinity;
        public float maxTargetHealthFractionOverride = Mathf.Infinity;
        public float minUserHealthFractionOverride = Mathf.NegativeInfinity;
        public float maxUserHealthFractionOverride = Mathf.Infinity;

        public IUndoable? ModifyMaster(CharacterMaster master)
        {
            if (master.aiComponents == null)
            {
                return default;
            }

            UnstableAIModifierResult result = new UnstableAIModifierResult();
            foreach (BaseAI? baseAI in master.AiComponents)
            {
                if(!baseAI)
                {
                    continue;
                }

                if (baseAI.skillDrivers == null)
                    continue;

                SetThresholds(baseAI.skillDrivers, ref result);
            }
            return result;
        }

        private void SetThresholds(AISkillDriver[] drivers, ref UnstableAIModifierResult result)
        {
            foreach(AISkillDriver? skillDriver in drivers)
            {
                if(!skillDriver)
                {
                    continue;
                }

                SetThreshold(skillDriver, ref result);
            }
        }

        private void SetThreshold(AISkillDriver driver, ref UnstableAIModifierResult result)
        {
            result.AddModification(driver, driver.minTargetHealthFraction, driver.maxTargetHealthFraction, driver.minUserHealthFraction, driver.maxUserHealthFraction);

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