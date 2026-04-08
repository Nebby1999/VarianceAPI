#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using HG;
using MSU;
using R2API;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace VAPI
{
    public interface IVariantSpawnCondition : IValidatable
    {
        public bool IsAvailable();
    }

    [Serializable]
    public class BasicSpawnCondition : IVariantSpawnCondition
    {
        public BasicSpawnCondition(int? _minimumStageCompletions, DirectorAPI.Stage? _stages, string[]? _customStages, Either<AddressReferencedUnlockableDef, UnlockableDef>? _requiredUnlock, Either<AddressReferencedUnlockableDef, UnlockableDef>? _forbiddenUnlock, Either<AddressReferencedExpansionDef[], ExpansionDef[]>? _requiredExpansions)
        {
            if(_minimumStageCompletions.HasValue)
            {
                minimumStageCompletions = _minimumStageCompletions.Value;
            }
            if(_stages.HasValue)
            {
                stages = _stages.Value;
            }
            if(_customStages != null)
            {
                customStages = _customStages;
            }
            if(_requiredUnlock.HasValue)
            {
                requiredUnlock = _requiredUnlock.Value.a ?? _requiredUnlock.Value.b;
            }
            if(_forbiddenUnlock.HasValue)
            {
                forbiddenUnlock = _forbiddenUnlock.Value.a ?? _forbiddenUnlock.Value.b;
            }
            if(_requiredExpansions.HasValue)
            {
                if(_requiredExpansions.Value.isA)
                {
                    requiredExpansionDefs = _requiredExpansions.Value.a;
                }
                else if(_requiredExpansions.Value.isB)
                {
                    requiredExpansionDefs = new AddressReferencedExpansionDef[_requiredExpansions.Value.b.Length];
                    for(int i = 0; i < requiredExpansionDefs.Length; i++)
                    {
                        requiredExpansionDefs[i] = _requiredExpansions.Value.b[i];
                    }
                }
            }
        }
        public BasicSpawnCondition() { }
        public int minimumStageCompletions;
        public DirectorAPI.Stage stages { get => _stages; set => _stages = value; }
        [SerializeField] private DirectorAPI.StageSerde _stages;
        public string[] customStages = Array.Empty<string>();

        [Space]

        [Tooltip("This unlockable must be unlocked for this variant to spawn")]
        public AddressReferencedUnlockableDef requiredUnlock = new AddressReferencedUnlockableDef();

        [Tooltip("This unlockable CANNOT be unlocked for this variant to spawn")]
        public AddressReferencedUnlockableDef forbiddenUnlock = new AddressReferencedUnlockableDef();

        [Tooltip("These expansions must be enabled for this variant to spawn")]
        public AddressReferencedExpansionDef[] requiredExpansionDefs = Array.Empty<AddressReferencedExpansionDef>();

        public virtual bool IsAvailable()
        {
            bool expansionRequirementMet = AreExpansionRequirementsMet();
            bool unlockableRequirementMet = AreUnlockableRequirementsMet();
            bool allowedInStage = AreStageRequirementsMet();

            return expansionRequirementMet && unlockableRequirementMet && allowedInStage;
        }

        public virtual void Validate()
        {
            for(int i = 0; i < customStages.Length; i++)
            {
                customStages[i] = customStages[i].ToLowerInvariant();
            }
        }

        protected bool AreExpansionRequirementsMet()
        {
            if (!Run.instance)
                return false;

            bool allExpansionsEnabled = false;
            foreach(var addressReferencedExpansion in requiredExpansionDefs)
            {
                if(!addressReferencedExpansion.AssetExists)
                {
                    continue;
                }

                allExpansionsEnabled = Run.instance.IsExpansionEnabled(addressReferencedExpansion.Asset);
            }

            return allExpansionsEnabled;
        }

        protected bool AreUnlockableRequirementsMet()
        {
            if (!Run.instance)
                return false;

            bool flag0 = !requiredUnlock || Run.instance.IsUnlockableUnlocked(requiredUnlock);
            bool flag1 = forbiddenUnlock && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlock);

            return flag0 && !flag1;
        }

        protected bool AreStageRequirementsMet()
        {
            if (!SceneInfo.instance)
                return false;

            DirectorAPI.StageInfo stageInfo = DirectorAPI.StageInfo.ParseInternalStageName(SceneInfo.instance.sceneDef.baseSceneName);

            if(stageInfo.stage == DirectorAPI.Stage.Custom)
            {
                //Return true if customStages is empty, otherwise, check if the current stage is in the custom stages array.
                return customStages.Length == 0 || customStages.Contains(stageInfo.CustomStageName.ToLowerInvariant());
            }

            return stages.HasFlag(stageInfo.stage);
        }
    }
}