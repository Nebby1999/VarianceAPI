using RoR2;
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

        public virtual void ApplyMaterials(CharacterModel targetModel)
        {
            for(int i = 0; i < materialReplacements.Length; i++)
            {
                var current = materialReplacements[i];
                targetModel.baseRendererInfos[current.rendererIndex].defaultMaterial = current.material;
            }
        }

        public virtual void ApplyLights(CharacterModel targetModel)
        {
            for (int i = 0; i < lightReplacements.Length; i++)
            {
                var current = lightReplacements[i];
                targetModel.baseLightInfos[current.rendererIndex].defaultColor = current.color;
                targetModel.baseLightInfos[current.rendererIndex].light.type = current.lightType;
            }
        }

        public virtual bool ApplyMeshes(CharacterModel targetModel, out ItemDisplayRuleSet storedIdrs, out MeshType meshType)
        {
            ItemDisplayRuleSet idrs = null;
            MeshType type = MeshType.Default;
            if(meshReplacements.Length == 0)
            {
                meshType = type;
                storedIdrs = idrs;
                return false;
            }

            for(int i = 0; i < meshReplacements.Length; i++)
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
