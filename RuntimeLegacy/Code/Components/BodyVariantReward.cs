using RoR2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace VAPI.Legacy.Components
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
        public bool hasApplied { get; private set; } = false;
        private List<VariantDef> _variants = new List<VariantDef>();
        private VariantRewardInfo _reward;
        private DeathRewards _deathRewards;
        private CharacterBody _characterBody;

        private void Awake()
        {
            if (!Run.instance)
                Destroy(this);

            _deathRewards = GetComponent<DeathRewards>();
            _characterBody = GetComponent<CharacterBody>();
        }

        private void Start()
        {
            if (applyOnStart && !hasApplied)
                Apply();
        }
        /// <summary>
        /// Applies the Rewards to this body
        /// </summary>
        public void Apply()
        {
            if (hasApplied)
            {
                VAPILog.Warning($"{this} has already been applied!");
                return;
            }

            hasApplied = true;
            variantsInBody = new ReadOnlyCollection<VariantDef>(_variants);
            _reward = new VariantRewardInfo();
            _reward.SetFromAverageOfTiers(_variants.Select(vd => vd.variantTierDef), Run.instance);

            _deathRewards.goldReward *= (uint)_reward.goldMultiplier;
            _deathRewards.expReward *= (uint)_reward.experienceMultiplier;
        }

        /// <summary>
        /// Adds a VariantDef to the BodyVariantReward's internal variants list
        /// <para>Returns if <see cref="hasApplied"/> is true</para>
        /// </summary>
        /// <param name="variantDefs">The VariantDefs to add</param>
        public void AddVariants(IEnumerable<VariantDef> variantDefs)
        {
            if (hasApplied)
            {
                VAPILog.Warning($"{this} has already been applied!");
                return;
            }
            foreach(VariantDef variant in variantDefs)
            {
                AddVariant(variant);
            }
        }

        /// <summary>
        /// Adds a VariantDef to the BodyVariantReward's internal variants list.
        /// <para>Returns if <see cref="hasApplied"/> is true</para>
        /// </summary>
        /// <param name="vd">The VariantDef to add</param>
        public void AddVariant(VariantDef vd)
        {
            if (hasApplied)
            {
                VAPILog.Warning($"{this} has already been applied!");
                return;
            }
            if(vd)
            {
                _variants.Add(vd);
            }
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

            if (_reward == null)
                return;

            if (Run.instance.isRunStopwatchPaused)
            {
                var chanceInRealm = VAPIConfig._hiddenRealmsItemRollChance.value;
                if (chanceInRealm <= 0)
                    return;

                if (Util.CheckRoll(chanceInRealm, VAPIConfig._luckAffectsItemRewards ? damageReport.attackerMaster.luck : 0))
                {
                    _reward.TrySpawnDroplet(damageReport);
                }
            }
            _reward.TrySpawnDroplet(damageReport);

            if (VariantSpawnManager.instance)
            {
                VariantSpawnManager.instance.OnVariantKilled(variantsInBody, damageReport);
            }
        }
    }
}