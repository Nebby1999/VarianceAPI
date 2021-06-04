using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantMeshReplacement", menuName = "VarianceAPI/VariantMeshReplacement", order = 8)]
    public class VariantMeshReplacement : ScriptableObject
    {
        [Header("Variant Mesh Replacement - NYI")]
            [Tooltip("Which RendererIndex You're Targetting")]
            [Min(0)]
            public int rendererIndex;
        
            [Tooltip("The Replacement Mesh")]
            public Mesh mesh;
    }
}
