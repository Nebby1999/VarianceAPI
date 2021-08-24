using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    public class VariantMaterialReplacement : ScriptableObject
    {
        [Header("Variant Material Replacement")]
            [Tooltip("Which RendererIndex You're Targetting")]
            [Min(0)]
            public int rendererIndex;

            [Tooltip("The Replacement Material\nIf you want to use a vanilla material, leave this null and load the VariantMaterialReplacement in your class that inherits from VariantMaterialGrabber")]
            public Material material;

            [Tooltip("Unique identifier of this material replacement.\nUsed on a class that inherits from VariantMaterialGrabber")]
            public string identifier;
    }
}
