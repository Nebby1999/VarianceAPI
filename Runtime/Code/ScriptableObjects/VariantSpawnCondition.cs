using Moonstorm.AddressableAssets;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A ScriptableObject used to represent unique SpawnConditions for a variant
    /// </summary>
    [CreateAssetMenu(fileName = "New VariantSpawnCondition", menuName = "VarianceAPI/VariantSpawnCondition")]
    public class VariantSpawnCondition : ScriptableObject
    {
        [Tooltip("The amount of stages that need to be completed before the variant can spawn")]
        public int minimumStageCompletions;
        [Tooltip("A flag of vanilla stages where this variant can spawn, Add the custom flag if you want this variant to spawn on custom stages")]
        public DirectorAPI.Stage stages;
        [Tooltip("A list of custom stageDef baseSceneNames where this variant can spawn")]
        public List<string> customStages = new List<string>();

        [Space]
        [Tooltip("This unlockable must be unlocked for this variant to spawn")]
        public AddressableUnlockableDef requiredUnlockableDef;
        [Tooltip("This unlockable CANNOT be unlocked for this variant to spawn")]
        public AddressableUnlockableDef forbiddenUnlockableDef;
        [Tooltip("These expansions must be enabled for this variant to spawn")]
        public List<AddressableExpansionDef> requiredExpansions = new List<AddressableExpansionDef>();

        private void OnValidate()
        {
            customStages = customStages.Select(stage => stage.ToLowerInvariant()).ToList();
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

            bool flag0 = !requiredUnlockableDef.Asset || Run.instance.IsUnlockableUnlocked(requiredUnlockableDef.Asset);
            bool flag1 = forbiddenUnlockableDef.Asset && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlockableDef.Asset);

            if (Run.instance.stageClearCount >= minimumStageCompletions && flag0 && !flag1)
            {
                List<ExpansionDef> expansions = requiredExpansions.Where(exp => exp.Asset)
                    .Select(exp => exp.Asset)
                    .ToList();

                bool metExpansionRequirements = false;
                foreach (ExpansionDef expansion in runExpansions)
                {
                    metExpansionRequirements = expansions.Contains(expansion);
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
