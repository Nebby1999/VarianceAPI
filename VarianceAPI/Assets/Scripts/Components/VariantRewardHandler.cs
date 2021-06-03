using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VarianceAPI.Modules;
using VarianceAPI.Scriptables;

namespace VarianceAPI.Components
{
    public class VariantRewardHandler : NetworkBehaviour
    {
        static readonly float Offset = 2f * Mathf.PI / Run.instance.participatingPlayerCount;
        
        public uint bonusGold;
        public float goldMult;

        public uint bonusXP;
        public float xpMult;

        public float whiteChance;
        public float greenChance;
        public float redChance;

        private DeathRewards deathRewards;
        private CharacterBody characterBody;

        private VariantHandler[] VariantHandlers;
        
        private readonly List<PickupIndex> redItems = Run.instance.availableTier3DropList;
        private readonly List<PickupIndex> greenItems = Run.instance.availableTier2DropList;
        private readonly List<PickupIndex> whiteItems = Run.instance.availableTier1DropList;
        
        private int nextRedItem;
        private int nextGreenItem;
        private int nextWhiteItem;

        private VariantTier highestTier;
        private void GetStuff()
        {
            this.deathRewards = base.GetComponent<DeathRewards>();
            this.characterBody = base.GetComponent<CharacterBody>();
            this.nextRedItem = Run.instance.treasureRng.RangeInt(0, redItems.Count);
            this.nextGreenItem = Run.instance.treasureRng.RangeInt(0, greenItems.Count);
            this.nextWhiteItem = Run.instance.treasureRng.RangeInt(0, redItems.Count);
        }
        public void InitCustomRewards(CustomVariantReward customVariantReward)
        {
            GetStuff();
            this.bonusGold = customVariantReward.goldBonus;
            this.goldMult = customVariantReward.goldMultiplier;

            this.bonusXP = customVariantReward.xpBonus;
            this.xpMult = customVariantReward.xpMultiplier;

            this.whiteChance = customVariantReward.whiteItemChance;
            this.greenChance = customVariantReward.greenItemChance;
            this.redChance = customVariantReward.redItemChance;

            ModifyRewards();
            RoR2.GlobalEventManager.onCharacterDeathGlobal += SpawnDroplet;
        }

        public void Init()
        {
            GetStuff();
            VariantHandlers = base.GetComponents<VariantHandler>();
            CalculateRewards(VariantHandlers);
            CheckItemDropChance();
            ModifyRewards();
            RoR2.GlobalEventManager.onCharacterDeathGlobal += SpawnDroplet;
        }
        private void ModifyRewards()
        {
            if(ConfigLoader.EnableGoldRewards.Value)
            {
                deathRewards.expReward = (uint)((deathRewards.expReward + bonusXP) * xpMult);
            }
            if(ConfigLoader.EnableXPRewards.Value)
            {
                deathRewards.goldReward = (uint)((deathRewards.goldReward + bonusGold) * goldMult);
            }
        }
        private void CalculateRewards(VariantHandler[] variantHandlers)
        {
            foreach (VariantHandler variant in variantHandlers)
            {
                if(!variant.isVariant)
                {
                    continue;
                }
                switch(variant.tier)
                {
                    case VariantTier.Common:
                        goldMult += ConfigLoader.CommonVariantGoldMultiplier.Value;
                        xpMult += ConfigLoader.CommonVariantXPMultiplier.Value;
                        break;
                    case VariantTier.Uncommon:
                        goldMult += ConfigLoader.UncommonVariantGoldMultiplier.Value;
                        xpMult += ConfigLoader.UncommonVariantXPMultiplier.Value;
                        break;
                    case VariantTier.Rare:
                        goldMult += ConfigLoader.RareVariantGoldMultiplier.Value;
                        xpMult += ConfigLoader.RareVariantXPMultiplier.Value;
                        break;
                    case VariantTier.Legendary:
                        goldMult += ConfigLoader.LegendaryVariantGoldMultiplier.Value;
                        xpMult += ConfigLoader.LegendaryVariantXPMultiplier.Value;
                        break;
                }
                if(variant.tier > highestTier)
                {
                    highestTier = variant.tier;
                }
            }
        }
        private void CheckItemDropChance()
        {
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
        }
        private void SpawnDroplet(DamageReport obj)
        {
            if(ConfigLoader.EnableItemRewards.Value)
            {
                if(Run.instance.isRunStopwatchPaused && ConfigLoader.HiddenRealmItemdropBehaviorConfig.Value != "Unchanged")
                {
                    if(ConfigLoader.HiddenRealmItemdropBehaviorConfig.Value == "Halved")
                    {
                        int rng = UnityEngine.Random.Range(1, 20);
                        if (rng > 10)
                        {
                            TrySpawnDroplet(obj);
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                    TrySpawnDroplet(obj);
                }
            }
        }
        private void TrySpawnDroplet(DamageReport damageReport)
        {
            if (damageReport.victimBody == this.characterBody)
            {
                if (damageReport.victimTeamIndex != TeamIndex.Player)
                {
                    var rng = UnityEngine.Random.Range(0f, 100f);
                    if (rng < redChance)
                    {
                        CreateDroplet(redItems, nextRedItem, damageReport.victimBody, damageReport.attackerBody);
                    }
                    else if (rng < greenChance + redChance)
                    {
                        CreateDroplet(greenItems, nextGreenItem, damageReport.victimBody, damageReport.attackerBody);
                    }
                    else if (rng < whiteChance + greenChance + redChance)
                    {
                        CreateDroplet(whiteItems, nextWhiteItem, damageReport.victimBody, damageReport.attackerBody);
                    }
                }
            }
        }
        private static void CreateDroplet(List<PickupIndex> itemList, int nextItem, CharacterBody variantBody, CharacterBody playerBody)
        {
            if(ConfigLoader.ItemRewardsSpawnsOnPlayer.Value)
            {
                PickupDropletController.CreatePickupDroplet(itemList[nextItem], playerBody.transform.position, (Vector3.up * 20f) + (5 * Vector3.right * Mathf.Cos(Offset)) + (5 * Vector3.forward * Mathf.Sin(Offset)));
            }
            else
            {
                PickupDropletController.CreatePickupDroplet(itemList[nextItem], variantBody.transform.position, (Vector3.up * 20f) + (5 * Vector3.right * Mathf.Cos(Offset)) + (5 * Vector3.forward * Mathf.Sin(Offset)));
            }
        }
    }
}
