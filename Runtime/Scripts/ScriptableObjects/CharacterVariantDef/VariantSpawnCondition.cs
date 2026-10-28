#nullable enable
using System;
using System.Collections;
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
    /// <summary>
    /// Interface for implementing a custom Spawn Condition for a Variant.
    /// <br></br>
    /// If <see cref="IsAvailable"/> resolves to true, then the Variant can spawn. This is called each time the Stage changes in the run.
    /// <br></br>
    /// Must support cloning via the <see cref="ICloneable"/> implementation
    /// <para></para>
    /// <b>Built in spawn conditions:</b>
    /// <list type="bullet">
    /// <item><see cref="BasicSpawnCondition"/>: The Reimplementation of VAPI2.0's VariantSpawnCondition.</item>
    /// </list>
    /// </summary>
    public interface IVariantSpawnCondition : IValidatable, ICloneable
    {
        /// <summary>
        /// The required unlockable for this variant, this can return null and it's utilized to determine wether a Variant requires an UnlockableDef to be accessible from the In-Lobby RuleBook
        /// </summary>
        public UnlockableDef? requiredUnlock { get; }

        /// <summary>
        /// The required expansion defs for this variant, this can return null and it's utilized to determine wether a Variant requires the Expansions in this Enumerable to be accessible from the In-Lobby RuleBook
        /// </summary>
        public IEnumerable<ExpansionDef>? requiredExpansionDefs { get; }

        /// <summary>
        /// Wether this variant is Available or not
        /// </summary>
        /// <returns>True if the variant is available, otherwise false.</returns>
        public bool IsAvailable();
    }

    /// <summary>
    /// A Spawn Condition that reimplements VAPI2.0's VariantSpawnCondition scriptable object
    /// <br></br>
    /// Open to inheritance, make sure to override <see cref="IsAvailable"/> and <see cref="Clone"/>
    /// </summary>
    [Serializable]
    public class BasicSpawnCondition : IVariantSpawnCondition
    {
        [Tooltip("The minimum stage completions for this variant.")]
        public int minimumStageCompletions = -1;

        /// <summary>
        /// Which stages this variant can spawn
        /// </summary>
        public DirectorAPI.Stage stages { get => _stages; set => _stages = value; }

        UnlockableDef? IVariantSpawnCondition.requiredUnlock => requiredUnlock.LoadAssetNow();
        IEnumerable<ExpansionDef>? IVariantSpawnCondition.requiredExpansionDefs
        {
            get
            {
                List<ExpansionDef> expansionDefs = new List<ExpansionDef>();
                foreach(var expansion in requiredExpansionDefs)
                {
                    var exp = expansion.LoadAssetNow();
                    if(exp)
                        expansionDefs.Add(exp);
                }
                return expansionDefs;
            }
        }

        [Tooltip("The stages where this variant can spawn")]
        [SerializeField] private DirectorAPI.StageSerde _stages;
        [Tooltip("A list of custom stages where this variant can spawn")]
        public string[] customStages = Array.Empty<string>();

        [Space]

        [Tooltip("This unlockable must be unlocked for this variant to spawn")]
        public AddressReferencedUnlockableDef requiredUnlock = new AddressReferencedUnlockableDef();

        [Tooltip("This unlockable CANNOT be unlocked for this variant to spawn")]
        public AddressReferencedUnlockableDef forbiddenUnlock = new AddressReferencedUnlockableDef();

        [Tooltip("These expansions must be enabled for this variant to spawn")]
        public AddressReferencedExpansionDef[] requiredExpansionDefs = Array.Empty<AddressReferencedExpansionDef>();

        /// <summary>
        /// Returns true if the variant is available using these spawn conditions.
        /// <br></br>
        /// Requires the following methods to return true:
        /// <list type="bullet">
        /// <item><see cref="AreExpansionRequirementsMet"/></item>
        /// <item><see cref="AreUnlockableRequirementsMet"/></item>
        /// <item><see cref="AreStageRequirementsMet"/></item>
        /// <item><see cref="IsStageCountGreaterThanMinimum"/></item>
        /// </list>
        /// </summary>
        public virtual bool IsAvailable()
        {
            bool expansionRequirementMet = AreExpansionRequirementsMet();
            bool unlockableRequirementMet = AreUnlockableRequirementsMet();
            bool allowedInStage = AreStageRequirementsMet();
            bool stageCompletionRequirementReached = IsStageCountGreaterThanMinimum();

            return expansionRequirementMet && unlockableRequirementMet && allowedInStage && stageCompletionRequirementReached;
        }

        /// <summary>
        /// Validate method
        /// </summary>
        public virtual void Validate()
        {
            for(int i = 0; i < customStages.Length; i++)
            {
                customStages[i] = customStages[i].ToLowerInvariant();
            }
        }

        /// <summary>
        /// Returns true if <see cref="Run.stageClearCount"/> is greater or equal than <see cref="minimumStageCompletions"/>
        /// </summary>
        protected bool IsStageCountGreaterThanMinimum()
        {
            if (!Run.instance)
                return false;

            return Run.instance.stageClearCount >= minimumStageCompletions;
        }

        /// <summary>
        /// Returns true if all the expansions in <see cref="requiredExpansionDefs"/> are enabled in the run.
        /// </summary>
        protected bool AreExpansionRequirementsMet()
        {
            if (!Run.instance)
                return false;

            foreach(var addressReferencedExpansion in requiredExpansionDefs)
            {
                if(!addressReferencedExpansion.AssetExists)
                {
                    continue;
                }

                if(!Run.instance.IsExpansionEnabled(addressReferencedExpansion.Asset))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if unlockable requirements are met
        /// </summary>
        protected bool AreUnlockableRequirementsMet()
        {
            if (!Run.instance)
                return false;

            bool flag0 = !requiredUnlock || Run.instance.IsUnlockableUnlocked(requiredUnlock);
            bool flag1 = forbiddenUnlock && Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlock);

            return flag0 && !flag1;
        }

        /// <summary>
        /// Returns true if stage requirements are met.
        /// </summary>
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

            if(stages == 0L)
            {
                return true;
            }
            return (stages & stageInfo.stage) == 0;
        }

        /// <summary>
        /// Returns a clone of this as an instance of <see cref="BasicSpawnCondition"/>
        /// <br></br>
        /// Override this method if youre inheriting from this class.
        /// </summary>
        public virtual object Clone()
        {
            var result = new BasicSpawnCondition()
            {
                minimumStageCompletions = minimumStageCompletions,
                _stages = _stages
            };
            HG.ArrayUtils.CloneTo(customStages, ref result.customStages);
            result.forbiddenUnlock = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedUnlockableDef, UnlockableDef>(forbiddenUnlock);
            result.requiredUnlock = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedUnlockableDef, UnlockableDef>(requiredUnlock);
            HG.ArrayUtils.EnsureCapacity(ref result.requiredExpansionDefs, requiredExpansionDefs.Length);
            for(int i = 0; i < result.requiredExpansionDefs.Length; i++)
            {
                result.requiredExpansionDefs[i] = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedExpansionDef, ExpansionDef>(requiredExpansionDefs[i]);
            }
            return result;
        }

        #region Constructors
        public static BasicSpawnCondition MinStages(int minStageCompletions)
        {
            return new BasicSpawnCondition(minStageCompletions, null, null, null, null, null);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BasicSpawnCondition(int? minimumStageCompletions, DirectorAPI.Stage? stages, string[]? customStages, Either<string, UnlockableDef>? forbiddenUnlockableKeyOrReference = null, Either<string, UnlockableDef>? requiredUnlockableKeyOrReference = null, Either<string, ExpansionDef>[]? requiredExpansionKeyOrReferences = null)
        {
            this.minimumStageCompletions = minimumStageCompletions ?? -1;
            this.stages = stages ?? ((DirectorAPI.Stage)0L);
            this.customStages = customStages ?? Array.Empty<string>();

            if(forbiddenUnlockableKeyOrReference.HasValue)
            {

                if(forbiddenUnlockableKeyOrReference.Value.isA)
                {
                    forbiddenUnlock.Address = forbiddenUnlockableKeyOrReference.Value.a;
                }
                else if(forbiddenUnlockableKeyOrReference.Value.b)
                {
                    forbiddenUnlock.Asset = forbiddenUnlockableKeyOrReference.Value.b;
                }
            }
            else
            {
                forbiddenUnlock = new AddressReferencedUnlockableDef();
            }

            if (requiredUnlockableKeyOrReference.HasValue)
            {
                if (requiredUnlockableKeyOrReference.Value.isA)
                {
                    requiredUnlock.Address = requiredUnlockableKeyOrReference.Value.a;
                }
                else if (requiredUnlockableKeyOrReference.Value.b)
                {
                    requiredUnlock.Asset = requiredUnlockableKeyOrReference.Value.b;
                }
            }
            else
            {
                requiredUnlock = new AddressReferencedUnlockableDef();
            }

            if (requiredExpansionKeyOrReferences != null)
            {

                requiredExpansionDefs = new AddressReferencedExpansionDef[requiredExpansionKeyOrReferences.Length];
                for (int i = 0; i < requiredExpansionDefs.Length; i++)
                {
                    if (requiredExpansionKeyOrReferences[i].isA)
                    {
                        requiredExpansionDefs[i] = new AddressReferencedExpansionDef(requiredExpansionKeyOrReferences[i].a);
                    }
                    else if (requiredExpansionKeyOrReferences[i].b)
                    {
                        requiredExpansionDefs[i] = new AddressReferencedExpansionDef(requiredExpansionKeyOrReferences[i].b);
                    }
                }
            }
            else
            {
                requiredExpansionDefs = Array.Empty<AddressReferencedExpansionDef>();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BasicSpawnCondition() { }
        #endregion
    }

    /// <summary>
    /// A Spawn Condition that returns true if the <see cref="VariantDependantCondition.dependantVariantDef"/>'s availability also resolves to true.
    /// </summary>
    [Serializable]
    public struct VariantDependantCondition : IVariantSpawnCondition
    {
        public CataloguedVariantReference dependantVariantDef;

        public UnlockableDef? requiredUnlock
        {
            get
            {
                var variantDef = dependantVariantDef.GetCharacterVariantDef();
                if(variantDef != null && variantDef.spawnCondition != null)
                {
                    return variantDef.spawnCondition.requiredUnlock;
                }
                return null;
            }
        }

        public IEnumerable<ExpansionDef>? requiredExpansionDefs
        {
            get
            {
                var variantDef = dependantVariantDef.GetCharacterVariantDef();
                if(variantDef != null && variantDef.spawnCondition != null)
                {
                    return variantDef.spawnCondition.requiredExpansionDefs;
                }
                return null;
            }
        }

        public object Clone()
        {
            return new VariantDependantCondition(dependantVariantDef);
        }

        public bool IsAvailable()
        {
            CharacterVariantDef? variantDef = dependantVariantDef.GetCharacterVariantDef();
            if(variantDef == null)
            {
                return true;
            }

            return VAPIUtils.IsVariantAvailable(variantDef);
        }

        public void Validate()
        {
            if(dependantVariantDef.IsValid() == false)
            {
                VAPILog.Warning($"VariantDependantCondition {this} has an invalid dependantVariantDef field");
            }
        }

        public VariantDependantCondition(CataloguedVariantReference _dependantVariantDef)
        {
            dependantVariantDef = _dependantVariantDef;
        }

        public VariantDependantCondition(string _dependantVariantDefName)
        {
            dependantVariantDef = new CataloguedVariantReference(_dependantVariantDefName);
        }

        public VariantDependantCondition(CharacterVariantDef _dependantVariantDef)
        {
            dependantVariantDef = new CataloguedVariantReference(_dependantVariantDef);
        }
    }
}