using UnityEngine;
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

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantDef", menuName = "VarianceAPI/VariantDef")]
    public class VariantDef : ScriptableObject
    {
        public VariantIndex VariantIndex { get; internal set; }

        public string bodyName;

        public VariantOverrideName[] nameOverrides = Array.Empty<VariantOverrideName>();

        public bool isUnique = false;

        public BasicAIModifier aiModifier;

        public VariantTierIndex variantTier;
        public VariantTierDef variantTierDef;

        [Range(0, 100)]
        public float spawnRate;

        [Tooltip("The conditions specified here need to be met for this variant to spawn, leave this blank if you want the variant to not have spawning restrictions")]
        public VariantSpawnCondition variantSpawnCondition;

        public VariantSkillReplacement[] skillReplacements = Array.Empty<VariantSkillReplacement>();

        public VariantInventory variantInventory;

        [Tooltip("Multiplier applied to the variant's Health\nWhere 1.0 = 100% Base health")]
        [Min(0)]
        public float healthMultiplier = 1;

        [Tooltip("Extra regen of the Variant")]
        public float regenBonus = 0;

        [Tooltip("Multiplier applied to the variant's Regen\nApplied after regenBonus\nWhere 1.0 = 100% Base Regen")]
        [Min(0)]
        public float regenMultiplier = 0;

        [Tooltip("Extra Shield of the Variant")]
        public float shieldBonus = 0;

        [Tooltip("Multiplier applied to the variant's Shield\nApplied after shieldBonus\nWhere 1.0 = 100% Base Shield")]
        [Min(0)]
        public float shieldMultiplier = 1;

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

        public VariantVisuals visualModifier;

        public float sizeModifier;

        public string arrivalToken;

        public VariantComponentProvider[] componentProviders = Array.Empty<VariantComponentProvider>();

        public SerializableEntityStateType deathStateOverride;

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
            if (!variantSpawnCondition)
                return true;

            var run = Run.instance;
            var sceneInfo = SceneInfo.instance;

            return (run && sceneInfo) ? IsAvailable(run, sceneInfo) : false;
        }

        public bool IsAvailable(Run run, SceneInfo sceneInfo)
        {
            ExpansionDef[] runExpansions = ExpansionCatalog.expansionDefs.Where(exp => run.IsExpansionEnabled(exp)).ToArray();
            DirectorAPI.StageInfo stageInfo = DirectorAPI.StageInfo.ParseInternalStageName(sceneInfo.sceneDef.baseSceneName);

            return variantSpawnCondition ? IsAvailable(stageInfo, runExpansions) : false;
        }

        public bool IsAvailable(DirectorAPI.StageInfo stageInfo, ExpansionDef[] runExpansions)
        {
            return variantSpawnCondition ? variantSpawnCondition.IsAvailable(stageInfo, runExpansions) : false;
        }

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
            public ComponentAttachmentType attachmentType;
        }
    }
}
