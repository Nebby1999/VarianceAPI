#nullable enable
using HG;
using RoR2;
using RoR2.ConVar;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace VAPI
{
    /// <summary>
    /// A runtime component that's added to a Character that needs to drop rewards via VAPI's reward system. You do not need to add this component manually.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class VariantDeathRewards : MonoBehaviour, IOnKilledServerReceiver
    {
        private static readonly ConVarFlags _flags =
#if DEBUG
            ConVarFlags.None
#else
            ConVarFlags.Cheat
#endif
            ;

        private static BoolConVar _logDeathRewards = new BoolConVar("vapi_cv_logdeathrewards", _flags, "0", "Enables logging for the VariantDeathRewards");

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
        private struct DisposeModifiers : IDisposable
        {
            public DeathRewards? targetDeathRewards;

            public float utilizedGoldRewardCoefficient;
            public float utilizedExpRewardCoefficient;

            public void Dispose()
            {
                if(targetDeathRewards)
                {
                    float modifiedGoldReward = targetDeathRewards!.goldReward;
                    float newGoldReward = modifiedGoldReward / utilizedGoldRewardCoefficient;
                    targetDeathRewards.goldReward = (uint)newGoldReward;

                    float modifiedExpReward = targetDeathRewards!.expReward;
                    float newExpReward = modifiedExpReward / utilizedExpRewardCoefficient;
                    targetDeathRewards.expReward = (uint)newExpReward;
                }

                targetDeathRewards = null;
                utilizedExpRewardCoefficient = 0;
                utilizedGoldRewardCoefficient = 0;
            }
        }

        /// <summary>
        /// Access to the variant controller for the body
        /// </summary>
        public CharacterBodyVariantController characterBodyVariantController { get; private set; }
        /// <summary>
        /// Access to the character body
        /// </summary>
        public CharacterBody characterBody { get; private set; }
        /// <summary>
        /// Access to the base DeathRewards component, this may be null
        /// </summary>
        public DeathRewards? deathRewards { get; private set; }

        /// <summary>
        /// Set on the server. If true, this variant will not drop any items
        /// </summary>
        public bool noItemRewardServer;

        /// <summary>
        /// Set on the server. If true, this variant will not recieve any gold or experience modifiers
        /// </summary>
        public bool noGoldExpModificationsServer;

        private float _pickupRewardChance;
        private WeightedSelection<ItemTier> _pickupTierRewardSelection = new WeightedSelection<ItemTier>();

        private void Awake()
        {
            deathRewards = GetComponent<DeathRewards>();
            characterBody = GetComponent<CharacterBody>();
            characterBodyVariantController = GetComponent<CharacterBodyVariantController>();
            Log($"Awake");
        }

        private void OnEnable()
        {
            if(characterBodyVariantController)
            {
                characterBodyVariantController.onVariantsUnapplied += OnVariantsUnapplied;
                characterBodyVariantController.onVariantsApplied += OnVariantsApplied;
                Log($"Subscribed to events on CharacterBodyVariantController");
            }
        }

        private void OnDisable()
        {
            if (characterBodyVariantController)
            {
                characterBodyVariantController.onVariantsUnapplied -= OnVariantsUnapplied;
                characterBodyVariantController.onVariantsApplied -= OnVariantsApplied;
                Log($"Unsubscribed to events on CharacterBodyVariantController");
            }
        }

        private DisposeModifiers? _disposeModifiers;

        private void OnVariantsApplied(CharacterVariantDef[] variants)
        {
            if (variants.Length == 0)
            {
                Log($"No variants applied, not doing modifications");
                return;
            }

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
                    Log($"{variant.cachedName} has no tier, ignoring.");
                    continue;
                }

                CharacterVariantTierDef tierDef = variant.variantTier!;

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
                Log("No variants have tiers, aborting reward application");
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


            if (deathRewards && noGoldExpModificationsServer == false)
            {
                if (finalGoldMultiplier > 0)
                {
                    float baseGoldReward = deathRewards!.goldReward;
                    float newGoldReward = baseGoldReward * finalGoldMultiplier;
                    deathRewards!.goldReward = (uint)newGoldReward;
                }
                if (finalExpMultiplier > 0)
                {
                    float baseExpReward = deathRewards!.expReward;
                    float newExpReward = baseExpReward * finalExpMultiplier;
                    deathRewards!.expReward = (uint)newExpReward;
                }
                _disposeModifiers = new DisposeModifiers
                {
                    targetDeathRewards = deathRewards,
                    utilizedExpRewardCoefficient = finalExpMultiplier,
                    utilizedGoldRewardCoefficient = finalGoldMultiplier
                };
            }

            float totalChanceForItemPickup = 0f;
            foreach (var (itemTier, chanceWithCount) in pickupRewardDictionary)
            {
                float calculatedFinalChance = chanceWithCount.CalculateFinalChance();
                totalChanceForItemPickup += calculatedFinalChance;
                _pickupTierRewardSelection.AddChoice(itemTier, calculatedFinalChance);
            }

            _pickupRewardChance = totalChanceForItemPickup;
            Log($"VariantDeathRewards application finished:\r\n" +
                $"finalGoldMultiplier: {(noGoldExpModificationsServer ? 1 : finalGoldMultiplier)}\r\n" +
                $"finalExpMultiplier: {(noGoldExpModificationsServer ? 1 : finalExpMultiplier)}\r\n" +
                $"pickupRewardChance: {(noItemRewardServer ? 0 : totalChanceForItemPickup)}\r\n" +
                $"pickupTierWeights: {(noItemRewardServer ? "None" : GetPickupTierWeights())}");
        }

        private string GetPickupTierWeights()
        {
            using var _ = HG.StringBuilderPool.RentStringBuilder(out var sb);

            for(int i = 0; i < _pickupTierRewardSelection.Count; i++)
            {
                var choice = _pickupTierRewardSelection.GetChoice(i);
                sb.AppendLine($"{choice.value}: {choice.weight}");
            }
            return sb.ToString();
        }

        private void OnVariantsUnapplied()
        {
            _disposeModifiers?.Dispose();
            _pickupTierRewardSelection.Clear();
            _pickupRewardChance = 0f;
            Log($"Unapplied and discarded modifiers");
        }

        /// <summary>
        /// Do not call this method manually.
        /// </summary>
        public void OnKilledServer(DamageReport damageReport)
        {
            if(!noItemRewardServer)
            {
                Log($"Not dropping reward as this has been marked for no item rewards");
            }
            if (!characterBodyVariantController || damageReport == null || damageReport.victimBody == null || damageReport.victimBody != characterBody || !VariantSpawnManager.instance || !Run.instance)
            {
                Log($"Not dropping reward as there's no CharacterBodyVariantController or not enough information on the killing damageReport");
                return;
            }

            var variantDefs = characterBodyVariantController.GetCharacterVariantDefs();
            if (variantDefs.Length == 0)
            {
                Log("Not dropping item as there's no variants applied");
                return;
            }

            //Utilize the attacker master, prioritize owner if it exists
            CharacterMaster? attackerMaster = damageReport.attackerMaster;
            if (damageReport.attackerOwnerMaster)
            {
                attackerMaster = damageReport.attackerOwnerMaster;
            }

            if (!attackerMaster)
            {
                Log($"Not dropping item as there's no attacker master");
                return;
            }

            CharacterBody? attackerBody = attackerMaster.GetBody();

            if (_pickupTierRewardSelection.Count <= 0)
            {
                Log($"Not dropping item as the pickupTierRewardSelection has no options");
                return;
            }

            float luck = VAPIConfig._luckAffectsItemRewards ? attackerMaster.luck : 0;

            //We should curb the amount of rewards given during hidden realms, if the user so desires.
            if (Run.instance.isRunStopwatchPaused)
            {
                var chanceInRealm = VAPIConfig._hiddenRealmsRollChance!.value;
                if (chanceInRealm <= 0)
                {
                    Log($"Not droppint item as items cannot spawn on hidden realms");
                    return;
                }

                bool canDropItemOnHiddenRealm = Util.CheckRoll(chanceInRealm, luck, attackerMaster);
                if (!canDropItemOnHiddenRealm)
                {
                    Log($"Not dropping item as the {chanceInRealm}% chance for item in hidden realms failed");
                    return;
                }
            }

            //If this check roll doesnt pass, then return, no reward for u
            if (!Util.CheckRoll(_pickupRewardChance, luck, attackerMaster))
            {
                Log($"Not dropping item as items as the {_pickupRewardChance}% chance for item drop has failed.");
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

        private void Log(object data)
        {
            if (_logDeathRewards.value)
            {
                VAPILog.Message($"[{gameObject.name}.VariantDeathRewards]: {data}");
            }
        }
    }
}