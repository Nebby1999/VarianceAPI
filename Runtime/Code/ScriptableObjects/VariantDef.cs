using EntityStates;
using HG;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using System;
using System.Linq;
using UnityEngine;
using VAPI.Components;
using VAPI.RuleSystem;

namespace VAPI
{
    /// <summary>
    /// A ScriptableObject that represents a new Variant
    /// </summary>
    [CreateAssetMenu(fileName = "New VariantDef", menuName = "VarianceAPI/VariantDef")]
    public class VariantDef : ScriptableObject
    {
        /// <summary>
        /// The variant's VariantIndex, do not set this value yourself
        /// </summary>
        public VariantIndex VariantIndex { get; internal set; }

        [Tooltip("The BodyPrefab's name for this variant, taken directly from the BodyCatalog")]
        public string bodyName;

        [Tooltip("The variant's Tier, set this to the existing enums, or to AssignedAtRuntime to specify a custom VariantTier")]
        [SerializeField] internal VariantTierIndex variantTier;
        [Tooltip("The Variant's TierDef, only applicable if variantTier is set to AssignedAtRuntime ")]
        [SerializeField] internal VariantTierDef variantTierDef;

        [Tooltip("Wether this variant can merge with other variants of the same body")]
        public bool isUnique = false;

        [Tooltip("The chance for this variant to spawn")]
        [Range(0, 100)]
        public float spawnRate;

        [Tooltip("A set of conditions that need to be met for this variant to spawn")]
        public VariantSpawnCondition variantSpawnCondition;

        [Tooltip("A token that will be displayed when the variant spawns, only applicable if the Tier announces arrivals")]
        public string arrivalToken;

        [Tooltip("The Variant's Inventory")]
        public VariantInventory variantInventory;

        [Tooltip("An override for the variant's death state")]
        public SerializableEntityStateType deathStateOverride;

        [Tooltip("A set of skill replacements for this variant")]
        public VariantSkillReplacement[] skillReplacements = Array.Empty<VariantSkillReplacement>();

        [Tooltip("A health multiplier applied to this variant")]
        [Min(0)]
        public float healthMultiplier = 1;

        [Tooltip("Bonus regeneration that's given to the variant")]
        public float regenBonus = 0;

        [Tooltip("Regeneration multiplier that's given to this variant, applied after regenBonus")]
        [Min(0)]
        public float regenMultiplier = 0;

        [Tooltip("Bonus shield that's given to the variant")]
        public float shieldBonus = 0;

        [Tooltip("Shield multiplier that's given to this variant, applied after shieldBonus")]
        [Min(0)]
        public float shieldMultiplier = 1;

        [Tooltip("Movement Speed multiplier that's given to the variant")]
        [Min(0)]
        public float moveSpeedMultiplier = 1;

        [Tooltip("Attack Speed multiplier that's given to the variant")]
        [Min(0)]
        public float attackSpeedMultiplier = 1;

        [Tooltip("Damage multiplier that's given to the variant")]
        [Min(0)]
        public float damageMultiplier = 1;

        [Tooltip("Bonus armor that's given to the variant")]
        public float armorBonus = 0;

        [Tooltip("Armor multiplier that's given to this variant, applied after armorBonus")]
        [Min(0)]
        public float armorMultiplier = 1;

        [Tooltip("The Variant's visual modifiers")]
        public VariantVisuals visualModifier;

        [Tooltip("The variant's Size modifier")]
        public VariantSizeModifier sizeModifier;

        [Tooltip("An AI modifier that's applied to this variant's master" +
            "\nDefault: No changes are made" +
            "\nUnstable: The variant can use it's Desperation Attacks always." +
            "\nForceSprint: The variant always sprints")]
        public BasicAIModifier aiModifier;

        [Tooltip("A set of name overrides that's applied to this variant")]
        public VariantOverrideName[] nameOverrides = Array.Empty<VariantOverrideName>();

        [Tooltip("A set of components that're added to this variant")]
        public VariantComponentProvider[] componentProviders = Array.Empty<VariantComponentProvider>();

