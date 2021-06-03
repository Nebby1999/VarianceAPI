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
        /// <summary>
        /// Unique identifier of the variant
        /// </summary>
        public string identifierName;

        /// <summary>
        /// name of the body to apply this variant info to
        /// </summary>
        public string bodyName;

        /// <summary>
        /// new name for the variant, this name will pop up when the entity is pinged.
        /// </summary>
        public string overrideName;

        /// <summary>
        /// Wether this variant is unique or not, unique variants cannot overlap with other variants.
        /// </summary>
        public bool unique;

        /// <summary>
        /// Wether the body for this variant comes from a mod, this is important.
        /// </summary>
        public bool isModded;

        /// <summary>
        /// The AI Modifier to apply to this variant.
        /// <para>Default causes no changes.</para>
        /// <para>Unstable causes the variant to use their desperation attack regardless of health.</para>
        /// <para>ForceSprint causes the variant to always sprint.</para>
        /// </summary>
        public VariantAIModifier aiModifier;

        /// <summary>
        /// The variant's Spawn Rate: 0 - 100, accepts decimal numbers
        /// </summary>
        public float spawnRate;

        /// <summary>
        /// The tier of the variant.
        /// <para>Uncommons have a red Health Bar.</para>
        /// <para>Rares make a unique spawn sound.</para>
        /// <para>Legendaries announce their arrival in the chat.</para>
        /// </summary>
        public VariantTier variantTier;

        /// <summary>
        /// Inventory the variant spawns with
        /// </summary>
        public ItemInfo[] customInventory;

        /// <summary>
        /// Equipment to give to the variant once it spawns.
        /// </summary>
        public EquipmentInfo customEquipment;

        /// <summary>
        /// Wether the variant can use equipments
        /// </summary>
        public bool usesEquipment;

        /// <summary>
        /// Replaces the variant's mesh, just leave it null unless you know how to grab meshes from other ingame sources or build assetbundles
        /// </summary>
        public VariantMeshReplacement[] meshReplacement;

        /// <summary>
        /// Replaces the variant's material, just leave it null unless you know how to grab materials from other ingame sources or build assetBundles
        /// </summary>
        public VariantMaterialReplacement[] materialReplacement;

        /// <summary>
        /// Replaces a Variant's Skill, leave it null unless you know what youre doing
        /// </summary>
        public VariantSkillReplacement[] skillReplacement;

        /// <summary>
        /// Increases or Decreases a variant's size.
        /// </summary>
        public VariantSizeModifier sizeModifier;

        /// <summary>
        /// Buff given to the variant once it spawns
        /// </summary>
        public VariantBuff[] buff;

        /// <summary>
        /// Multiplier applied to the variant's health, where 1.0 = 100% base health
        /// </summary>
        public float healthMultiplier;

        /// <summary>
        /// Multiplier applied to the variant's movement speed, where 1.0 = 100% Base Movement Speed
        /// </summary>
        public float moveSpeedMultiplier;

        /// <summary>
        /// Multiplier applied to the variant's attack speed, where 1.0 = 100% Base attack speed
        /// </summary>
        public float attackSpeedMultiplier;

        /// <summary>
        /// Multiplier applied to the variant's damage stat, where 1.0 = 100% base damage
        /// </summary>
        public float damageMultiplier;

        /// <summary>
        /// Multiplier applied to the variant's armor stat, where 1.0 = 100% base armor
        /// </summary>
        public float armorMultiplier;

        /// <summary>
        /// Extra armor to give to the variant, refer to the wiki's page on armor.
        /// </summary>
        public float armorBonus;

        /// <summary>
        /// Wether the variant gives rewards when it dies.
        /// </summary>
        public bool givesRewards;

        /// <summary>
        /// The rewards the variant gives when it dies.
        /// Replaces the default rewards given by it's tier.
        /// </summary>
        public CustomVariantReward customVariantReward;

        /// <summary>
        /// Message to display on chat when the variant spawns, only works if the variant is Legendary
        /// </summary>
        public string arrivalMessage;
    }
}
