using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VarianceAPI.Scriptables;
using System.Linq;
using System.Reflection;
using Logger = VarianceAPI.MainClass;

namespace VarianceAPI.Modules
{
    /// <summary>
    /// The Variant Material Grabber Class functions as a way to load INGAME materials.
    /// <para>This class and its methods must be run before VarianceAPI registers your variants.</para>
    /// </summary>
    public class VariantMaterialGrabber
    {
        [Tooltip("Load your AssetBundle on this variable.")]
        public AssetBundle assetBundle;

        [Tooltip("This list contains all your incomplete VariantMaterialReplacements made in thunderkit\n" +
            "The VariantMaterialGrabber class Identifies an incomplete material by having...\n" +
            "A: Material set to Null\n" +
            "B: Unique Identifier not being null.")]
        public List<VariantMaterialReplacement> incompleteVariantMaterials = new List<VariantMaterialReplacement>();

        [Tooltip("This list must contain all the completed variant materials made in code.\n" +
            "A completed variant material replacement must match it's incomplete version' Unique Identifier.")]
        public List<VariantMaterialReplacement> completeVariantsMaterials = new List<VariantMaterialReplacement>();

        public void Init()
        {
            GrabIncompleteVariantMaterials(assetBundle);
            StartReplacement();
        }
        /// <summary>
        /// Loads all the incompelte VariantMaterialReplacements and stores them in the incompleteVariantMaterials list.
        /// </summary>
        /// <param name="yourAssets"> Your mod's Asset Bundle.</param>
        public virtual void GrabIncompleteVariantMaterials(AssetBundle yourAssets)
        {
            Logger.Log.LogMessage("Grabbing the Incomplete Variant Materials from " + yourAssets.name);
            incompleteVariantMaterials = assetBundle.LoadAllAssets<VariantMaterialReplacement>().Where(
                VMR => VMR.material == null && VMR.identifier != null).ToList();
        }
        public void StartReplacement()
        {
            if(incompleteVariantMaterials == null)
            {
                Logger.Log.LogError("IncompleteVariantMaterials list is null! Aborting...");
                return;
            }
            foreach(VariantMaterialReplacement variantMaterialReplacement in incompleteVariantMaterials)
            {
                ReplaceIncompleteVariantMaterialsWithCompleteVariantMaterials(variantMaterialReplacement);
            }
        }
        public virtual void ReplaceIncompleteVariantMaterialsWithCompleteVariantMaterials(VariantMaterialReplacement incompleteVariantMaterial)
        {
            List<VariantMaterialReplacement> completeReplacements = new List<VariantMaterialReplacement>(completeVariantsMaterials);
            foreach (VariantMaterialReplacement completeReplacement in completeReplacements)
            {
                if(incompleteVariantMaterial.identifier == completeReplacement.identifier)
                {
                    Logger.Log.LogMessage("Found matching VariantMaterialReplacement! fixing the Incomplete variant material...");
                    incompleteVariantMaterial.material = completeReplacement.material;
                    Logger.Log.LogMessage("Replaced " + incompleteVariantMaterial.identifier + "'s Material with the correct one!");
                }
                else
                {
                    continue;
                }
            }
        }
    }
}