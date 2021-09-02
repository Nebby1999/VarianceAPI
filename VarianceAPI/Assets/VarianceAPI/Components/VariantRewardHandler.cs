using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VarianceAPI.ScriptableObjects;

namespace VarianceAPI.Components
{
    public class VariantRewardHandler : NetworkBehaviour
    {
        public VariantInfo[] VariantInfos;

        private VariantTier highestTier;

        private DeathRewards deathRewards;
        private CharacterBody characterBody;

        public float goldMult = 1;
        public float xpMult = 1;

        public float whiteChance;
        public float greenChance;
        public float redChance;

        private List<PickupIndex> redItems;
        private List<PickupIndex> greenItems;
        private List<PickupIndex> whiteItems;

        private int nextRedItem;
        private int nextGreenItem;
        private int nextWhiteItem;

        public void GetStuff()
        {
            Debug.Log("a");
            deathRewards = base.GetComponent<DeathRewards>();
            Debug.Log("a");
            characterBody = base.GetComponent<CharacterBody>();
            Debug.Log("a");
            redItems = Run.instance.availableTier3DropList;
            Debug.Log("a");
            greenItems = Run.instance.availableTier2DropList;
            Debug.Log("a");
            whiteItems = Run.instance.availableTier1DropList;
            Debug.Log("a");
            if(Run.instance)
            {
                Debug.Log("instance");
                nextWhiteItem = Run.instance.treasureRng.RangeInt(0, whiteItems.Count);
                Debug.Log("a");
                nextGreenItem = Run.instance.treasureRng.RangeInt(0, greenItems.Count);
                Debug.Log("a");
                nextRedItem = Run.instance.treasureRng.RangeInt(0, redItems.Count);
            }
            Debug.Log("a");
            if (ConfigLoader.EnableItemRewards.Value)
            {
                Debug.Log("a");
                GlobalEventManager.onCharacterDeathGlobal += SpawnDroplet;
            }
        }

        public void Modify()
        {
            if (!NetworkServer.active)
                return;

            GetStuff();
            if(VariantInfos.Length == 0)
            {
                Destroy(this);
                return;
            }
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

            #region Gold and XP Rewards
            deathRewards.goldReward *= (uint)goldMult;
            deathRewards.expReward *= (uint)xpMult;
            #endregion
        }

        private void SpawnDroplet(DamageReport damageReport)
        {
            if(damageReport.victimBody.Equals(characterBody))
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
                }
                else
                {
                    TrySpawnDroplet(damageReport);
                }
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
        private void CreateDroplet(List<PickupIndex> itemList, int nextItem, DamageReport damageReport)
        {
            if (ConfigLoader.ItemRewardsSpawnsOnPlayer.Value)
            {
                PickupDropletController.CreatePickupDroplet(itemList[nextItem], damageReport.attackerBody.transform.position, (Vector3.up * 20) + (Vector3.right * Random.Range(1,5) + (Vector3.forward * Random.Range(1,5))));
            }
            else
            {
                PickupDropletController.CreatePickupDroplet(itemList[nextItem], damageReport.victimBody.transform.position, (Vector3.up * 20) + (Vector3.right * Random.Range(1, 5) + (Vector3.forward * Random.Range(1, 5))));
            }
        }
        private void OnDestroy()
        {
            if (ConfigLoader.EnableItemRewards.Value)
            {
                GlobalEventManager.onCharacterDeathGlobal -= SpawnDroplet;
            }
        }
    }
}