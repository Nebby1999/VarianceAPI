using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;

namespace VAPI
{
    public class VariantRewardInfo
    {
        public float goldMultiplier;
        public float experienceMultiplier;
        public float whiteChance;
        public float greenChance;
        public float redChance;

        private List<PickupIndex> whiteItems;
        private List<PickupIndex> greenItems;
        private List<PickupIndex> redItems;

        private int nextRedItem;
        private int nextGreenItem;
        private int nextWhiteItem;

        public virtual void SetIndicesAndNextItems(Run runInstance)
        {
            whiteItems = Run.instance.availableTier1DropList;
            greenItems = Run.instance.availableTier2DropList;
            redItems = Run.instance.availableTier3DropList;

            nextWhiteItem = Run.instance.treasureRng.RangeInt(0, whiteItems.Count);
            nextGreenItem = Run.instance.treasureRng.RangeInt(0, greenItems.Count);
            nextRedItem = Run.instance.treasureRng.RangeInt(0, redItems.Count);
        }

        public virtual void SetFromAverageOfTiers(IEnumerable<VariantTierDef> tiers, Run instance)
        {
            goldMultiplier = 0;
            experienceMultiplier = 0;
            whiteChance = 0;
            greenChance = 0;
            redChance = 0;

            foreach(VariantTierDef tierDef in tiers)
            {
                goldMultiplier += tierDef.GoldMultiplierMinus1;
                experienceMultiplier += tierDef.ExperienceMultiplierMinus1;
                whiteChance += tierDef.whiteItemDropChance;
                greenChance += tierDef.greenItemDropChance;
                redChance += tierDef.redItemDropChance;
            }

            int count = tiers.Count();
            whiteChance /= count;
            greenChance /= count;
            redChance /= count;

            SetIndicesAndNextItems(instance);
        }

        public virtual void TrySpawnDroplet(DamageReport damageReport, CharacterMaster master = null)
        {
            if (!NetworkServer.active)
            {
                VAPILog.Warning($"TrySpawnDroplet called on client.");
                return;
            }

            if(Util.CheckRoll(redChance, master ? master.luck : 0))
            {
                CreateDroplet(redItems[nextRedItem], damageReport);
            }
            else if(Util.CheckRoll(redChance + greenChance, master ? master.luck : 0))
            {
                CreateDroplet(greenItems[nextGreenItem], damageReport);
            }
            else if(Util.CheckRoll(redChance + greenChance + whiteChance, master ? master.luck : 0))
            {
                CreateDroplet(whiteItems[nextWhiteItem], damageReport);
            }
        }

        protected virtual void CreateDroplet(PickupIndex dropletIndex, DamageReport report)
        {
            PickupDropletController.CreatePickupDroplet(dropletIndex,
                VAPIConfig.itemRewardsSpawnOnPlayer.Value ? report.attacker.transform.position : report.victim.transform.position,
                (Vector3.up * 20) + (Vector3.right * Random.Range(1, 5) + (Vector3.forward * Random.Range(1, 5))));
        }

        public VariantRewardInfo(float goldMultiplier, float experienceMultiplier, float whiteChance, float greenChance, float redChance)
        {
            this.goldMultiplier = goldMultiplier;
            this.experienceMultiplier = experienceMultiplier;
            this.whiteChance = whiteChance;
            this.greenChance = greenChance;
            this.redChance = redChance;
        }

        public VariantRewardInfo()
        {

        }
    }
}
