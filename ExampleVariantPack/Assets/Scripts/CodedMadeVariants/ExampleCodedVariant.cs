using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Modules;
using VarianceAPI.Scriptables;
using VarianceAPI;
using UnityEngine;
using ExampleVariantPack.ThunderkitMadeVariants;
using BepInEx.Configuration;

namespace ExampleVariantPack.CodedMadeVariants
{
    /* Example coded variant.
     * This class gives an example on how to create Variants at runtime, useful if you do not know how to use thunderkit but you do have knowledge behind creating mods
     */
    public class ExampleCodedVariant
    {
        internal List<VariantInfo> CodedVariants = new List<VariantInfo>();
        internal VariantInfo[] variantInfoArray;
        public void Init(ConfigFile config)
        {
            var exampleVariant = ScriptableObject.CreateInstance<VariantInfo>();
            //Example Coded Variant
                exampleVariant.identifierName = "EVP_ExampleCodedVariant";
                exampleVariant.bodyName = "Wisp";
                exampleVariant.overrideName = "Example Coded Variant";
                exampleVariant.spawnRate = 50f;
                exampleVariant.givesRewards = true;
                exampleVariant.variantTier = VariantTier.Common;
                exampleVariant.customInventory = EVP_exampleCodedVariantInventory;
                exampleVariant.sizeModifier = Helpers.FlyingSizeModifier(5.0f);
                exampleVariant.healthMultiplier = 10f;
                exampleVariant.moveSpeedMultiplier = 0.1f;
                exampleVariant.attackSpeedMultiplier = 0.1f;
                exampleVariant.damageMultiplier = 10f;
                exampleVariant.armorMultiplier = 1f;
                exampleVariant.armorBonus = -50f;
                exampleVariant.variantConfig = Helpers.CreateConfig(exampleVariant);
                CodedVariants.Add(exampleVariant);
            RegisterVariants(CodedVariants, config);
        }
        private void RegisterVariants(List<VariantInfo> variantInfo, ConfigFile config)
        {
            var VCC = new VariantRegister();
            VCC.RegisterCodedVariantConfigs(variantInfo, config);
        }
        private static readonly ItemInfo[] EVP_exampleCodedVariantInventory = new ItemInfo[]
        {
            Helpers.SimpleItem("ExtraLife"),
            Helpers.SimpleItem("AlienHead", 10)
        };
    }
}
