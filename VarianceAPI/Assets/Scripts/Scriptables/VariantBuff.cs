using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantBuff", menuName = "VarianceAPI/VariantBuff", order = 5)]
    public class VariantBuff : ScriptableObject
    {
        /// <summary>
        /// The Buff's BuffDef
        /// </summary>
        public string buffDef;
        
        /// <summary>
        /// Wether or not the buff applies has an expiration timer.
        /// </summary>
        public bool isTimed;

        /// <summary>
        /// How long the timed buff lasts
        /// </summary>
        public float time;
    }
}