        /// <summary>
        /// Represents the Variant's TierIndex
        /// </summary>
        public VariantTierIndex VariantTierIndex
        {
            get
            {
                if (variantTierDef)
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

        /// <summary>
        /// The Variant's TierDef
        /// </summary>
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
            if (variantTier == VariantTierIndex.AssignedAtRuntime && !variantTierDef)
            {
                Debug.LogError($"{this} has a variantTier set to AssignedAtRuntime, but no variantTierDef asset asigned!");
            }

            if (variantTierDef)
            {
                variantTier = VariantTierIndex.AssignedAtRuntime;
            }
        }

        /// <summary>
        /// Wether or not this variant is available or not, A Run and a SceneInfo must exist
        /// </summary>
        /// <returns>True if available, false otherwise</returns>
        public bool IsAvailable()
        {
            var runInstance = Run.instance;
            var sceneInfo = SceneInfo.instance;

            return (runInstance && sceneInfo) && IsAvailable(runInstance, sceneInfo, runInstance.ruleBook);
        }

        /// <summary>
        /// Wether or not this variant is available or not.
        /// </summary>
        /// <param name="run">The current run</param>
        /// <param name="sceneInfo">The current run's Scene</param>
        /// <returns>True if available, false otherwise</returns>
        public bool IsAvailable(Run run, SceneInfo sceneInfo) => IsAvailable(run, sceneInfo, run.ruleBook);

        /// <summary>
        /// Wether or not this variant is available or not
        /// </summary>
        /// <param name="run">The current run</param>
        /// <param name="sceneInfo">The current run's scene</param>
        /// <param name="runRulebook">The current run's RuleBook</param>
        /// <returns>True if available, false otherwise</returns>
        public bool IsAvailable(Run run, SceneInfo sceneInfo, RuleBook runRulebook)
        {
            ExpansionDef[] runExpansions = ExpansionCatalog.expansionDefs.Where(exp => run.IsExpansionEnabled(exp)).ToArray();
            DirectorAPI.StageInfo stageInfo = DirectorAPI.StageInfo.ParseInternalStageName(sceneInfo.sceneDef.baseSceneName);

            return IsAvailable(stageInfo, runExpansions, runRulebook);
        }

        /// <summary>
        /// Wether or not this variant is available or not
        /// </summary>
        /// <param name="stageInfo">The current StageInfo</param>
        /// <param name="runExpansions">The run's ExpansionDefs</param>
        /// <param name="runRulebook">The run's RuleBook</param>
        /// <returns>True if available, false otherwise</returns>
        public bool IsAvailable(DirectorAPI.StageInfo stageInfo, ExpansionDef[] runExpansions, RuleBook runRulebook)
        {
            if(spawnRate == 0)
            {
                return false;
            }
            bool variantRulesEnabled = RuleBookExtras.CanVariantSpawn(runRulebook, VariantIndex);

            if (!variantRulesEnabled)
            {
                return false;
            }

            if(variantSpawnCondition)
            {
                return variantSpawnCondition.IsAvailable(stageInfo, runExpansions);
            }
            return true;
        }

        /// <summary>
        /// Represents a name override for a variant
        /// </summary>
        [Serializable]
        public class VariantOverrideName
        {
            [Tooltip("The token used for this override name" +
                "\nTo the right is a field to determine how the override is applied:" +
                "\nPrefix: Applied before the Variant's original name" +
                "\nSuffix: Applied after the Variant's original name" +
                "\nOverride: Completely overrides the Variant's original name")]
            public string token;
            /// <summary>
            /// The type of name override
            /// </summary>
            public OverrideNameType overrideType;
        }

        /// <summary>
        /// Represents a Skill replacement for a variant
        /// </summary>
        [Serializable]
        public class VariantSkillReplacement
        {
            [Tooltip("The SkillDef for this Variant Replacement" +
                "\nTo the right is a field where you can specify what SkillSlot this replacement targets.")]
            public SkillDef skillDef;
            /// <summary>
            /// The skill slot for this replacement
            /// </summary>
            public SkillSlot skillSlot;
        }
        /// <summary>
        /// Represents a Component that will be addeed to this variant
        /// </summary>
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
            /// <summary>
            /// how the component will be attached
            /// </summary>
            public ComponentAttachmentType attachmentType;
        }
    }
}
