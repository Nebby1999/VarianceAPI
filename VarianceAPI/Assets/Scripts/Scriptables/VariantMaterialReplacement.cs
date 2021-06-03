using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantMaterialReplacement", menuName = "VarianceAPI/VariantMaterialReplacement", order = 8)]
    public class VariantMaterialReplacement : ScriptableObject
    {
        /// <summary>
        /// Which rendererInfo youre trying to replace the material of
        /// </summary>
        public int rendererIndex;

        /// <summary>
        /// Material to replace it with
        /// </summary>
        public Material material;
    }
}
