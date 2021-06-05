using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Scriptables;

namespace VarianceAPI.Modules
{
    public class VariantMaterialGrabber
    {
        public AssetBundle assetBundle;
        public List<VariantMaterialReplacement> materialReplacements = new List<VariantMaterialReplacement>();
        internal VariantMaterialReplacement[] incompleteMaterialReplacements;
        public virtual void GrabIncompleteMaterialReplacements(AssetBundle assetBundle)
        {
            Debug.Log("VarianceAPI: Grabbing Incomplete Material Replacements from " + assetBundle.name);
            incompleteMaterialReplacements = assetBundle.LoadAllAssets<VariantMaterialReplacement>().Where(p => !p.material).ToArray();
        }
        public virtual void FixMaterials(List<VariantMaterialReplacement> completeMaterialReplacements)
        {
            foreach(VariantMaterialReplacement i in incompleteMaterialReplacements)
            {
                Debug.Log("VarianceAPI: Proceeding to replace null material in " + i.varName);
                ActuallyFixMaterial(i, completeMaterialReplacements);
            }
        }
        private void ActuallyFixMaterial(VariantMaterialReplacement incompleteMaterial, List<VariantMaterialReplacement> VariantMaterialReplacements)
        {
            var temp = new List<VariantMaterialReplacement>();
            temp = VariantMaterialReplacements;
            for (int i = 0; i < temp.Count; i++)
            {
                VariantMaterialReplacement replacement = temp[i];
                if (incompleteMaterial.varName != replacement.varName)
                {
                    continue;
                }
                else if (incompleteMaterial.varName == replacement.varName)
                {
                    Debug.Log("VarianceAPI: Found matching varName! replacing material with correct one!");
                    incompleteMaterial.material = replacement.material;
                    Debug.Log("VarianceAPI: Succesfully replaced material of " + incompleteMaterial.varName);
                    temp.Remove(replacement);
                }
            }
        }
    }
}
