#nullable enable
using RoR2.Skills;
using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// Represents a replacement for a Skill.
    /// <br></br>
    /// This class shouldn't be reused across multiple variants, call <see cref="Clone"/> instead of reusing.
    /// </summary>
    [Serializable]
    public sealed class VariantSkillReplacement : ICloneable
    {
        private readonly struct DisposableSkillOverride : IDisposable
        {
            public readonly GenericSkill targetGenericSkill;
            public readonly object source;
            public readonly SkillDef overrideSkillDef;
            public readonly GenericSkill.SkillOverridePriority priority;
            public DisposableSkillOverride(GenericSkill targetGenericSkill, object source, SkillDef skillDef, GenericSkill.SkillOverridePriority priority)
            {
                this.targetGenericSkill = targetGenericSkill;
                this.source = source;
                this.overrideSkillDef = skillDef;
                this.priority = priority;
            }

            public void Dispose()
            {
                if (!targetGenericSkill)
                    return;

                targetGenericSkill.UnsetSkillOverride(source, overrideSkillDef, priority);
            }
        }

        [Tooltip("The skill to use as a replacement, can be left null.")]
        public SkillDef? skillDef;

        [Tooltip("The slot to replace it's skill, if left none, the slot name is used instead")]
        public SkillSlot slot = SkillSlot.None;
        [Tooltip("The name of the GenericSkill to replace it's skill, only used if slot is set to None")]
        public string slotName = "";

        /// <summary>
        /// Applies the skill replacements to the skill locator.
        /// </summary>
        public IDisposable? ApplySkillReplacement(SkillLocator skillLocator)
        {
            if (!skillLocator)
                return null;

            GenericSkill? genericSkillTarget = null;
            genericSkillTarget = skillLocator.GetOrFindSkill(slot, slotName);

            if(!genericSkillTarget)
            {
                return null;
            }

            SkillDef overrideSkillDef = GetOverrideSkillDef();
            DisposableSkillOverride disposableOverride = new DisposableSkillOverride(genericSkillTarget!, this, overrideSkillDef, GenericSkill.SkillOverridePriority.Replacement);
            genericSkillTarget!.SetSkillOverride(disposableOverride.source, disposableOverride.overrideSkillDef, disposableOverride.priority);

            return disposableOverride;
        }

        private static LazyLoader<SkillDef> goToMainSkillDef = new LazyLoader<SkillDef>("GoToMain");
        private SkillDef GetOverrideSkillDef()
        {
            if(skillDef)
            {
                return skillDef!;
            }
            return (SkillDef)goToMainSkillDef!;
        }

        /// <summary>
        /// Returns a clone of this.
        /// </summary>
        public object Clone()
        {
            if(slot == SkillSlot.None)
            {
                return new VariantSkillReplacement(skillDef, slotName);
            }
            else
            {
                return new VariantSkillReplacement(skillDef, slot);
            }
        }

        /// <summary>
        /// Constructor for a skill replacement targetting a skill slot
        /// </summary>
        public VariantSkillReplacement(SkillDef? _skillDef, SkillSlot _slot)
        {
            skillDef = _skillDef;
            slot = _slot;
        }

        /// <summary>
        /// Constructor for a skill replacement targetting a slot name
        /// </summary>
        public VariantSkillReplacement(SkillDef? _skillDef, string _slotName)
        {
            skillDef = _skillDef;
            slotName = _slotName;
            slot = SkillSlot.None;
        }

        /// <summary>
        /// Parameterless Constructor
        /// </summary>
        public VariantSkillReplacement() { }
    }
}