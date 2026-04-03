
using R2API;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A ScriptableObject used to represent unique SpawnConditions for a variant
    /// </summary>
    public class VariantSpawnCondition : ScriptableObject
    {
        [Tooltip("The amount of stages that need to be completed before the variant can spawn")]
        public int minimumStageCompletions;
        public DirectorAPI.Stage stages
        {
            get
            {
                return _stages;
            }
            set
            {
                _stages = value;
            }
        }
        [Tooltip("A flag of vanilla stages where this variant can spawn, Add the custom flag if you want this variant to spawn on custom stages")]
        [SerializeField] private DirectorAPI.StageSerde _stages;
        [Tooltip("A list of custom stageDef baseSceneNames where this variant can spawn")]
        public List<string> customStages = new List<string>();

        [Space]

        [Tooltip("This unlockable must be unlocked for this variant to spawn")]
        public AddressReferencedUnlockableDef requiredUnlock;

        [Tooltip("This unlockable CANNOT be unlocked for this variant to spawn")]
        public AddressReferencedUnlockableDef forbiddenUnlock;

        [Tooltip("These expansions must be enabled for this variant to spawn")]
        public List<AddressReferencedExpansionDef> requiredExpansionDefs = new List<AddressReferencedExpansionDef>();

        private void OnValidate()
        {
            customStages = customStages.Select(stage => stage.ToLowerInvariant()).ToList();
        }

        /// <summary>
        /// Awake method for VariantSpawnCondition
        /// <para>Ensures the now deprecated <see cref="requiredExpansions"/>, <see cref="forbiddenUnlockableDef"/>, and <see cref="requiredUnlockableDef"/> are migrated to their new fields</para>
        /// </summary>
        protected virtual void Awake()
        {
        }

        /// <summary>
        /// Determines wether a Variant that has this spawn condition can spawn or not
        /// </summary>
        /// <param name="stageInfo">The current stage's StageInfo</param>
        /// <param name="runExpansions">The Run's enabled expansions</param>
        /// <returns>True if the variant can spawn, false otherwise</returns>
        public virtual bool IsAvailable(DirectorAPI.StageInfo stageInfo, ExpansionDef[] runExpansions)
        {
            if (!Run.instance)
                return false;
            bool allowedInThisRun = AreRunRequirementsMet(runExpansions);
            bool allowedInStage = AllowedInStage(stageInfo);

            return allowedInThisRun && allowedInStage;
        }

        /// <summary>
        /// Determines wether the requirements for the Run are met
        /// </summary>
        /// <param name="runExpansions">The run's ExpansionDefs</param>
        /// <returns>True if the run allows the spawning of this variant, false otherwise</returns>
        public virtual bool AreRunRequirementsMet(ExpansionDef[] runExpansions)
        {
            if (!Run.instance)
                return false;

            bool flag0 = !requiredUnlock || Run.instance.IsUnlockableUnlocked(requiredUnlock);
            bool flag1 = forbiddenUnlock && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlock);

            if (Run.instance.stageClearCount >= minimumStageCompletions && flag0 && !flag1)
            {
                List<ExpansionDef> expansions = requiredExpansionDefs.Where(exp => exp.AssetExists)
                    .Select(exp => exp.Asset)
                    .ToList();

                bool metExpansionRequirements = false;
                foreach (ExpansionDef expansion in expansions)
                {
                    metExpansionRequirements = runExpansions.Contains(expansion);
                }
                return metExpansionRequirements;
            }
            return false;
        }

        /// <summary>
        /// Wether this SpawnCondition can be used in the stage specified in <paramref name="stageInfo"/>
        /// </summary>
        /// <param name="stageInfo">The Stage's StageInfo</param>
        /// <returns>True if allowed, false otherwise.</returns>
        public virtual bool AllowedInStage(DirectorAPI.StageInfo stageInfo)
        {
            if (stageInfo.stage == DirectorAPI.Stage.Custom)
            {
                return customStages.Count > 0 ? customStages.Contains(stageInfo.CustomStageName.ToLowerInvariant()) : true;
            }
            return stages.HasFlag(stageInfo.stage);
        }
    }
}
