using RoR2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace VAPI.Components
{
    /// <summary>
    /// The behaviour that handles VariantRewards
    /// </summary>
    public class BodyVariantReward : MonoBehaviour, IOnKilledServerReceiver
    {
        /// <summary>
        /// The Body's VariantDefs
        /// </summary>
        public ReadOnlyCollection<VariantDef> variantsInBody;
        /// <summary>
        /// Wether the rewards are applied on start
        /// </summary>
        public bool applyOnStart = true;
        /// <summary>
        /// Wether this BodyVariantReward has applied the modifiers for rewards
        /// </summary>
        public bool HasApplied { get; private set; } = false;
        private List<VariantDef> variants = new List<VariantDef>();
        private VariantRewardInfo reward;
        private DeathRewards deathRewards;
        private CharacterBody characterBody;

        private void Awake()
        {
            if (!Run.instance)
                Destroy(this);

            deathRewards = GetComponent<DeathRewards>();
            characterBody = GetComponent<CharacterBody>();
        }

        private void Start()
        {
            if (applyOnStart && !HasApplied)
                Apply();
        }
        /// <summary>
        /// Applies the Rewards to this body
        /// </summary>
        public void Apply()
        {
            if (HasApplied)
            {
                VAPILog.Warning($"{this} has already been applied!");
                return;
            }

            HasApplied = true;
            variantsInBody = new ReadOnlyCollection<VariantDef>(variants);
            reward = new VariantRewardInfo();
            reward.SetFromAverageOfTiers(variants.Select(vd => vd.VariantTierDef), Run.instance);

            deathRewards.goldReward *= (uint)reward.goldMultiplier;
            deathRewards.expReward *= (uint)reward.experienceMultiplier;
        }

        /// <summary>
        /// Adds a VariantDef to the BodyVariantReward's internal variants list.
        /// <para>Returns if <see cref="HasApplied"/> is true</para>
        /// </summary>
        /// <param name="vd">The VariantDef to add</param>
        public void AddVariant(VariantDef vd)
        {
            if (HasApplied)
            {
                VAPILog.Warning($"{this} has already been applied!");
                return;
            }
            variants.Add(vd);
        }

        /// <summary>
        /// Adds a VariantDef to the BodyVariantReward's internal variants list
        /// <para>Returns if <see cref="HasApplied"/> is true</para>
        /// </summary>
        /// <param name="variantDefs">The VariantDefs to add</param>
        public void AddVariants(IEnumerable<VariantDef> variantDefs)
        {
            if (HasApplied)
            {
                VAPILog.Warning($"{this} has already been applied!");
                return;
            }
            variants.AddRange(variantDefs);
        }

        /// <summary>
        /// Raised when the variant gets killed, do not trigger this yourself.
        /// </summary>
        public void OnKilledServer(DamageReport damageReport)
        {
            if (damageReport == null)
                return;

            if (damageReport.attackerTeamIndex != TeamIndex.Player)
                return;

            if (!Run.instance)
                return;

            if (!damageReport.attackerMaster)
                return;

            if (Run.instance.isRunStopwatchPaused)
            {
                var chanceInRealm = VAPIConfig.hiddenRealmsItemRollChance.Value;
                if (chanceInRealm <= 0)
                    return;

                if (Util.CheckRoll(chanceInRealm, VAPIConfig.luckAffectsItemRewards.Value ? damageReport.attackerMaster.luck : 0))
                {
                    reward.TrySpawnDroplet(damageReport);
                }
            }
            reward.TrySpawnDroplet(damageReport);

            if (VariantSpawnManager.Instance)
            {
                VariantSpawnManager.Instance.OnVariantKilled(variantsInBody, damageReport);
            }
        }
    }
}