using RoR2;
using System;
using UnityEngine;

namespace VAPI.Legacy
{
    [Obsolete("VarianceAPI's 2.0 systems are obsolete, there's a menu item under \"Tools/VAPI\" that's used to migrate VariantVisuals into CharacterVariantVisualModifier")]
    public class VariantVisuals : ScriptableObject
    {
        [Serializable]
        public class VariantMaterialReplacement
        {
            public int rendererIndex;

            public Material material;
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

        public VariantMaterialReplacement[] materialReplacements = Array.Empty<VariantMaterialReplacement>();
        public VariantLightReplacement[] lightReplacements = Array.Empty<VariantLightReplacement>();
        public VariantMeshReplacement[] meshReplacements = Array.Empty<VariantMeshReplacement>();
    }
}
