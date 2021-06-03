using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Modules;
using VarianceAPI.Scriptables;
using VarianceAPI;
using UnityEngine;

namespace ExampleVariantPack.Variants
{
    /* Example coded variant.
     * This class gives an example on how to create Variants at runtime, useful if you do not know how to use thunderkit but you do have knowledge behind creating mods
     */
    public class ExampleCodedVariant
    {
        internal List<VariantInfo> CodedVariants;
        public void RegisterVariants()
        {
            var ExampleCodedVariant = ScriptableObject.CreateInstance<VariantInfo>();
            {
                ExampleCodedVariant.identifierName = "EVP_ExampleCodedVariant";
                ExampleCodedVariant.bodyName = "Wisp";
                ExampleCodedVariant.overrideName = "Example Coded Variant";
                ExampleCodedVariant.spawnRate = 50f;
                ExampleCodedVariant.givesRewards = true;
                ExampleCodedVariant.variantTier = VariantTier.Common;
                ExampleCodedVariant.customInventory = EVP_exampleCodedVariantInventory;
                ExampleCodedVariant.sizeModifier = Helpers.FlyingSizeModifier(5.0f);
                ExampleCodedVariant.healthMultiplier = 10f;
                ExampleCodedVariant.moveSpeedMultiplier = 0.1f;
                ExampleCodedVariant.attackSpeedMultiplier = 0.1f;
                ExampleCodedVariant.damageMultiplier = 10f;
                ExampleCodedVariant.armorMultiplier = 1f;
                ExampleCodedVariant.armorBonus = -50f;

                CodedVariants.Add(ExampleCodedVariant);
            }
            foreach (VariantInfo variant in CodedVariants)
            {
                if(variant.isModded)
                {
                    Helpers.AddModdedVariant(variant);
                }
                else
                {
                    Helpers.AddVariant(variant);
                }
            }
        }
        private static readonly ItemInfo[] EVP_exampleCodedVariantInventory = new ItemInfo[]
        {
            Helpers.SimpleItem("ExtraLife"),
            Helpers.SimpleItem("AlienHead", 10)
        };
    }
}
