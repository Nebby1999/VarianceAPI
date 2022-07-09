using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantVisuals", menuName = "VarianceAPI/VariantVisuals")]
    public class VariantVisuals : ScriptableObject
    {
        public VariantMaterialReplacement[] materialReplacements = Array.Empty<VariantMaterialReplacement>();
        public VariantLightReplacement[] lightReplacements = Array.Empty<VariantLightReplacement>();
        public VariantMeshReplacement[] meshReplacements = Array.Empty<VariantMeshReplacement>();

        [Serializable]
        public class VariantMaterialReplacement
        {
            public int rendererIndex;

            public AddressableMaterial material;
        }

        [Serializable]
        public class VariantLightReplacement
        {
            public int rendererIndex;
            public Color32 color;
            public LightType lightType;
        }

        [Serializable]
        public class VariantMeshReplacement
        {
            public int rendererIndex;

            public Mesh mesh;

            public MeshType meshType;
        }
    }
}
