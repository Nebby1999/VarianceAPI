using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantMeshReplacement", menuName = "VarianceAPI/VariantMeshReplacement", order = 9)]
    public class VariantMeshReplacement : ScriptableObject
    {
        /// <summary>
        /// Which rendererInfo youre trying to replace the mesh of
        /// </summary>
        public int rendererIndex;

        /// <summary>
        /// Mesh to replace it with
        /// </summary>
        public Mesh mesh;
    }
}
