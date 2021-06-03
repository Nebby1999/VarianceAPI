using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Modules;
using VarianceAPI.Scriptables;
using VarianceAPI;

namespace ExampleVariantPack.Variants
{
    /* Example coded variant.
     * This class gives an example on how to create Variants at runtime, useful if you do not know how to use thunderkit but you do have knowledge behind creating mods
     */
    public class ExampleCodedVariant
    {
        public void RegisterVariants()
        {
            Helpers.AddVariant(new VariantInfo
            {
                identifierName = "EVP_BigAssWisp",
                bodyName = "Wisp",
                overrideName = "Big Ass Wisp",
                spawnRate = 50f,
                givesRewards = true,
                variantTier = VariantTier.Common,
                customInventory = bigAssWispInventory,
                sizeModifier = Helpers.FlyingSizeModifier(5.0f),
                healthMultiplier = 10f,
                moveSpeedMultiplier = 0.1f,
                attackSpeedMultiplier = 0.1f,
                damageMultiplier = 10f,
                armorMultiplier = 1f,
                armorBonus = -100f,
            });;
        }

        private static readonly ItemInfo[] bigAssWispInventory = new ItemInfo[]
        {
            Helpers.SimpleItem("ExtraLife"),
            Helpers.SimpleItem("AlienHead", 10)
        };
    }
}
