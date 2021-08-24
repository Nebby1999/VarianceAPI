using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VarianceAPI.ScriptableObjects;

namespace VarianceAPI.Components
{
    public class VariantRewardHandler : MonoBehaviour
    {
        static readonly float Offset = 2f * Mathf.PI / Run.instance.participatingPlayerCount;

        public VariantInfo[] VariantInfos;
        private VariantTier highestTier;

        private DeathRewards deathRewards;
        private CharacterBody characterBody;

        public float goldMult;
        public float xpMult;

        public float whiteChance;
        public float greenChance;
        public float redChance;

        private List<PickupIndex> redItems = Run.instance.availableTier3DropList;
        private List<PickupIndex> greenItems = Run.instance.availableTier2DropList;
        private List<PickupIndex> whiteItems = Run.instance.availableTier1DropList;

        private int nextRedItem;
        private int nextGreenItem;
        private int nextWhiteItem;

        public void Start()
        {
            deathRewards = gameObject.GetComponent<DeathRewards>();
            characterBody = gameObject.GetComponent<CharacterBody>();

            nextWhiteItem = Run.instance.treasureRng.RangeInt(0, whiteItems.Count);
            nextGreenItem = Run.instance.treasureRng.RangeInt(0, greenItems.Count);
            nextRedItem = Run.instance.treasureRng.RangeInt(0, redItems.Count);

            if(ConfigLoader.EnableItemRewards.Value)
            {
                GlobalEventManager.onCharacterDeathGlobal += SpawnDroplet;
            }
        }

        public void Modify()
        {
            #region Gold and XP Multiplier
            foreach (VariantInfo variantInfo in VariantInfos)
            {
                switch (variantInfo.variantTier)
                {
                    case VariantTier.Common:
                        goldMult += ConfigLoader.CommonVariantGoldMultiplier.Value - 1;
                        xpMult += ConfigLoader.CommonVariantXPMultiplier.Value - 1;
                        break;
                    case VariantTier.Uncommon:
                        goldMult += ConfigLoader.UncommonVariantGoldMultiplier.Value - 1;
                        xpMult += ConfigLoader.UncommonVariantXPMultiplier.Value - 1;
                        break;
                    case VariantTier.Rare:
                        goldMult += ConfigLoader.RareVariantGoldMultiplier.Value - 1;
                        xpMult += ConfigLoader.RareVariantXPMultiplier.Value - 1;
                        break;
                    case VariantTier.Legendary:
                        goldMult += ConfigLoader.LegendaryVariantGoldMultiplier.Value - 1;
                        xpMult += ConfigLoader.LegendaryVariantXPMultiplier.Value - 1;
                        break;
                }
                if (variantInfo.variantTier > highestTier)
                {
                    highestTier = variantInfo.variantTier;
                }
            }
            #endregion

            #region Item Chances
            switch (highestTier)
            {
                case VariantTier.Common:
                    this.whiteChance = ConfigLoader.CommonVariantWhiteItemDropChance.Value;
                    this.greenChance = ConfigLoader.CommonVariantGreenItemDropChance.Value;
                    this.redChance = ConfigLoader.CommonVariantRedItemDropChance.Value;
                    break;
                case VariantTier.Uncommon:
                    this.whiteChance = ConfigLoader.UncommonVariantWhiteItemDropChance.Value;
                    this.greenChance = ConfigLoader.UncommonVariantGreenItemDropChance.Value;
                    this.redChance = ConfigLoader.UncommonVariantRedItemDropChance.Value;
                    break;
                case VariantTier.Rare:
                    this.whiteChance = ConfigLoader.RareVariantWhiteItemDropChance.Value;
                    this.greenChance = ConfigLoader.RareVariantGreenItemDropChance.Value;
                    this.redChance = ConfigLoader.RareVariantRedItemDropChance.Value;
                    break;
                case VariantTier.Legendary:
                    this.whiteChance = ConfigLoader.LegendaryVariantWhiteItemDropChance.Value;
                    this.greenChance = ConfigLoader.LegendaryVariantGreenItemDropChance.Value;
                    this.redChance = ConfigLoader.LegendaryVariantRedItemDropChance.Value;
                    break;
            }
            #endregion
        }

        private void SpawnDroplet(DamageReport damageReport)
        {
            if (Run.instance.isRunStopwatchPaused && ConfigLoader.HiddenRealmItemdropBehaviorConfig.Value != "Unchanged")
            {
                if (ConfigLoader.HiddenRealmItemdropBehaviorConfig.Value == "Halved")
                {
                    int rng = UnityEngine.Random.Range(1, 20);
                    if (rng > 10)
                    {
                        TrySpawnDroplet(damageReport);
                    }
                }
                else
                {
                }
            }
            else
            {
                TrySpawnDroplet(damageReport);
            }
        }

        private void TrySpawnDroplet(DamageReport damageReport)
        {
            if(damageReport.victimBody == characterBody)
            {
                if (damageReport.victimTeamIndex != TeamIndex.Player)
                {
                    if (Util.CheckRoll(redChance))
                    {
                        CreateDroplet(redItems, nextRedItem, damageReport);
                    }
                    else if (Util.CheckRoll(redChance + greenChance))
                    {
                        CreateDroplet(greenItems, nextGreenItem, damageReport);
                    }
                    else if (Util.CheckRoll(redChance + greenChance + whiteChance))
                    {
                        CreateDroplet(whiteItems, nextWhiteItem, damageReport);
                    }
                }
            }
        }
        private static void CreateDroplet(List<PickupIndex> itemList, int nextItem, DamageReport damageReport)
        {
            if (ConfigLoader.ItemRewardsSpawnsOnPlayer.Value)
            {
                PickupDropletController.CreatePickupDroplet(itemList[nextItem], damageReport.attackerBody.transform.position, (Vector3.up * 20f) + (5 * Vector3.right * Mathf.Cos(Offset)) + (5 * Vector3.forward * Mathf.Sin(Offset)));
            }
            else
            {
                PickupDropletController.CreatePickupDroplet(itemList[nextItem], damageReport.victimBody.transform.position, (Vector3.up * 20f) + (5 * Vector3.right * Mathf.Cos(Offset)) + (5 * Vector3.forward * Mathf.Sin(Offset)));
            }
        }
    }
}