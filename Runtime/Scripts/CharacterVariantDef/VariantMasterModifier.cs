#nullable enable
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    public interface IVariantMasterModifier : IValidatable, ICloneable
    {
        public IDisposable? ModifyMaster(CharacterMaster master);
    }

    [Serializable]
    public sealed class UnstableAIModifier : IVariantMasterModifier
    {
        private struct UnstableAIModifierResult : IDisposable
        {
            private struct SkillDriverWithOriginalFractionOverrides
            {
                public AISkillDriver skillDriver;
                public float minTargetHealthFractionOriginal;
                public float maxTargetHealthFractionOriginal;
                public float minUserHealthFractionOriginal;
                public float maxUserHealthFractionOriginal;
            }

            private List<SkillDriverWithOriginalFractionOverrides>? undoData;

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

            public void Dispose()
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
                undoData.Clear();
            }
        }
        public float minTargetHealthFractionOverride = Mathf.NegativeInfinity;
        public float maxTargetHealthFractionOverride = Mathf.Infinity;
        public float minUserHealthFractionOverride = Mathf.NegativeInfinity;
        public float maxUserHealthFractionOverride = Mathf.Infinity;

        public IDisposable? ModifyMaster(CharacterMaster master)
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

        public object Clone()
        {
            return new UnstableAIModifier
            {
                minTargetHealthFractionOverride = minTargetHealthFractionOverride,
                minUserHealthFractionOverride = minUserHealthFractionOverride,
                maxTargetHealthFractionOverride = maxTargetHealthFractionOverride,
                maxUserHealthFractionOverride = maxUserHealthFractionOverride
            };
        }
    }

    [Serializable]
    public struct AlwaysSprintAIModifier : IVariantMasterModifier
    {
        public struct AlwaysSprintAIModifierResult : IDisposable
        {
            private struct SkillDriverSprint
            {
                public AISkillDriver skillDriver;
                public bool shouldSprintOriginal;
            }

            private List<SkillDriverSprint>? _skillDriverSprint;

            public void AddModification(AISkillDriver skillDriver, bool shouldSprintOriginal)
            {
                _skillDriverSprint ??= new List<SkillDriverSprint>();
                _skillDriverSprint.Add(new SkillDriverSprint { skillDriver = skillDriver, shouldSprintOriginal = shouldSprintOriginal });
            }

            public void Dispose()
            {
                if (_skillDriverSprint == null)
                    return;

                for (int i = _skillDriverSprint.Count - 1; i >= 0; i--)
                {
                    _skillDriverSprint[i].skillDriver.shouldSprint = _skillDriverSprint[i].shouldSprintOriginal;
                }
                _skillDriverSprint.Clear();
            }
        }
        public IDisposable? ModifyMaster(CharacterMaster master)
        {
            if (master.aiComponents == null)
            {
                return null;
            }

            AlwaysSprintAIModifierResult result = new AlwaysSprintAIModifierResult();
            foreach (BaseAI? baseAI in master.AiComponents)
            {
                if (!baseAI)
                {
                    continue;
                }

                if (baseAI.skillDrivers == null)
                    continue;

                ForceSprint(baseAI.skillDrivers, ref result);
            }
            return result;
        }

        private void ForceSprint(AISkillDriver[] drivers, ref AlwaysSprintAIModifierResult result)
        {
            foreach (AISkillDriver? skillDriver in drivers)
            {
                if (!skillDriver)
                {
                    continue;
                }
                result.AddModification(skillDriver, skillDriver.shouldSprint);

                skillDriver.shouldSprint = true;
            }
        }

        public void Validate() { }

        public object Clone()
        {
            return this;
        }
    }

    [Serializable]
    public sealed class BaseAIDampModifier : IVariantMasterModifier
    {
        private struct BaseAIDampModifierResult : IDisposable
        {
            private float? baseAIDampBonus;
            private float? baseAIDampMultiplier;
            private List<BaseAI>? _aiComponents;

            public void Dispose()
            {
                if (_aiComponents == null)
                    return;

                for (int i = _aiComponents.Count - 1; i >= 0; i--)
                {
                    _aiComponents[i].aimVectorDampTime -= baseAIDampBonus ?? 0;
                    _aiComponents[i].aimVectorMaxSpeed /= baseAIDampMultiplier ?? 1;
                }
                _aiComponents.Clear();
            }

            public void AddBaseAI(BaseAI ai)
            {
                _aiComponents ??= new List<BaseAI>();
                _aiComponents.Add(ai);
            }

            public BaseAIDampModifierResult(float baseAIDampBonus, float baseAIDampMultiplier)
            {
                _aiComponents = new List<BaseAI>();
                this.baseAIDampBonus = baseAIDampBonus;
                this.baseAIDampMultiplier = baseAIDampMultiplier;
            }
        }
        public float baseAIDampBonus = 0;
        [Min(0 + float.Epsilon)]
        public float baseAIDampMultiplier = 1;
        public IDisposable? ModifyMaster(CharacterMaster master)
        {
            if (master.AiComponents == null)
                return null;

            var result = new BaseAIDampModifierResult(baseAIDampBonus, baseAIDampMultiplier);

            foreach(BaseAI? baseAI in master.AiComponents)
            {
                if(!baseAI)
                {
                    continue;
                }

                result.AddBaseAI(baseAI);

                baseAI.aimVectorDampTime += baseAIDampBonus;
                baseAI.aimVectorMaxSpeed *= baseAIDampMultiplier;
            }
            return result;
        }

        public void Validate() { }

        public object Clone()
        {
            return new BaseAIDampModifier(baseAIDampBonus, baseAIDampMultiplier);
        }

        public BaseAIDampModifier(float _baseAIDampBonus, float _baseAIDampMultiplier)
        {
            baseAIDampBonus = _baseAIDampBonus;
            baseAIDampMultiplier = Mathf.Max(0, _baseAIDampMultiplier);
        }
    }
}