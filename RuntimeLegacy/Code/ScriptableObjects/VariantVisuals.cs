using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A ScriptableObject used to create a variant's Visuals
    /// </summary>
    [CreateAssetMenu(fileName = "New VariantVisuals", menuName = "VarianceAPI/VariantVisuals")]
    public class VariantVisuals : ScriptableObject
    {
        /// <summary>
        /// Represents a Material replacement
        /// </summary>
        [Serializable]
        public class VariantMaterialReplacement
        {
            [Tooltip("The rendererInfo's index to replace")]
            public int rendererIndex;

            [Tooltip("The material to use")]
            public Material material;
        }

        /// <summary>
        /// Represents a Light replacement
        /// </summary>
        [Serializable]
        public class VariantLightReplacement
        {
            [Tooltip("The lightInfo's index to replace")]
            public int rendererIndex;
            [Tooltip("The new color for the Light")]
            public Color32 color;
            [Tooltip("The type of light to use")]
            public LightType lightType;
        }

        /// <summary>
        /// Represents a Mesh replacement
        /// </summary>
        [Serializable]
        public class VariantMeshReplacement
        {
            [Tooltip("The RendererInfo's index to replace")]
            public int rendererIndex;
            [Tooltip("The new mesh to use")]
            public Mesh mesh;
            [Tooltip("The type of mesh, set this when needed")]
            public MeshType meshType;
        }

        [Tooltip("The VariantVisual's Material Replacements")]
        public VariantMaterialReplacement[] materialReplacements = Array.Empty<VariantMaterialReplacement>();
        [Tooltip("The VariantVisual's Light Replacements")]
        public VariantLightReplacement[] lightReplacements = Array.Empty<VariantLightReplacement>();
        [Tooltip("The VariantVisual's Mesh Replacements")]
        public VariantMeshReplacement[] meshReplacements = Array.Empty<VariantMeshReplacement>();

        /// <summary>
        /// Applies the material replacements in <see cref="materialReplacements"/> to the target model
        /// </summary>
        /// <param name="targetModel">The model to modify</param>
        public virtual void ApplyMaterials(CharacterModel targetModel)
        {
            for (int i = 0; i < materialReplacements.Length; i++)
            {
                var current = materialReplacements[i];
                targetModel.baseRendererInfos[current.rendererIndex].defaultMaterial = current.material;
            }
        }

        /// <summary>
        /// Applies the light replacements in <see cref="lightReplacements"/> to the target model
        /// </summary>
        /// <param name="targetModel">The model to modify</param>
        public virtual void ApplyLights(CharacterModel targetModel)
        {
            for (int i = 0; i < lightReplacements.Length; i++)
            {
                var current = lightReplacements[i];
                targetModel.baseLightInfos[current.rendererIndex].defaultColor = current.color;
                targetModel.baseLightInfos[current.rendererIndex].light.type = current.lightType;
            }
        }

        /// <summary>
        /// Applies the mesh replacements in <see cref="meshReplacements"/> to thge target model, and outputs the stored IDRS of the variant and the mesh type that was applied
        /// </summary>
        /// <param name="targetModel">The model to modify</param>
        /// <param name="storedIdrs">The stored IDRS of the variant</param>
        /// <param name="meshType">Tthe last mesh that was applied</param>
        /// <returns>True if the replacements where applied, false otherwise or if no mesh replacements are specified</returns>
        public virtual bool ApplyMeshes(CharacterModel targetModel, out ItemDisplayRuleSet storedIdrs, out MeshType meshType)
        {
            ItemDisplayRuleSet idrs = null;
            MeshType type = MeshType.Default;
            if (meshReplacements.Length == 0)
            {
                meshType = type;
                storedIdrs = idrs;
                return false;
            }

            for (int i = 0; i < meshReplacements.Length; i++)
            {
                var current = meshReplacements[i];

                if (current.meshType != MeshType.Default)
                    type = current.meshType;

                idrs = targetModel.itemDisplayRuleSet;
                targetModel.baseRendererInfos[current.rendererIndex].renderer.GetComponent<SkinnedMeshRenderer>().sharedMesh = current.mesh;
            }

            storedIdrs = idrs;
            meshType = type;
            return true;
        }
    }
}
