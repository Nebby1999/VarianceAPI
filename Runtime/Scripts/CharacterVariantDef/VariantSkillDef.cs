#nullable enable
using RoR2.Skills;
using RoR2;
using System;

namespace VAPI
{
    [Serializable]
    public sealed class VariantSkillReplacement
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
        public SkillDef? skillDef;
        public SkillSlot slot = SkillSlot.None;
        public string slotName = "";

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
    }
}