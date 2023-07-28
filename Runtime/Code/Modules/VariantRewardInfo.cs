using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// Represents a Reward from a Variant
    /// </summary>
    public class VariantRewardInfo
    {
        /// <summary>
        /// The gold multiplier applied to the variant when killed
        /// </summary>
        public float goldMultiplier;
        /// <summary>
        /// The experience multiplier applied to the variant when killed
        /// </summary>
        public float experienceMultiplier;
        /// <summary>
        /// The chance for this variant to drop a White item
        /// </summary>
        public float whiteChance;
        /// <summary>
        /// The chance for this variant to drop a green item instead of a White item
        /// </summary>
        public float greenChance;
        /// <summary>
        /// The chance for this variant to drop a red item instead of a Green or White item
        /// </summary>
        public float redChance;

        protected List<PickupIndex> whiteItems;
        protected List<PickupIndex> greenItems;
        protected List<PickupIndex> redItems;

        protected int nextRedItem;
        protected int nextGreenItem;
        protected int nextWhiteItem;

        /// <summary>
        /// Set the reward's pickup index lists and the next item to drop
        /// </summary>
        /// <param name="runInstance">The current run</param>
        public virtual void SetIndicesAndNextItems(Run runInstance)
        {
            whiteItems = Run.instance.availableTier1DropList;
            greenItems = Run.instance.availableTier2DropList;
            redItems = Run.instance.availableTier3DropList;

            nextWhiteItem = Run.instance.treasureRng.RangeInt(0, whiteItems.Count);
            nextGreenItem = Run.instance.treasureRng.RangeInt(0, greenItems.Count);
            nextRedItem = Run.instance.treasureRng.RangeInt(0, redItems.Count);
        }

        /// <summary>
        /// Sets <see cref="goldMultiplier"/>, <see cref="experienceMultiplier"/>, <see cref="whiteChance"/>, <see cref="greenChance"/>, and <see cref="redChance"/> values from the average of the variant's VariantTiers, especially useful when a variant merges together
        /// </summary>
        /// <param name="tiers">The tiers of the variant's VariantDefs</param>
        /// <param name="runInstance">The current run</param>
        public virtual void SetFromAverageOfTiers(IEnumerable<VariantTierDef> tiers, Run runInstance)
        {
            var list = tiers.ToList();
            if(list.Count == 0)
            {
                goldMultiplier = 1;
                experienceMultiplier = 1;
                whiteChance = 0;
                greenChance = 0;
                redChance = 0;
                return;
            }
            goldMultiplier = 0;
            experienceMultiplier = 0;
            whiteChance = 0;
            greenChance = 0;
            redChance = 0;

            if(list.Count == 1)
            {
                VariantTierDef td = list.FirstOrDefault();
                goldMultiplier = td.goldMultiplier;
                experienceMultiplier = td.experienceMultiplier;
                whiteChance = td.whiteItemDropChance;
                greenChance = td.greenItemDropChance;
                redChance = td.redItemDropChance;
            }
            else
            {
                goldMultiplier = 1;
                experienceMultiplier = 1;
                whiteChance = 0;
                greenChance = 0;
                redChance = 0;
                foreach (VariantTierDef tierDef in list)
                {
                    goldMultiplier += tierDef.GoldMultiplierMinus1;
                    experienceMultiplier += tierDef.ExperienceMultiplierMinus1;
                    whiteChance += tierDef.whiteItemDropChance;
                    greenChance += tierDef.greenItemDropChance;
                    redChance += tierDef.redItemDropChance;
                }

                int count = list.Count;
                whiteChance /= count;
                greenChance /= count;
                redChance /= count;
            }
            SetIndicesAndNextItems(runInstance);
        }

        /// <summary>
        /// Attempts to spawn a Droplet when the variant dies
        /// </summary>
        /// <param name="damageReport">The damageReport that killed the variant</param>
        /// <param name="master">The variant killer's master, used for CheckRoll</param>
        public virtual void TrySpawnDroplet(DamageReport damageReport, CharacterMaster master = null)
        {
            if (!NetworkServer.active)
            {
                VAPILog.Warning($"TrySpawnDroplet called on client.");
                return;
            }

            if (Util.CheckRoll(redChance, master ? master.luck : 0))
            {
                CreateDroplet(redItems[nextRedItem], damageReport);
            }
            else if (Util.CheckRoll(redChance + greenChance, master ? master.luck : 0))
            {
                CreateDroplet(greenItems[nextGreenItem], damageReport);
            }
            else if (Util.CheckRoll(redChance + greenChance + whiteChance, master ? master.luck : 0))
            {
                CreateDroplet(whiteItems[nextWhiteItem], damageReport);
            }
        }

        /// <summary>
        /// Creates a droplet with the specified pickupIndex
        /// </summary>
        /// <param name="dropletIndex">The pickupIndex of the droplet</param>
        /// <param name="report">The damageReport thtat killed the variant</param>
        protected virtual void CreateDroplet(PickupIndex dropletIndex, DamageReport report)
        {
            PickupDropletController.CreatePickupDroplet(dropletIndex,
                VAPIConfig.itemRewardsSpawnOnPlayer ? report.attacker.transform.position : report.victim.transform.position,
                (Vector3.up * 20) + (Vector3.right * Random.Range(1, 5) + (Vector3.forward * Random.Range(1, 5))));
        }

        /// <summary>
        /// Constructor for VariantRewardInfo
        /// </summary>
        public VariantRewardInfo(float goldMultiplier, float experienceMultiplier, float whiteChance, float greenChance, float redChance)
        {
            this.goldMultiplier = goldMultiplier;
            this.experienceMultiplier = experienceMultiplier;
            this.whiteChance = whiteChance;
            this.greenChance = greenChance;
            this.redChance = redChance;
        }

        /// <summary>
        /// Parameterless Constructor for VariantRewardInfo
        /// </summary>
        public VariantRewardInfo()
        {

        }
    }
}
