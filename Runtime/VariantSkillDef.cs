#nullable enable
using RoR2.Skills;
using RoR2;
using System;

namespace VAPI
{
    [Serializable]
    public sealed class VariantSkillReplacement
    {
        public SkillDef? skillDef;
        public SkillSlot slot = SkillSlot.None;
        public string slotName = "";
    }
}