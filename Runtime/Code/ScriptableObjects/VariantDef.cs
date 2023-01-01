﻿using UnityEngine;
using R2API;
using System.Collections.Generic;
using System.Linq;
using VAPI.Components;
using Moonstorm.AddressableAssets;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using RoR2.Skills;
using HG;
using EntityStates;
using VAPI.RuleSystem;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantDef", menuName = "VarianceAPI/VariantDef")]
    public class VariantDef : ScriptableObject
    {
        public VariantIndex VariantIndex { get; internal set; }

        public string bodyName;

        [SerializeField] internal VariantTierIndex variantTier;
        [SerializeField] internal VariantTierDef variantTierDef;

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

        public VariantOverrideName[] nameOverrides = Array.Empty<VariantOverrideName>();

        public VariantComponentProvider[] componentProviders = Array.Empty<VariantComponentProvider>();

        public VariantTierIndex VariantTierIndex
        {
            get
            {
                if(variantTierDef)
                {
                    return variantTierDef.Tier;
                }
                return variantTier;
            }
            set
            {
                variantTierDef = VariantTierCatalog.GetVariantTierDef(value);
            }
        }

        public VariantTierDef VariantTierDef
        {
            get
            {
                if (!variantTierDef)
                    variantTierDef = VariantTierCatalog.GetVariantTierDef(VariantTierIndex);
                return variantTierDef;
            }
            set
            {
                variantTierDef = value;
                variantTier = variantTierDef.Tier;
            }
        }

        private void OnValidate()
        {
            if(variantTier == VariantTierIndex.AssignedAtRuntime && !variantTierDef)
            {
                Debug.LogError($"{this} has a variantTier set to AssignedAtRuntime, but no variantTierDef asset asigned!");
            }

            if(variantTierDef)
            {
                variantTier = VariantTierIndex.AssignedAtRuntime;
            }
        }


        public bool IsAvailable()
        {
            var runInstance = Run.instance;
            var sceneInfo = SceneInfo.instance;

            return (runInstance && sceneInfo) && IsAvailable(runInstance, sceneInfo, runInstance.ruleBook);
        }

        public bool IsAvailable(Run run, SceneInfo sceneInfo) => IsAvailable(run, sceneInfo, run.ruleBook);

        public bool IsAvailable(Run run, SceneInfo sceneInfo, RuleBook runRulebook)
        {
            ExpansionDef[] runExpansions = ExpansionCatalog.expansionDefs.Where(exp => run.IsExpansionEnabled(exp)).ToArray();
            DirectorAPI.StageInfo stageInfo = DirectorAPI.StageInfo.ParseInternalStageName(sceneInfo.sceneDef.baseSceneName);

            return IsAvailable(stageInfo, runExpansions, runRulebook);
        }

        public bool IsAvailable(DirectorAPI.StageInfo stageInfo, ExpansionDef[] runExpansions, RuleBook runRulebook)
        {
            bool variantRuleEnabled = RuleBookExtras.CanVariantSpawn(runRulebook, VariantIndex);

            if(!variantRuleEnabled)
            {
                return false;
            }

            return variantSpawnCondition && variantSpawnCondition.IsAvailable(stageInfo, runExpansions);
        }

        [Serializable]
        public class VariantOverrideName
        {
            [Tooltip("The token used for this override name" +
                "\nTo the right is a field to determine how the override is applied:" +
                "\nPrefix: Applied before the Variant's original name" +
                "\nSuffix: Applied after the Variant's original name" +
                "\nOverride: Completely overrides the Variant's original name")]
            public string token;
            public OverrideNameType overrideType;
        }

        [Serializable]
        public class VariantSkillReplacement
        {
            [Tooltip("The SkillDef for this Variant Replacement" +
                "\nTo the right is a field where you can specify what SkillSlot this replacement targets.")]
            public SkillDef skillDef;
            public SkillSlot skillSlot;
        }
        [Serializable]
        public class VariantComponentProvider
        {
            [Tooltip("A component that inherits from VariantComponent to add to this variant." +
                "\nYou'll probably want to use RoR2EditorKit for filling this." +
                "\nTo the right is a field to determine Where the component is added." +
                "\nBody: Added to same game object where the CharacterBody component is located." +
                "\nModel: Added to the same game object where the CharacterModel component is located." +
                "\nMaster: Added to the same game object where the CharacterMaster component is located.")]
            [SerializableSystemType.RequiredBaseType(typeof(VariantComponent))]
            public SerializableSystemType componentToAdd;
            public ComponentAttachmentType attachmentType;
        }
    }
}
