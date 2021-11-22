using RoR2;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityStates;
using HG;
using VarianceAPI.Components;

namespace VarianceAPI.ScriptableObjects
{
    [CreateAssetMenu(fileName = "VariantInfo", menuName = "VarianceAPI/VariantInfo")]
    public class VariantInfo : ScriptableObject
    {
        [Serializable]
        public struct VariantOverrideName
        {
            [Tooltip("The text to add to the variant's original name")]
            public string textToAdd;

            [Tooltip("Override order of this override Name\nPrefix: The text to add is applied before the Variant's original name\nSuffix: The text to add is applied after the Variant's original name")]
            public OverrideNameType overrideType;
        }
        [Serializable]
        public struct VariantSkillReplacement
        {
            [Tooltip("Skillslot to apply the replacement\nYou can use DevDebugToolkit's Spawn_As Command for knowing what skillslot you must target")]
            public SkillSlot skillSlot;

            [Tooltip("The Replacement Skill")]
            public SkillDef skillDef;
        }

        [Serializable]
        public struct VariantExtraComponent
        {
            [Tooltip("What component to add to the Variant\nYou should probably use RoR2EditorKit for filling this field.")]
            [SerializableSystemType.RequiredBaseType(typeof(VariantComponent))]
            public SerializableSystemType componentToAdd;

            [Tooltip("Where the component will be added.\nModel = Attatches to the mdl game object.\nBody = Attatches to the Body game object\nMaster = Atatches to the Master game object.")]
            public ComponentType componentType;
        }

        [Header("Important Settings")]
        [Tooltip("A unique identifier for this variantInfo.")]
        public string identifier;

        [Tooltip("The CharacterBody prefab to modify")]
        public string bodyName;

        [Tooltip("Add to, or Override the body's name. Can be left empty.")]
        public VariantOverrideName[] overrideNames;

        [Tooltip("Wether the Variant youre creating can overlap with other variants.")]
        public bool unique;

        [Tooltip("The Modifier to give to this variant's AI.\nDefault: No Changes\nUnstable: AI uses it's desperation attack whenever it wants\nForceSprint: AI Always sprints.")]
        public VariantAIModifier aiModifier;

        [Tooltip("The Variant's Tier\nCommon: Have no special differences\nUncommon: Have a red healthbar\nRare: Makes an unique sound\nLegendary: Announces arrival on chat.\nThe higher the tier, the more of these identification effects the variant has.")]
        public VariantTier variantTier;

        [Tooltip("The Variant's SpawnRate\nAccepted Values range from 0 to 100")]
        [Range(0, 100)]
        public float spawnRate;

        [Header("Custom Inventory & Skills")]

        [Tooltip("The Variant's Inventory, includes:\nItem Inventory.\nBuffs\nEquipment usage.\nCan be left empty")]
        public VariantInventoryInfo variantInventory;

        [Tooltip("Replaces a variant's skills, Can be left empty.")]
        public VariantSkillReplacement[] skillReplacements;

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

        [Header("Visual changes")]
        
        [Tooltip("Visual changes applied to the variant.")]
        public VariantVisualModifier visualModifier;

        [Tooltip("Increases or Decreases the variant's Size\nCan be null")]
        public VariantSizeModifier sizeModifier;

        [Header("Other Settings")]

        [Tooltip("Message to Display on chat when the variant spawns, only works if the variant is Rare or Legendary.\nCan be left null")]
        public string arrivalMessage;

        [Tooltip("Extra Components to add to the variant, Can be left null.")]
        public VariantExtraComponent[] extraComponents;

        [Tooltip("Wether or not to replace a Variant's DeathState\nCan be left null")]
        public SerializableEntityStateType customDeathState;
    }
}
