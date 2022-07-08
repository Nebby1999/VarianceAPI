using UnityEngine;
using R2API;
using System.Collections.Generic;
using Moonstorm.AddressableAssets;
using RoR2.ExpansionManagement;
using RoR2;
using System.Linq;

namespace VAPI
{
    [CreateAssetMenu(fileName = "new VariantSpawnCondition", menuName = "VarianceAPI/VariantSpawnCondition")]
    public class VariantSpawnCondition : ScriptableObject
    {
        public int minimumStageCompletions;
        public DirectorAPI.Stage stages;
        public List<string> customStages;

        [Space]
        public AddressableUnlockableDef requiredUnlockableDef;
        public AddressableUnlockableDef forbiddenUnlockableDef;
        public List<AddressableExpansionDef> requiredExpansions;

        public virtual bool IsAvailable(DirectorAPI.StageInfo stageInfo, ExpansionDef[] runExpansions)
        {
            if (!Run.instance)
                return false;
            bool allowedInThisRun = AreRunRequirementsMet(runExpansions);
            bool allowedInStage = AllowedInStage(stageInfo);

            return allowedInThisRun && allowedInStage;
        }

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
                foreach(ExpansionDef expansion in runExpansions)
                {
                    metExpansionRequirements = expansions.Contains(expansion);
                }
                return metExpansionRequirements;
            }
            return false;
        }

        public virtual bool AllowedInStage(DirectorAPI.StageInfo stageInfo)
        {
            if(stageInfo.stage == DirectorAPI.Stage.Custom)
            {
                return customStages.Count > 0 ? customStages.Contains(stageInfo.CustomStageName.ToLowerInvariant()) : true;
            }
            return stages.HasFlag(stageInfo.stage);
        }
    }
}
