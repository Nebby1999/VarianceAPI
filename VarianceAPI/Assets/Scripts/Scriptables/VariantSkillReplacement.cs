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
        /// <summary>
        /// Which skill slot to replace
        /// </summary>
        public SkillSlot skillSlot;

        /// <summary>
        /// replacement skill
        /// </summary>
        public SkillDef skillDef;
    }
}
