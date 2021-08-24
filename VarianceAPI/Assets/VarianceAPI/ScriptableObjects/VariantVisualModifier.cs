using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.ScriptableObjects
{

    [CreateAssetMenu(fileName = "VariantVisualModifier", menuName = "VarianceAPI/VariantVisualModifier")]
    public class VariantVisualModifier : ScriptableObject
    {
        [Serializable]
        public struct VariantMaterialReplacement
        {
            [Tooltip("The name of this specific Replacement, not used in anything, just used for organization in the editor.")]
            public string ElementName;

            [Space]


            [Tooltip("Which RendererIndex You're Targetting")]
            [Min(0)]
            public int rendererIndex;

            [Tooltip("The Replacement Material\nIf you want to use a vanilla material, leave this null and load the VariantMaterialReplacement in your class that inherits from VariantMaterialGrabber")]
            public Material material;

            [Tooltip("Unique identifier of this material replacement.\nUsed on a class that inherits from VariantMaterialGrabber")]
            public string identifier;
        }
        [Serializable]
        public struct VariantLightReplacement
        {
            [Tooltip("The name of this specific Replacement, not used in anything, just used for organization in the editor.")]
            public string ElementName;

            [Space]


            [Tooltip("Which RendererIndex You're Targetting")]
            [Min(0)]
            public int rendererIndex;

            [Tooltip("The Replacement Color")]
            public Color color;

            [Tooltip("What kind of light the replacement uses.")]
            public LightType lightType;
        }

        [Serializable]
        public struct VariantMeshReplacement
        {
            [Tooltip("The name of this specific Replacement, not used in anything, just used for organization in the editor.")]
            public string ElementName;

            [Space]


            [Tooltip("Which RendererIndex You're Targetting")]
            [Min(0)]
            public int rendererIndex;

            [Tooltip("The Replacement Mesh")]
            public Mesh mesh;

            [Tooltip("What kind of mesh youre replacing.\nUsed to correctly assign bones in certain mesh replacements.")]
            public MeshType meshType;
        }

        public VariantMaterialReplacement[] MaterialReplacements = Array.Empty<VariantMaterialReplacement>();

        public VariantLightReplacement[] LightReplacements = Array.Empty<VariantLightReplacement>();

        public VariantMeshReplacement[] MeshReplacements = Array.Empty<VariantMeshReplacement>();

    }
}
