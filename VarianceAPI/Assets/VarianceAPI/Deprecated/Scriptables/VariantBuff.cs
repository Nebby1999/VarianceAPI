using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantBuff", menuName = "VAPI_OLD/VariantBuff")]
    public class VariantBuff : ScriptableObject
    {
        [Header("VariantBuff")]
            [Tooltip("The buff to give to the enemy, MUST be the same as the Buff's BuffDef!")]
            public string buffDef;

        [Header("Timed Settings")]
            [Tooltip("Wether the Buff Expires on a Timer")]
            public bool isTimed;

            [Tooltip("How Long the Timed Buff Lasts")]
            [Min(0)]
            public float time;

            [Tooltip("How many stacks to give the timed buff.")]
            [Min(0)]
            public int stacks;
    }
}
