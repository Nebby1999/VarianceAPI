using System;
using UnityEngine;
using UnityEngine.UIElements;
using EntityStates;
using HG;
using UnityEngine.Networking;
using R2API.AddressReferencedAssets;
using RoR2;
using MSU;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx;
using VAPI.AddressReferencedAssets;
#nullable enable

namespace VAPI
{
    /// <summary>
    /// Main ScriptableObject that defines a Variant for a character. These are catalogued by the <see cref="CharacterVariantCatalog"/>.
    /// <br></br>
    /// When a variant is added to the catalog via a <see cref="VariantPack"/>, and it's <see cref="targetBody"/> points to a valid <see cref="CharacterBody"/> prefab, the variant becomes available for spawning via the character's <see cref="CharacterVariantProvider"/>. This provider is later used for creating them in a run.
    /// <para></para>
    /// It is extremely recommended to create new ones in code via the <see cref="CharacterVariantDefBuilder"/>, or by cloning an existing one using <see cref="CharacterVariantDef.CreateInstance(CharacterVariantDef, string)"/>
    /// </summary>
    [CreateAssetMenu(fileName = "New CharacterVariantDef", menuName = "VarianceAPI/CharacterVariantDef")]
    public sealed class CharacterVariantDef : CachedNameScriptableObject
    {
        /// <summary>
        /// The <see cref="CharacterVariantIndex"/> assigned to this variant def
        /// </summary>
        public CharacterVariantIndex characterVariantIndex { get; internal set; } = CharacterVariantIndex.none;

        [Header("General Settings")]
        [Tooltip("A Target body for this variant. This variant will be available to this body.")]
        public CharacterVariantTarget targetBody = new CharacterVariantTarget();
        [Tooltip("The tier for this variant, can be left null")]
        public CharacterVariantTierDef? variantTier = null;

        [Tooltip("Wether this variant is unique or not. Unique variants cannot merge with other variants")]
        public bool isUnique;

        [Tooltip("Wether this variant is hidden from the RuleBook options. Hidden variants will also not get config entries for the uniqueness and spawn rate settings")]
        public bool isHidden;

        [Tooltip("The spawn rate for this variant, 0 -> 100")]
        [Range(0, 100)]
        public float spawnRate;

        [Tooltip("An arrival token for this variant when it spawns, only used if it's tier announces arrivals")]
        public string arrivalToken = "";

        [Header("Spawn Conditions")]
        [SerializeReference, SubclassSelector]
        [Tooltip("A spawn condition for this variant. This may be left null")]
        public IVariantSpawnCondition? spawnCondition = null;

        [Header("Master Related")]
        [SerializeReference, SubclassSelector]
        [Tooltip("An array of master modifiers for this variant")]
        public IVariantMasterModifier[] masterModifiers = Array.Empty<IVariantMasterModifier>();

        [Tooltip("An inventory definition for this variant, holding information like items and equipments")]
        public VariantInventoryDefinition inventoryDefinition = new VariantInventoryDefinition();

        [Header("Body Related")]

        [SerializeReference, SubclassSelector]
        [Tooltip("An array of body modifiers for this variant")]
        public IVariantBodyModifier[] bodyModifiers = Array.Empty<IVariantBodyModifier>();

        [SerializeReference, SubclassSelector]
        [Tooltip("A name provider for this variant, used to change the display name. This may be null")]
        public IVariantNameProvider? variantNameProvider = null;

        [Tooltip("A death state override for this variant")]
        public VariantDeathStateOverride deathStateOverride = new VariantDeathStateOverride();

        [Tooltip("An array of skill replacements for this variant")]
        public VariantSkillReplacement[] skillReplacements = Array.Empty<VariantSkillReplacement>();

        [SerializeReference, SubclassSelector]
        [Tooltip("A stat modifier for this variant, uses RecalculateStatsAPI internally. This may be null")]
        public IVariantStatModifier? statModifier = null;

        [Tooltip("A list of buffs for this variant")]
        public VariantBuffStorage variantBuffs = new VariantBuffStorage();

        [Tooltip("The visuals for this variant. This may be null")]
        public CharacterVariantVisualModifier? visualModifier = null;

        [Tooltip("A scale multiplier for this variant. Cannot be less or equal to 0")]
        [Min(0 + float.Epsilon)]
        public float scaleMultiplier = 1f;

        [Header("Other")]
        [Tooltip("A collection of components for this variant")]
        public VariantComponentCollection additionalVariantComponents = new VariantComponentCollection();

