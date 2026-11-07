#nullable enable
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// A <see cref="IVariantMasterModifier"/> is a <see cref="IValidatable"/> and <see cref="ICloneable"/> interface used to define new Master modifiers. These implementations can be serialized in the editor via the <see cref="SerializeReference"/> attribute.
    /// <br></br>
    /// Modifications are done via the <see cref="ModifyMaster(CharacterMaster)"/> method.
    /// <br></br>
    /// Must support cloning via the <see cref="ICloneable"/> implementation.
    /// <para></para>
    /// <b>Built In Master Modifiers</b>
    /// <list type="bullet">
    /// <item><see cref="UnstableAIModifier"/>: Reimplementation of VAPI 2.0's Unstable flag. Will make it so AI uses "low health" skill drivers even at max health</item>
    /// <item><see cref="AlwaysSprintAIModifier"/>: Reimplementation of VAPI 2.0's Always Sprint flag. Will make it so AI is always sprinting</item>
    /// <item><see cref="AISkillDriverAimVectorModifier"/>: Used to override the Aim Vector values for a specific skill driver</item>
    /// <item><see cref="BaseAIDampModifier"/>: Used to modify the Aim Vectors of the BaseAI</item>
    /// <item><see cref="ReplaceSkillDrivers"/>: Used to replace an AI's Skill Drivers with new ones</item>
    /// </list>
    /// </summary>
    public interface IVariantMasterModifier : IValidatable, ICloneable
    {
        /// <summary>
        /// Modify the <paramref name="master"/> here. This method should return a disposable object used to revert any changes applied to the master.
        /// </summary>
        public IDisposable? ModifyMaster(CharacterMaster master);
    }

    /// <summary>
    /// Forces an AI to use it's "low health" skill drivers, even at max health.
    /// </summary>
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

                for (int i = 0; i < undoData.Count; i++)
                {
                    AISkillDriver skillDriver = undoData[i].skillDriver;
                    if (!skillDriver)
                        continue;

                    skillDriver.minTargetHealthFraction = undoData[i].minTargetHealthFractionOriginal;
                    skillDriver.maxTargetHealthFraction = undoData[i].maxTargetHealthFractionOriginal;
                    skillDriver.minUserHealthFraction = undoData[i].minUserHealthFractionOriginal;
                    skillDriver.maxUserHealthFraction = undoData[i].maxUserHealthFractionOriginal;
                }
                undoData.Clear();
            }
        }
        [Tooltip("Overrides all minTargetHealthFraction on the skill drivers values with this value")]
        public float minTargetHealthFractionOverride = Mathf.NegativeInfinity;
        [Tooltip("Overrides all maxTargetHealthFractionOverride on the skill drivers values with this value")]
        public float maxTargetHealthFractionOverride = Mathf.Infinity;
        [Tooltip("Overrides all minUserHealthFractionOverride on the skill drivers values with this value")]
        public float minUserHealthFractionOverride = Mathf.NegativeInfinity;
        [Tooltip("Overrides all maxUserHealthFractionOverride on the skill drivers values with this value")]
        public float maxUserHealthFractionOverride = Mathf.Infinity;

        public IDisposable? ModifyMaster(CharacterMaster master)
        {
            if (!NetworkServer.active)
                return null;

            if (master.aiComponents == null)
            {
#if DEBUG
                VAPILog.Warning($"Not applying UnstableAI modifier as the master is not an AI");
#endif
                return null;
            }

            UnstableAIModifierResult result = new UnstableAIModifierResult();
            foreach (BaseAI? baseAI in master.AiComponents)
            {
                if (!baseAI)
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
            foreach (AISkillDriver? skillDriver in drivers)
            {
                if (!skillDriver)
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

        /// <summary>
        /// Clones the metadata in this into a new instance of <see cref="UnstableAIModifier"/>
        /// </summary>
        public object Clone()
        {
            return new UnstableAIModifier(minTargetHealthFractionOverride, maxTargetHealthFractionOverride, minUserHealthFractionOverride, maxUserHealthFractionOverride);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public UnstableAIModifier(float minTargetHealthFractionOverride = Mathf.NegativeInfinity, float maxTargetHealthFractionOverride = Mathf.Infinity, float minUserHealthFractionOverride = Mathf.NegativeInfinity, float maxUserHealthFractionOverride = Mathf.Infinity)
        {
            this.minTargetHealthFractionOverride = minTargetHealthFractionOverride;
            this.maxTargetHealthFractionOverride = maxTargetHealthFractionOverride;
            this.minUserHealthFractionOverride = minUserHealthFractionOverride;
            this.maxUserHealthFractionOverride = maxUserHealthFractionOverride;
        }

        /// <summary>
        /// Parameterless Constructor
        /// </summary>
        public UnstableAIModifier() { }
    }

    /// <summary>
    /// Forces an AI to always sprint.
    /// </summary>
    [Serializable]
    public struct AlwaysSprintAIModifier : IVariantMasterModifier
    {
        private struct AlwaysSprintAIModifierResult : IDisposable
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
            if (!NetworkServer.active)
            {
                return null;
            }

            if (master.aiComponents == null)
            {
#if DEBUG
                VAPILog.Warning($"Not applying AlwaysSprintAI modifier as the master is not an AI");
#endif
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

        /// <summary>
        /// Clones the metadata in this into a new instance of <see cref="UnstableAIModifier"/>
        /// </summary>
        public object Clone()
        {
            return this;
        }
    }

    /// <summary>
    /// Changes a specific skill driver's aim vector modifiers.
    /// <para></para>
    /// If the specified skill driver doesnt have overrides for the values, the modifiers are applied using the BaseAI's values.
    /// </summary>
    [Serializable]
    public sealed class AISkillDriverAimVectorModifier : IVariantMasterModifier
    {
        private struct AISkillDriverAimVectorModifierResult : IDisposable
        {
            public AISkillDriver affectedAISkillDriver;
            public float originalAimVectorDampTimeOverrideValue;
            public float originalAimVectorMaxspeedOverrideValue;

            public void Dispose()
            {
                affectedAISkillDriver.aimVectorDampTimeOverride = originalAimVectorDampTimeOverrideValue;
                affectedAISkillDriver.aimVectorMaxSpeedOverride = originalAimVectorMaxspeedOverrideValue;
            }
        }

        /// <summary>
        /// Multiplier applied to the SkillDriver's aimVectorMaxSpeed.
        /// </summary>
        public float aimVectorMaxSpeedMultiplier { get => _aimVectorMaxSpeedMultiplier; set => _aimVectorMaxSpeedMultiplier = Mathf.Max(0 + float.Epsilon, value); }
        [SerializeField, Min(0 + float.Epsilon)]
        [Tooltip("Multiplier applied to the SkillDriver's aimVectorMaxSpeed.")]
        private float _aimVectorMaxSpeedMultiplier = 1f;

        /// <summary>
        /// Multiplier applied to the SkillDriver's aimVectorDampTime.
        /// </summary>
        public float aimVectorDampTimeMultiplier { get => _aimVectorDampTimeMultiplier; set => _aimVectorDampTimeMultiplier = Mathf.Max(0 + float.Epsilon, value); }
        [SerializeField, Min(0 + float.Epsilon)]
        [Tooltip("Multiplier applied to the SkillDriver's aimVectorDampTime.")]
        private float _aimVectorDampTimeMultiplier = 1f;

        [Tooltip("The SkillDriver to modify")]
        public string aiSkillDriverTarget = "";
        [Tooltip("if the skill driver uses this skill slot, it'll be modified")]
        public SkillSlot targetSkillSlot;

        /// <summary>
        /// Clones the metadata in this into a new instance of <see cref="AISkillDriverAimVectorModifier"/>
        /// </summary>
        public object Clone()
        {
            return new AISkillDriverAimVectorModifier
            {
                aiSkillDriverTarget = aiSkillDriverTarget,
                _aimVectorDampTimeMultiplier = _aimVectorDampTimeMultiplier,
                _aimVectorMaxSpeedMultiplier = aimVectorMaxSpeedMultiplier
            };
        }

        public IDisposable? ModifyMaster(CharacterMaster master)
        {
            if (!NetworkServer.active)
                return null;

            if (string.IsNullOrWhiteSpace(aiSkillDriverTarget) && targetSkillSlot == SkillSlot.None)
            {
                VAPILog.Warning($"Cannot apply AISkillDriverAimVectorModifier. The target skill driver string is null, empty or whitespace and the targetSkillSlot enum is none");
                return null;
            }

            if (master.AiComponents == null)
            {

#if DEBUG
                VAPILog.Warning($"Not applying UnstableAI modifier as the master is not an AI");
#endif
                return null;
            }

            DisposableCollectionHelper helper = new DisposableCollectionHelper(true);
            for (int i = 0; i < master.AiComponents.Length; i++)
            {
                var aiComponent = master.aiComponents[i];
                foreach (var skillDriver in aiComponent.skillDrivers)
                {
                    if (skillDriver.customName == aiSkillDriverTarget || skillDriver.skillSlot == targetSkillSlot)
                    {

                        var result = new AISkillDriverAimVectorModifierResult
                        {
                            originalAimVectorDampTimeOverrideValue = skillDriver.aimVectorDampTimeOverride,
                            originalAimVectorMaxspeedOverrideValue = skillDriver.aimVectorMaxSpeedOverride,
                            affectedAISkillDriver = skillDriver
                        };
                        helper.AddDisposable(result);

                        //If there's no overrides set to the skill driver, initialize them by taking them from the AI component itself.
                        if (skillDriver.aimVectorDampTimeOverride < 0)
                        {
                            skillDriver.aimVectorDampTimeOverride = aiComponent.aimVectorDampTime;
                        }
                        if (skillDriver.aimVectorMaxSpeedOverride < 0)
                        {
                            skillDriver.aimVectorMaxSpeedOverride = aiComponent.aimVectorMaxSpeed;
                        }

                        //apply multipliers to overrides then return the disposable
                        skillDriver.aimVectorDampTimeOverride *= aimVectorDampTimeMultiplier;
                        skillDriver.aimVectorMaxSpeedOverride *= aimVectorMaxSpeedMultiplier;
                    }
                }
            }

            return helper;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(aiSkillDriverTarget) && targetSkillSlot == SkillSlot.None)
            {
                VAPILog.Warning($"Ai skill driver target should not be empty, null or whitespace when the targetSkillSlot is set to SkillSlot.None!");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AISkillDriverAimVectorModifier() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public AISkillDriverAimVectorModifier(SkillSlot targetSlot, float aimVectorMaxSpeedMultiplier = 1, float aimVectorDampTimeMultiplier = 1, string aiSkillDriverTarget = "")
        {
            this.targetSkillSlot = targetSlot;
            this.aiSkillDriverTarget = aiSkillDriverTarget;
            this.aimVectorDampTimeMultiplier = aimVectorDampTimeMultiplier;
            this.aimVectorMaxSpeedMultiplier = aimVectorMaxSpeedMultiplier;
        }
    }

    /// <summary>
    /// Changes an AI's aim vector modifiers
    /// </summary>
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
        [Tooltip("This value is added to aimVectorDampTime")]
        public float baseAIDampBonus = 0;

        /// <summary>
        /// This value is multiplied by aimVectorMaxSpeed
        /// </summary>
        public float baseAIDampMultiplier { get => _baseAIDampMultiplier; set => _baseAIDampMultiplier = Mathf.Max(0 + float.Epsilon, value); }
        [SerializeField, Min(0 + float.Epsilon)]
        [Tooltip("This value is multiplied by aimVectorMaxSpeed")]
        private float _baseAIDampMultiplier = 1;
        public IDisposable? ModifyMaster(CharacterMaster master)
        {
            if (!NetworkServer.active)
                return null;

            if (master.AiComponents == null)
            {
#if DEBUG
                VAPILog.Warning($"Not applying UnstableAI modifier as the master is not an AI");
#endif
                return null;
            }

            var result = new BaseAIDampModifierResult(baseAIDampBonus, _baseAIDampMultiplier);

            foreach (BaseAI? baseAI in master.AiComponents)
            {
                if (!baseAI)
                {
                    continue;
                }

                result.AddBaseAI(baseAI);

                baseAI.aimVectorDampTime += baseAIDampBonus;
                baseAI.aimVectorMaxSpeed *= _baseAIDampMultiplier;
            }
            return result;
        }

        public void Validate() { }

        /// <summary>
        /// Clones the metadata in this into a new instance of <see cref="BaseAIDampModifier"/>
        /// </summary>
        public object Clone()
        {
            return new BaseAIDampModifier(baseAIDampBonus, _baseAIDampMultiplier);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseAIDampModifier(float _baseAIDampBonus = 0, float _baseAIDampMultiplier = 1)
        {
            baseAIDampBonus = _baseAIDampBonus;
            baseAIDampMultiplier = _baseAIDampMultiplier;
        }

        /// <summary>
        /// Parameterless Constructor
        /// </summary>
        public BaseAIDampModifier()
        {

        }
    }

    /// <summary>
    /// Replaces an AI's Skill Drivers
    /// </summary>
    [Serializable]
    public struct ReplaceSkillDrivers : IVariantMasterModifier
    {
        private struct DisposableSkillDriverReplacement : IDisposable
        {
            public AISkillDriver[] _origSkillDrivers;
            public BaseAI _modifiedBaseAI;

            public GameObject _customSkillDriversContainer;

            public void Dispose()
            {
                if (_modifiedBaseAI)
                {
                    _modifiedBaseAI.ReplaceSkillDrivers(_origSkillDrivers);
                }

                if (_customSkillDriversContainer)
                {
                    UnityEngine.Object.Destroy(_customSkillDriversContainer);
                }
            }
        }

        [Tooltip("The GameObject prefab that contains the new skill drivers for the AI")]
        public GameObject skillDriversPrefab;

        /// <summary>
        /// Clones the metadata in this into a new instance of <see cref="ReplaceSkillDrivers"/>
        /// </summary>
        public object Clone()
        {
            return new ReplaceSkillDrivers
            {
                skillDriversPrefab = skillDriversPrefab
            };
        }

        public IDisposable? ModifyMaster(CharacterMaster master)
        {
            if (!NetworkServer.active)
                return null;

            if (!master)
                return null;

            if (skillDriversPrefab == null)
            {
#if DEBUG
                VAPILog.Warning($"Not applying ReplaceSkillDrivers modifier as there's no skillDriversPrefab assigned");
#endif
                return null;
            }

            if (!master.TryGetComponent<BaseAI>(out var modifiedBaseAI))
            {
#if DEBUG
                VAPILog.Warning($"Not applying ReplaceSkillDrivers modifier as the master is not an AI");
#endif
                return null;
            }

            if (!skillDriversPrefab.TryGetComponent<AISkillDriver>(out _))
            {
#if DEBUG
                VAPILog.Warning($"Not applying ReplaceSkillDrivers modifier as the skillDriversPrefab has no AISkillDriver components");
#endif
                return null;
            }

            GameObject customSkillDriversContainer = UnityEngine.Object.Instantiate(skillDriversPrefab);
            customSkillDriversContainer.transform.SetParent(master.transform);
            AISkillDriver[] origSkillDrivers = modifiedBaseAI.skillDrivers;
            modifiedBaseAI.ReplaceSkillDrivers(customSkillDriversContainer.GetComponents<AISkillDriver>());

            return new DisposableSkillDriverReplacement
            {
                _origSkillDrivers = origSkillDrivers,
                _modifiedBaseAI = modifiedBaseAI,
                _customSkillDriversContainer = customSkillDriversContainer
            };
        }

        public void Validate()
        {
            if (skillDriversPrefab == null)
            {
                VAPILog.Warning($"Cannot have a null skillDriversPrefab on a ReplaceSkillDrivers Master Modifier");
                return;
            }

            if (skillDriversPrefab.TryGetComponent<AISkillDriver>(out _))
            {
                VAPILog.Warning($"Assigned skillDriverPrefab has no AISkillDrivers");
                return;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_skillDriversPrefab">The skill drivers to use</param>
        public ReplaceSkillDrivers(GameObject _skillDriversPrefab)
        {
            skillDriversPrefab = _skillDriversPrefab;
        }
    }
}