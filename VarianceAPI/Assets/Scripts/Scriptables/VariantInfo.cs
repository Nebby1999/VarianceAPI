using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantInfo", menuName = "VarianceAPI/VariantInfo", order = 0)]
    public class VariantInfo : ScriptableObject
    {
        [Header("Important Settings")]
            [Tooltip("Unique name of this Variant")]
            public string identifierName;

            [Tooltip("The BodyName prefab to modify")]
            public string bodyName;

            [Tooltip("The overrideName, maximum 2 per Variant\nCan be null")]
            public VariantOverrideName[] overrideName = new VariantOverrideName[2];

            [Tooltip("Whether the body youre trying to access is from a mod, this is important.")]
            public bool isModded;

            [Tooltip("The Variant's Config Entries\nIf null, the variant cannot be configured\nCreates Config entries for the Variant's SpawnRate and wether it's Unique or not")]
            public VariantConfig variantConfig;

            [Tooltip("Whether the Variant youre creating can overlap with other variants\nGets overriden by VariantConfig if it exists")]
            public bool unique;

            [Tooltip("The Modifier to give to this variant's AI.\nDefault: No Changes\nUnstable: AI uses it's desperation attack whenever it wants\nForceSprint: AI Always sprints.")]
            public VariantAIModifier aiModifier;

            [Tooltip("The Variant's Tier\nCommon: Have no special differences\nUncommon: Have a red healthbar\nRare: Makes an unique sound\nLegendary: Announces arrival on chat.\nThe higher the tier, the more of these identification effects the variant has.")]
            public VariantTier variantTier;

            [Tooltip("The Variant's SpawnRate\nGets overriden by VariantConfig if it exists.\nAccepted Values range from 0 to 100")]
            [Range(0,100)]
            public float spawnRate;

        [Header("Reward Settings")]
            [Tooltip("Whether the variant gives rewards when it dies.")]
            public bool givesRewards;

            [Tooltip("Whether the variant uses custom rewards or the default reward system\nCan be null")]
            public CustomVariantReward customVariantReward;

            [Header("Custom Inventory & Skills")]

            [Tooltip("The Variant's Inventory\nCan be null")]
            public ItemInfo[] customInventory;

            [Tooltip("The Equipment the variant has\nCan Be null")]
            public EquipmentInfo customEquipment;

            [Tooltip("Whether the variant uses it's equipment")]
            public bool usesEquipment;

            [Tooltip("Buff given to the variant once it spawns\nCan be null")]
            public VariantBuff[] buff;

            [Tooltip("Replaces a Variant's Skill\nCan be null")]
            public VariantSkillReplacement[] skillReplacement;

        [Header("Stat Modifiers")]
            [Tooltip("Multiplier applied to the variant's Health\nWhere 1.0 = 100% Base health")]
            [Min(0)]
            public float healthMultiplier = 1;
            
            [Tooltip("Multiplier applied to the variant's Movement Speed\nWhere 1.0 = 100% Base Movement Speed")]
            [Min(0)]
            public float moveSpeedMultiplier = 1;

            [Tooltip("Multiplier applied to the variant's Attack Speed\nWhere 1.0 = 100% Base Movement Speed")]
            [Min(0)]
            public float attackSpeedMultiplier = 1;

            [Tooltip("Multiplier applied to the variant's Damage\nWhere 1.0 = 100% Base Damage")]
            [Min(0)]
            public float damageMultiplier = 1;

            [Tooltip("Multiplier applied to the variant's Armor, where 1.0 = 100% Base Armor")]
            [Min(0)]
            public float armorMultiplier = 1;

            [Tooltip("Extra armor of the Variant\nRefer to the wiki's page on armor for how to use")]
            public float armorBonus = 0;

        [Header("Other Settings")]

            [Tooltip("Message to Display on chat when the variant spawns, only works if the variant is Legendary")]
            public string arrivalMessage;

            [Tooltip("Replaces the variant's mesh, currently doesnt work.\nCan be null")]
            public VariantMeshReplacement[] meshReplacement;

            [Tooltip("Replaces the variant's Material\nCan be null")]
            public VariantMaterialReplacement[] materialReplacement;

        [Tooltip("Replaces the Variant's Light emitting properties\nCan be null")]
        public VariantLightReplacement[] lightReplacement;

            [Tooltip("Increases or Decreases the variant's Size\nCan be null")]
            public VariantSizeModifier sizeModifier;
            
            [Tooltip("Extra components to add to the variant, can be null.")]
            public VariantExtraComponent[] extraComponents;

            [Tooltip("Wether or not to replace a Variant's DeathState\nThis needs to be the combination of the Namespace, alongside the class name\nExample: YourNameSpace.VariantEntityStates.Jellyfish.MOAJDeathState")]
            public string customDeathState;
    }
}