        /// <summary>
        /// The config file used to configure this variant. Set by the VariantPackManager. This can be null.
        /// <br></br>
        /// If it exists, Configurations for this variant's <see cref="spawnRate"/> and <see cref="isUnique"/> will be created
        /// </summary>
        public ConfigFile? associatedConfigFile { get; internal set; }

        /// <summary>
        /// The mod that added this Variant. Set by the VariantPackManager. This is only null if the VariantPackManager has not initialized.
        /// </summary>
        public BaseUnityPlugin? ownerPlugin { get; internal set; }

        /// <summary>
        /// True if at least one <see cref="CharacterVariantProvider"/> has this variant in it's internal collection.
        /// <br></br>
        /// This is rarely false, a situation where this is false is where a variant is added to a <see cref="VariantPack"/>, but the <see cref="targetBody"/> resolves to a nonexistent Body Prefab. This is most commonly seen on Modded Variants.
        /// </summary>
        public bool hasProvider { get; internal set; }

        protected override void OnValidate()
        {
            base.OnValidate();
            spawnCondition?.Validate();
            variantNameProvider?.Validate();
            statModifier?.Validate();
            foreach(var masterModifier in masterModifiers)
            {
                masterModifier?.Validate();
            }
            foreach(var bodyModifier in bodyModifiers)
            {
                bodyModifier?.Validate();
            }
        }

        /// <summary>
        /// Returns true if this variant is available under the current run context.
        /// </summary>
        public bool IsAvailable()
        {
            if (spawnRate == 0)
                return false;

            if (!hasProvider)
                return false;

            return VAPIUtils.IsVariantAvailable(this);
        }

        /// <summary>
        /// Creates a clone of <paramref name="other"/>, assigns its name to <paramref name="newName"/> and returns the clone.
        /// </summary>
        public static CharacterVariantDef CreateInstance(CharacterVariantDef other, string newName)
        {
            CharacterVariantDef variantDef = CreateInstance<CharacterVariantDef>();
            variantDef.cachedName = newName;
            variantDef.targetBody = (CharacterVariantTarget)other.targetBody.Clone();
            variantDef.variantTier = other.variantTier;
            variantDef.isUnique = other.isUnique;
            variantDef.isHidden = other.isHidden;
            variantDef.spawnRate = other.spawnRate;
            variantDef.arrivalToken = other.arrivalToken;

            if(other.spawnCondition != null)
            {
                variantDef.spawnCondition = (IVariantSpawnCondition?)other.spawnCondition.Clone();
            }

            List<IVariantMasterModifier> masterModifiers = new List<IVariantMasterModifier>();
            foreach(var otherMasterModifier in other.masterModifiers)
            {
                if(otherMasterModifier != null)
                {
                    masterModifiers.Add((IVariantMasterModifier)otherMasterModifier.Clone());
                }
            }
            variantDef.masterModifiers = masterModifiers.ToArray();
            variantDef.inventoryDefinition = (VariantInventoryDefinition)other.inventoryDefinition.Clone();

            List<IVariantBodyModifier> bodyModifiers = new List<IVariantBodyModifier>();
            foreach(var otherBodyModifier in other.bodyModifiers)
            {
                if(otherBodyModifier != null)
                {
                    bodyModifiers.Add((IVariantBodyModifier)otherBodyModifier.Clone());
                }
            }
            variantDef.bodyModifiers = bodyModifiers.ToArray();
            if(other.variantNameProvider != null)
            {
                variantDef.variantNameProvider = (IVariantNameProvider?)other.variantNameProvider.Clone();
            }
            variantDef.deathStateOverride = (VariantDeathStateOverride)other.deathStateOverride.Clone();
            List<VariantSkillReplacement> skillReplacements = new List<VariantSkillReplacement>();
            foreach(var otherSkillReplacement in other.skillReplacements)
            {
                if(otherSkillReplacement != null)
                {
                    skillReplacements.Add((VariantSkillReplacement)otherSkillReplacement.Clone());
                }
            }
            variantDef.skillReplacements = skillReplacements.ToArray();
            if(other.statModifier != null)
            {
                variantDef.statModifier = (IVariantStatModifier?)other.statModifier.Clone();
            }
            variantDef.variantBuffs = (VariantBuffStorage)other.variantBuffs.Clone();
            if(other.visualModifier != null)
            {
                variantDef.visualModifier = other.visualModifier;
            }
            variantDef.scaleMultiplier = other.scaleMultiplier;

            variantDef.additionalVariantComponents = (VariantComponentCollection)other.additionalVariantComponents.Clone();
            return variantDef;
        }
    }
}