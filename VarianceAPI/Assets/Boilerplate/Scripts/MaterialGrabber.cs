using RoR2;
using UnityEngine;
using VarianceAPI.Modules;
using VarianceAPI.Scriptables;

namespace YourPackNameHere
{
    /// <summary>
    /// The MaterialGrabber class contains methods for loading Risk of Rain 2 Materials.
    /// <para>Most of RiskOfRain2's Materials cannot be obtained using Resources.Load() as such, we need to get them in specific Ways, and cannot be easily obtained using Scriptable Objects</para>
    /// <para>Consider this class as a "VariantMaterialReplacement" factory. where you create the correct VariantMaterialReplacements in code and then use the Init() method for replacing the incomplete VariantMaterialReplacements in your AssetBundle.</para>
    /// <para>VariantMaterialGrabber loads incompelte VariantMaterialReplacement that follow the following criteria...</para>
    /// <para>The Material set must be null</para>
    /// <para>The identifier must not be an Empty string.</para>
    /// </summary>
    public class MaterialGrabber : VariantMaterialGrabber
    {
        /// <summary>
        /// A very easy way to get Item's Materials is to load their Displays using an ItemDisplayRuleSet.
        /// </summary>
        public ItemDisplayRuleSet IDRS;

        /// <summary>
        /// Always initialize your Materials.
        /// </summary>
        public Material moltenPerforatorMaterial;
        public Material solusControlUnitMaterial;

        /// <summary>
        /// This is the method you call in your MainClass for grabbing materials and eventually fixing the incomplete ones.
        /// </summary>
        /// <param name="assets">Your VariantPack's Assets.</param>
        public void StartGrabber(AssetBundle assets)
        {
            assetBundle = assets; //<---- We set the assetBundle variable that's in VariantMaterialGrabber to our asset bundle, this way, VarianceAPI can automatically load all the incomplete VariantMaterialReplacements.
            CreateCorrectMaterials();
            Init(); // <---- This method in VariantMaterialGrabber replaces your incomplete VariantMaterialReplacements with the correct ones.
        }

        /// <summary>
        /// This method is where you will be storing and creating your correct VariantMaterialReplacements.
        /// <para>If you do not know how to get more materials, you can use the moltenPerforatorMaterial's value for items or the solusControlUnitMaterial's for materials found in CharacterBodies.</para>
        /// </summary>
        public void CreateCorrectMaterials()
        {
            //We load Commando's ItemDisplayRuleSet so we can get the materials itemDisplays use.
            IDRS = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;

            //Loading the Molten Perforator's Material.
            moltenPerforatorMaterial = Object.Instantiate(IDRS.FindDisplayRuleGroup(RoR2Content.Items.FireballsOnHit).rules[0].followerPrefab.GetComponentInChildren<MeshRenderer>().material);
            CreateVMR(moltenPerforatorMaterial, "PerforatorMaterial");

            //Loading the Solus Control Unit's Material.
            solusControlUnitMaterial = Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/RoboBallBossBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            CreateVMR(solusControlUnitMaterial, "SolusControlUnitMaterial");
        }

        /// <summary>
        /// This method creates your VariantMaterialReplacement. and adds it to the "completeVariantsMaterials" list found in VariantMaterialGrabber.
        /// <para>completeVariantsMaterials are your materials that'll replace the incomplete ones.</para>
        /// </summary>
        /// <param name="material">The Material that the incomplete variant material replacement will inherit</param>
        /// <param name="identifier">The VariantMaterialReplacement's unique identifier. this MUST match the one set in the Editor or else the replacement wont happen!!!!</param>
        public void CreateVMR(Material material, string identifier)
        {
            VariantMaterialReplacement variantMaterialReplacement = ScriptableObject.CreateInstance<VariantMaterialReplacement>();
            variantMaterialReplacement.identifier = identifier;
            variantMaterialReplacement.material = material;

            completeVariantsMaterials.Add(variantMaterialReplacement);
        }
    }
}
