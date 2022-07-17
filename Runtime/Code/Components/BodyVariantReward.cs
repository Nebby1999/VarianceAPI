using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using System.Collections.ObjectModel;

namespace VAPI.Components
{
    public class BodyVariantReward : MonoBehaviour, IOnKilledServerReceiver
    {
        public ReadOnlyCollection<VariantDef> variantsInBody;
        public bool applyOnStart = true;
        private List<VariantDef> variants = new List<VariantDef>();

        private bool hasApplied = false;
        private VariantRewardInfo reward;
        private DeathRewards deathRewards;
        private CharacterBody characterBody;
        
        public void Awake()
        {
            if (!Run.instance)
                Destroy(this);

            deathRewards = GetComponent<DeathRewards>();
            characterBody = GetComponent<CharacterBody>();
        }

        private void Start()
        {
            if (applyOnStart && !hasApplied)
                Apply();
        }
        public void Apply()
        {
            if (hasApplied)
            {
                VAPILog.Warning($"{this} has already been applied!");
                return;
            }

            hasApplied = true;
            variantsInBody = new ReadOnlyCollection<VariantDef>(variants);
            reward = new VariantRewardInfo();
            reward.SetFromAverageOfTiers(variants.Select(vd => vd.VariantTierDef), Run.instance);

            deathRewards.goldReward *= (uint)reward.goldMultiplier;
            deathRewards.expReward *= (uint)reward.experienceMultiplier;
        }

        public void AddVariant(VariantDef vd) => variants.Add(vd);

        public void AddVariants(IEnumerable<VariantDef> variantDefs) => variants.AddRange(variantDefs);

        public void OnKilledServer(DamageReport damageReport)
        {
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