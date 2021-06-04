using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantSkillReplacement", menuName = "VarianceAPI/VariantSkillReplacement", order = 7)]
    public class VariantSkillReplacement : ScriptableObject
    {
        [Header("Variant Skill Replacement")]

            [Tooltip("Skillslot to apply the replacement\nYou can use DevDebugToolkit's Spawn_As Command for knowing what skillslot you must target")]
            public SkillSlot skillSlot;

            [Tooltip("The Replacement Skill")]
            public SkillDef skillDef;
    }
}
