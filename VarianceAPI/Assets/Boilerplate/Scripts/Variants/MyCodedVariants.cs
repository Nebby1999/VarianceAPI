using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VarianceAPI;
using VarianceAPI.Modules;
using VarianceAPI.Scriptables;

namespace YourPackNameHere.Variants
{
    /// <summary>
    /// This class shows you how to start creating Variants in code.
    /// <para>While this is not the ideal method that VarianceAPI reccomends, it is still a viable way if you do not want to use Thunderkit.</para>
    /// </summary>
    public class MyCodedVariants
    {
        /// <summary>
        /// This list will contain all your variants made in code, ALWAYS add the variant youre creating to this list.
        /// </summary>
        internal List<VariantInfo> CodedVariants = new List<VariantInfo>();

        /// <summary>
        /// The init method is your main method where you create your Variants.
        /// </summary>
        /// <param name="config">Your Plugin's Config File.</param>
        public void Init(ConfigFile config)
        {
            CreateInventories();
            //We start by declaring a new instance of VariantInfo using ScriptableObject.CreateInstance()
            var exampleCodedVariant = ScriptableObject.CreateInstance<VariantInfo>();
            //Example Coded Variant
            {
                exampleCodedVariant.identifierName = "YPNH_ExampleCodedVariant";
                exampleCodedVariant.bodyName = "Beetle";
                exampleCodedVariant.overrideName = Helpers.CreateOverrideName("Cool Lookin'", OverrideNameType.Preffix); //<--- Remember, the Helpers class in VarianceAPI contains lots of methods for creating specific Scriptable Objects.
                exampleCodedVariant.unique = true;
                exampleCodedVariant.aiModifier = VariantAIModifier.ForceSprint;
                exampleCodedVariant.variantTier = VariantTier.Legendary;
                exampleCodedVariant.spawnRate = 50f;
                exampleCodedVariant.givesRewards = true;
                exampleCodedVariant.customVariantReward = Helpers.CreateVariantReward(10f, 420, 1f, 69, 50f, 25f, 10f);
                exampleCodedVariant.variantInventory = YPNH_ExampleCoddedVariantInventory;
                exampleCodedVariant.healthMultiplier = 10f;
                exampleCodedVariant.moveSpeedMultiplier = 0.5f;
                exampleCodedVariant.attackSpeedMultiplier = 2;
                exampleCodedVariant.damageMultiplier = 1;
                exampleCodedVariant.armorMultiplier = 1;
                exampleCodedVariant.armorBonus = 5f;
                exampleCodedVariant.sizeModifier = Helpers.GroundSizeModifier(3f);
                exampleCodedVariant.arrivalMessage = "SUPRISE MOTHERFUCKER!";
                exampleCodedVariant.variantConfig = Helpers.CreateConfig(exampleCodedVariant); //<---- We want to create the Config last, because Helpers.CreateConfig() uses the values you input in the VariantInfo.
                CodedVariants.Add(exampleCodedVariant); //<--- Once you finish creating your Variant, you add the VariantInfo to the CodedVariants list.
            }
            RegisterVariants(CodedVariants, config); //<--- This method begins registering your coded variants. ALWAYS load this method last! if you load it before some variants might be missing from the list!
        }

        /// <summary>
        /// This method just calls VariantRegister's RegisterConfigs() method that take a List<> and a Coinfig File.
        /// </summary>
        /// <param name="variantInfo">Your list containing all your variantInfos made in code.</param>
        /// <param name="config">Your Mod's Config file.</param>
        private void RegisterVariants(List<VariantInfo> variantInfo, ConfigFile config)
        {
            var VR = new VariantRegister();
            VR.RegisterConfigs(variantInfo, config);
        }

        /// <summary>
        /// This is an easy way of creating an Inventory for a variant that has more than 2 items.
        /// </summary>
        internal VariantInventory YPNH_ExampleCoddedVariantInventory = ScriptableObject.CreateInstance<VariantInventory>();

        internal void CreateInventories()
        {
            YPNH_ExampleCoddedVariantInventory.counts = new int[2] { 10, 4 };
            YPNH_ExampleCoddedVariantInventory.itemStrings = new string[2] { "CritGlasses", "Behemoth" };
        }
    }
}