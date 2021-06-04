using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantMaterialReplacement", menuName = "VarianceAPI/VariantMaterialReplacement", order = 9)]
    public class VariantMaterialReplacement : ScriptableObject
    {
        [Header("Variant Material Replacement")]
            [Tooltip("Which RendererIndex You're Targetting")]
            [Min(0)]
            public int rendererIndex;

            [Tooltip("The Replacement Material")]
            public Material material;
    }
}
