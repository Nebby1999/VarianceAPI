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
                if (variantTierDef)
                    return variantTierDef;
                return VariantTierCatalog.GetVariantTierDef(VariantTierIndex);
            }
            set
            {
                variantTierDef = value;
                variantTier = VariantTierIndex.AssignedAtRuntime;
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

            return variantSpawnCondition ? IsAvailable(stageInfo, runExpansions) : true;
        }

        public bool IsAvailable(DirectorAPI.StageInfo stageInfo, ExpansionDef[] runExpansions)
        {
            return variantSpawnCondition ? variantSpawnCondition.IsAvailable(stageInfo, runExpansions) : true;
        }

        [Serializable]
        public class VariantOverrideName
        {
            [Tooltip("The token used for this override name")]
            public string token;
            [Tooltip("How the override is applied:" +
                "\nPrefix: Applied before the Variant's original name" +
                "\nSuffix: Applied after the Variant's original name" +
                "\nOverride: Completely overrides the Variant's original name")]
            public OverrideNameType overrideType;
        }

        [Serializable]
        public class VariantSkillReplacement
        {
            [Tooltip("The SkillDef for this Variant Replacement")]
            public SkillDef skillDef;
            [Tooltip("The slot that this skill replacement targets")]
            public SkillSlot skillSlot;
        }
        [Serializable]
        public class VariantComponentProvider
        {
            [Tooltip("A component that inherits from VariantComponent to add to this variant." +
                "\nYou'll probably want to use RoR2EditorKit for filling this.")]
            [SerializableSystemType.RequiredBaseType(typeof(VariantComponent))]
            public SerializableSystemType componentToAdd;
            [Tooltip("Where the component is added." +
                "\nBody: Added to same game object where the CharacterBody component is located." +
                "\nModel: Added to the same game object where the CharacterModel component is located." +
                "\nMaster: Added to the same game object where the CharacterMaster component is located.")]
            public ComponentAttachmentType attachmentType;
        }
    }
}
