using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantSizeModifier", menuName = "VarianceAPI/VariantSizeModifier", order = 6)]
    public class VariantSizeModifier : ScriptableObject
    {
        /// <summary>
        /// The new size of the monster, where 1.0 = 100% base size
        /// </summary>
        public float newSize;
        /// <summary>
        /// Wether the size modifier scales colliders, useful for flying enemies.
        /// </summary>
        public bool scaleCollider;
    }
}
