#nullable enable
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    public class VariantDeathRewards : MonoBehaviour, IOnKilledServerReceiver
    {
        private sealed class ChanceWithCount
        {
            public float highestBaseChanceObserved = float.NegativeInfinity;
            public float compoundedChance = 0f;
            public int compoundedCount = 0;

            public float CalculateFinalChance()
            {
                float result = highestBaseChanceObserved;
                if (compoundedCount > 1)
                {
                    float compoundedChanceWithoutHighestBaseChanceObserved = compoundedChance - highestBaseChanceObserved;
                    float average = compoundedChanceWithoutHighestBaseChanceObserved / (compoundedCount - 1);
                    result += average;
                }
                return result;
            }
            public void AddChance(float chance)
            {
                if (chance > highestBaseChanceObserved)
                {
                    highestBaseChanceObserved = chance;
                }
                compoundedChance += chance;
                compoundedCount++;
            }
        }
        public CharacterBodyVariantController characterBodyVariantController { get; private set; }
        public CharacterBody characterBody { get; private set; }
        public DeathRewards? deathRewards { get; private set; }

        public bool supressRewards;

        private float _pickupRewardChance;
        private WeightedSelection<ItemTier> _pickupTierRewardSelection = new WeightedSelection<ItemTier>();

        private void Awake()
        {
            deathRewards = GetComponent<DeathRewards>();
            characterBody = GetComponent<CharacterBody>();
            characterBodyVariantController = GetComponent<CharacterBodyVariantController>();
        }

        private void OnEnable()
        {
            characterBodyVariantController.onBecameVariantGlobal += OnBecameVariant;
        }

        private void OnDisable()
        {
            characterBodyVariantController.onBecameVariantGlobal -= OnBecameVariant;
        }

        private bool _rewardsApplied = false;


        private void OnBecameVariant(NetworkedVariantCollection variants)
        {
            if (_rewardsApplied)
            {
                VAPILog.Warning($"Cannot apply rewards twice for {this}");
                return;
            }
            _rewardsApplied = true;

            if (supressRewards)
                return;

            float highestGoldMultiplier = float.NegativeInfinity;
            float highestExpMultiplier = float.NegativeInfinity;
            float goldMultiplierAverage = 0;
            float expMultiplierAverage = 0;
            Dictionary<ItemTier, ChanceWithCount> pickupRewardDictionary = new Dictionary<ItemTier, ChanceWithCount>();

            int variantTierCount = 0;
            foreach (CharacterVariantDef variant in variants)
            {
                if (!variant.variantTier)
                {
                    continue;
                }

                VariantTierDef tierDef = variant.variantTier!;

                if (tierDef.goldRewardCoefficient > highestGoldMultiplier)
                {
                    highestGoldMultiplier = tierDef.goldRewardCoefficient;
                }
                if (tierDef.experienceRewardCoefficient > highestExpMultiplier)
                {
                    highestExpMultiplier = tierDef.experienceRewardCoefficient;
                }

                goldMultiplierAverage += tierDef.goldRewardCoefficient;
                expMultiplierAverage += tierDef.experienceRewardCoefficient;

                if (tierDef.canDropCommon)
                {
                    pickupRewardDictionary.TryAdd(ItemTier.Tier1, new ChanceWithCount());
                    pickupRewardDictionary[ItemTier.Tier1].AddChance(tierDef.commonItemRewardChance);
                }
                if (tierDef.canDropUncommon)
                {
                    pickupRewardDictionary.TryAdd(ItemTier.Tier2, new ChanceWithCount());
                    pickupRewardDictionary[ItemTier.Tier2].AddChance(tierDef.uncommonItemRewardChance);
                }
                if (tierDef.canDropLegendary)
                {
                    pickupRewardDictionary.TryAdd(ItemTier.Tier3, new ChanceWithCount());
                    pickupRewardDictionary[ItemTier.Tier3].AddChance(tierDef.legendaryItemRewardChance);
                }
                if (tierDef.canDropBoss && (deathRewards?.bossDropTable ?? false)) //Only add boss drop chance if body can drop boss item.
                {
                    pickupRewardDictionary.TryAdd(ItemTier.Boss, new ChanceWithCount());
                    pickupRewardDictionary[ItemTier.Boss].AddChance(tierDef.bossItemRewardChance);
                }

                variantTierCount++;
            }

            //No variants have tiers, so abort application of rewards
            if (variantTierCount == 0)
            {
                return;
            }

            float finalGoldMultiplier = highestGoldMultiplier;
            float finalExpMultiplier = highestExpMultiplier;

            if (variantTierCount > 1)
            {
                goldMultiplierAverage -= highestGoldMultiplier;
                expMultiplierAverage -= highestExpMultiplier;

                goldMultiplierAverage /= variantTierCount - 1;
                expMultiplierAverage /= variantTierCount - 1;

                finalGoldMultiplier += goldMultiplierAverage;
                finalExpMultiplier += expMultiplierAverage;
            }


            if (deathRewards)
            {
                if (finalGoldMultiplier > 0)
                {
                    deathRewards!.goldReward *= (uint)finalGoldMultiplier;
                }
                if (finalExpMultiplier > 0)
                {
                    deathRewards!.expReward *= (uint)finalExpMultiplier;
                }
            }

            float totalChanceForItemPickup = 0f;
            foreach (var (itemTier, chanceWithCount) in pickupRewardDictionary)
            {
                float calculatedFinalChance = chanceWithCount.CalculateFinalChance();
                totalChanceForItemPickup += calculatedFinalChance;
                _pickupTierRewardSelection.AddChoice(itemTier, calculatedFinalChance);
            }

            _pickupRewardChance = totalChanceForItemPickup;
        }

        public void OnKilledServer(DamageReport damageReport)
        {
            if (supressRewards)
                return;

            if (damageReport == null)
                return;

            if (damageReport.victimBody == null || damageReport.victimBody != characterBody)
                return;

            if (!VariantSpawnManager.instance)
            {
                return;
            }

            if (!Run.instance)
            {
                return;
            }

            //Utilize the attacker master, prioritize owner if it exists
            CharacterMaster? attackerMaster = damageReport.attackerMaster;
            if (damageReport.attackerOwnerMaster)
            {
                attackerMaster = damageReport.attackerOwnerMaster;
            }
            CharacterBody? attackerBody = attackerMaster.GetBody();

            if (!attackerMaster)
            {
                return;
            }

            if (_pickupTierRewardSelection.Count <= 0)
            {
                return;
            }

            float luck = VAPIConfig._luckAffectsItemRewards ? attackerMaster.luck : 0;

            //We should curb the amount of rewards given during hidden realms, if the user so desires.
            if (Run.instance.isRunStopwatchPaused)
            {
                var chanceInRealm = VAPIConfig._hiddenRealmsRollChance!.value;
                if (chanceInRealm <= 0)
                {
                    return;
                }

                bool canDropItemOnHiddenRealm = VariantSpawnManager.instance!.variantRewardRng.CheckRoll(chanceInRealm, luck, attackerMaster);
                if (!canDropItemOnHiddenRealm)
                {
                    return;
                }
            }

            //If this check roll doesnt pass, then return, no reward for u
            if (!VariantSpawnManager.instance!.variantRewardRng.CheckRoll(_pickupRewardChance, luck, attackerMaster))
            {
                return;
            }

            //Evaluate the tier reward.
            ItemTier pickupReward = _pickupTierRewardSelection.Evaluate(VariantSpawnManager.instance!.variantRewardRng!.nextNormalizedFloat);

            //spawn droplet
            switch (pickupReward)
            {
                case ItemTier.Tier1:
                    CreateDroplet(Run.instance.availableTier1DropList, attackerBody, damageReport.victimBody);
                    break;
                case ItemTier.Tier2:
                    CreateDroplet(Run.instance.availableTier2DropList, attackerBody, damageReport.victimBody);
                    break;
                case ItemTier.Tier3:
                    CreateDroplet(Run.instance.availableTier3DropList, attackerBody, damageReport.victimBody);
                    break;
                case ItemTier.Boss:
                    if (deathRewards && deathRewards!.bossDropTable)
                    {
                        CreateDroplet(deathRewards!.bossDropTable.GeneratePickup(VariantSpawnManager.instance!.variantRewardRng), attackerBody, damageReport.victimBody);
                    }
                    else
                    {
                        VAPILog.Warning($"PickupTier reward for {this} rolled Boss tier but character has no boss drop table!");
                        return;
                    }
                    break;
            }
        }

        private void CreateDroplet(List<PickupIndex> pickupIndices, CharacterBody killerBody, CharacterBody victimBody)
        {
            PickupIndex pickupIndex = pickupIndices[VariantSpawnManager.instance!.variantRewardRng!.RangeInt(0, pickupIndices.Count)];
            CreateDroplet(new UniquePickup(pickupIndex), killerBody, victimBody);
        }

        private void CreateDroplet(UniquePickup pickupIndex, CharacterBody killerBody, CharacterBody victimBody)
        {
            Vector3 velocity = (Vector3.up * 20) + (Vector3.right * Random.Range(1, 5) + (Vector3.forward * Random.Range(1, 5)));
            Vector3 position = victimBody.corePosition;

            if(VAPIConfig._itemRewardsSpawnOnKiller)
            {
                if(killerBody)
                {
                    position = killerBody.corePosition;
                }
            }

#pragma warning disable CS0612 // Type or member is obsolete
            PickupDropletController.CreatePickupDroplet(pickupIndex, position, velocity);
#pragma warning restore CS0612 // Type or member is obsolete
        }
    }
}