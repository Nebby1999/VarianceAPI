using EntityStates;
using HG;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using VAPI.Legacy.Components;

namespace VAPI.Legacy
{
    /// <summary>
    /// A ScriptableObject that represents a new Variant
    /// </summary>
    [CreateAssetMenu(fileName = "AssCunt", menuName = "VAPI/AssCunt")]
    public class VariantDef : ScriptableObject
    {
        public string bodyName;

        [SerializeField, FormerlySerializedAs("variantTier")] internal VariantTierIndex _variantTier;
        [SerializeField, FormerlySerializedAs("variantTierDef")] internal VariantTierDef _variantTierDef;

        public bool isUnique = false;

        [Range(0, 100)]
        public float spawnRate;

        public VariantSpawnCondition variantSpawnCondition;

        public string arrivalToken;

        public VariantInventory variantInventory;

        public SerializableEntityStateType deathStateOverride;

        public VariantSkillReplacement[] skillReplacements = Array.Empty<VariantSkillReplacement>();

        [Min(0)]
        public float healthMultiplier = 1;

        public float regenBonus = 0;

        [Min(0)]
        public float regenMultiplier = 0;

        public float shieldBonus = 0;

        [Min(0)]
        public float shieldMultiplier = 1;

        [Min(0)]
        public float moveSpeedMultiplier = 1;

        [Min(0)]
        public float attackSpeedMultiplier = 1;

        [Min(0)]
        public float damageMultiplier = 1;

        public float armorBonus = 0;

        [Min(0)]
        public float armorMultiplier = 1;

        public VariantVisuals visualModifier;

        public VariantSizeModifier sizeModifier;

        public BasicAIModifier aiModifier;

        public float baseAIDampBonus;

        [Min(0)]
        public float baseAIDampMultiplier = 1;

        public VariantOverrideName[] nameOverrides = Array.Empty<VariantOverrideName>();

        public VariantComponentProvider[] componentProviders = Array.Empty<VariantComponentProvider>();

        [Serializable]
        public class VariantOverrideName
        {
            public string token;
            public OverrideNameType overrideType;
        }

        [Serializable]
        public class VariantSkillReplacement
        {
            public SkillDef skillDef;
            public SkillSlot skillSlot;
        }

        [Serializable]
        public class VariantComponentProvider
        {
            [SerializableSystemType.RequiredBaseType(typeof(VariantComponent))]
            public SerializableSystemType componentToAdd;
            /// <summary>
            /// how the component will be attached
            /// </summary>
            public ComponentAttachmentType attachmentType;
        }
    }
}
