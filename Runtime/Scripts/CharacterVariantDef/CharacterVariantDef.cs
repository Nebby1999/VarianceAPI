#if UNITY_EDITOR
using UnityEditor;
#endif
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
#nullable enable

namespace VAPI
{
    [CreateAssetMenu(fileName = "New CharacterVariantDef", menuName = "VarianceAPI/CharacterVariantDef")]
    public sealed class CharacterVariantDef : CachedNameScriptableObject
    {
        public CharacterVariantIndex characterVariantIndex { get; internal set; }
        [Header("General Settings")]
        public VariantCharacterTarget targetCharacter = new VariantCharacterTarget();
        public CharacterVariantTierDef? variantTier = null;
        public bool isUnique;
        [Range(0, 100)]
        public float spawnRate;
        public string arrivalToken = "";

        [Header("Spawn Conditions")]
        [SerializeReference, SubclassSelector]
        public IVariantSpawnCondition? spawnCondition = null;

        [Header("Master Related")]
        [SerializeReference, SubclassSelector]
        public IVariantMasterModifier[] masterModifiers = Array.Empty<IVariantMasterModifier>();
        public VariantInventoryDefinition inventoryDefinition = new VariantInventoryDefinition();

        [Header("Body Related")]
        [SerializeReference, SubclassSelector]
        public IVariantNameProvider? variantNameProvider = null;
        public VariantDeathStateOverride deathStateOverride = new VariantDeathStateOverride();
        public VariantSkillReplacement[] skillReplacements = Array.Empty<VariantSkillReplacement>();
        [SerializeReference, SubclassSelector]
        public IVariantStatModifier? statModifier = null;
        public VariantBuffStorage variantBuffs = new VariantBuffStorage();
        public CharacterVariantVisualModifier? visualModifier = null;
        [Min(0 + float.Epsilon)]
        public float scaleMultiplier = 1f;

        [Header("Other")]
        public VariantComponentCollection additionalVariantComponents = new VariantComponentCollection();

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

        public bool IsAvailable()
        {
            if (spawnRate == 0)
                return false;

            //TODO: Check for rulebook

            return spawnCondition?.IsAvailable() ?? true;
        }

        public static CharacterVariantDef CreateInstance(CharacterVariantDef other, string newName)
        {
            CharacterVariantDef variantDef = CreateInstance<CharacterVariantDef>();
            variantDef.cachedName = newName;
            variantDef.targetCharacter = (VariantCharacterTarget)other.targetCharacter.Clone();
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