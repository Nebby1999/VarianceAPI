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
    /// ScriptableObject class for CharacterVariants.
    /// <br></br>
    /// New ones can be created in code via the <see cref="CharacterVariantDefBuilder"/>, or by "Cloning" one using <see cref="CharacterVariantDef.CreateInstance(CharacterVariantDef, string)"/>
    /// </summary>
    [CreateAssetMenu(fileName = "New CharacterVariantDef", menuName = "VarianceAPI/CharacterVariantDef")]
    public sealed class CharacterVariantDef : CachedNameScriptableObject
    {
        /// <summary>
        /// The <see cref="CharacterVariantIndex"/> assigned to this variant def
        /// </summary>
        public CharacterVariantIndex characterVariantIndex { get; internal set; }

        [Header("General Settings")]
        [Tooltip("A Target body for this variant. This variant will be available to this body.")]
        public CharacterVariantTarget targetBody = new CharacterVariantTarget();
        [Tooltip("The tier for this variant, can be left null")]
        public CharacterVariantTierDef? variantTier = null;

        [Tooltip("Wether this variant is unique or not. Unique variants cannot merge with other variants")]
        public bool isUnique;

        [Tooltip("The spawn rate for this variant")]
        [Range(0, 100)]
        public float spawnRate;

        [Tooltip("An arrival token for this variant when it spawns, only used if it's tier announces arrivals")]
        public string arrivalToken = "";

        [Header("Spawn Conditions")]
        [SerializeReference, SubclassSelector]
        [Tooltip("A spawn condition for this variant.")]
        public IVariantSpawnCondition? spawnCondition = null;

        [Header("Master Related")]
        [SerializeReference, SubclassSelector]
        [Tooltip("An array of master modifiers for this variant")]
        public IVariantMasterModifier[] masterModifiers = Array.Empty<IVariantMasterModifier>();

        [Tooltip("An inventory definition for this variant, holding information like items and equipments")]
        public VariantInventoryDefinition inventoryDefinition = new VariantInventoryDefinition();

        [Header("Body Related")]
        [SerializeReference, SubclassSelector]
        [Tooltip("A name provider for this variant, used to change the display name")]
        public IVariantNameProvider? variantNameProvider = null;

        [Tooltip("A death state override for this variant")]
        public VariantDeathStateOverride deathStateOverride = new VariantDeathStateOverride();

        [Tooltip("An array of skill replacements for this variant")]
        public VariantSkillReplacement[] skillReplacements = Array.Empty<VariantSkillReplacement>();

        [SerializeReference, SubclassSelector]
        [Tooltip("A stat modifier for this variant, uses RecalculateStatsAPI internally")]
        public IVariantStatModifier? statModifier = null;

        [Tooltip("A list of buffs for this variant")]
        public VariantBuffStorage variantBuffs = new VariantBuffStorage();

        [Tooltip("The visuals for this variant")]
        public CharacterVariantVisualModifier? visualModifier = null;

        [Tooltip("A scale multiplier for this variant")]
        [Min(0 + float.Epsilon)]
        public float scaleMultiplier = 1f;

        [Header("Other")]
        [Tooltip("A collection of components for this variant")]
        public VariantComponentCollection additionalVariantComponents = new VariantComponentCollection();

        /// <summary>
        /// The config file used to configure this variant. Set by the VariantPackManager
        /// </summary>
        public ConfigFile? associatedConfigFile { get; internal set; }


        /// <summary>
        /// The mod that added this Variant. Set by the VariantPackManager.
        /// </summary>
        public BepInPlugin? ownerPlugin { get; internal set; }

        /// <summary>
        /// True if this <see cref="CharacterVariantDef"/> has at least one <see cref="CharacterVariantProvider"/> in the <see cref="CharacterVariantCatalog"/>.
        /// <br></br>
        /// This is usually false when the variant is added to a VariantPack, but the <see cref="targetCharacter"/> resolves to an invalid CharacterBody or CharacterMaster component.
        /// </summary>
        public bool hasProvider { get; internal set; }

        protected override void OnValidate()
        {
            spawnCondition?.Validate();
            variantNameProvider?.Validate();
            statModifier?.Validate();
            foreach(var masterModifier in masterModifiers)
            {
                masterModifier?.Validate();
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

            //Unavailable if no run or if the rule is not enabled.
            if (!Run.instance || !VAPIRuleBook.IsVariantRuleEnabled(this, Run.instance.ruleBook))
                return false;

            return spawnCondition?.IsAvailable() ?? true;
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
            variantDef.scaleMultiplier = other.scaleMultiplier;

            variantDef.additionalVariantComponents = (VariantComponentCollection)other.additionalVariantComponents.Clone();
            return variantDef;
        }
    }
}