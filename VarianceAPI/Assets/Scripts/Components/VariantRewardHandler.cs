using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
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
        private Transform thisTransform;

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
            this.thisTransform = base.GetComponent<Transform>();
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
            RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        public void Init()
        {
            GetStuff();
            VariantHandlers = base.GetComponents<VariantHandler>();
            CalculateRewards(VariantHandlers);
            CheckItemDropChance();
            ModifyRewards();
            RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }
        private void ModifyRewards()
        {
            deathRewards.expReward = (uint)((deathRewards.expReward + bonusXP) * xpMult);
            deathRewards.goldReward = (uint)((deathRewards.goldReward + bonusGold) * goldMult);
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
                        goldMult += 1.3f;
                        xpMult += 1.3f;
                        break;
                    case VariantTier.Uncommon:
                        goldMult += 1.6f;
                        xpMult += 1.6f;
                        break;
                    case VariantTier.Rare:
                        goldMult += 2.0f;
                        xpMult += 2.0f;
                        break;
                    case VariantTier.Legendary:
                        goldMult += 3.0f;
                        xpMult += 3.0f;
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
                    this.whiteChance = 3;
                    this.greenChance = 0;
                    this.redChance = 0;
                    break;
                case VariantTier.Uncommon:
                    this.whiteChance = 5;
                    this.greenChance = 1;
                    this.redChance = 0;
                    break;
                case VariantTier.Rare:
                    this.whiteChance = 10;
                    this.greenChance = 5;
                    this.redChance = 1;
                    break;
                case VariantTier.Legendary:
                    this.whiteChance = 25;
                    this.greenChance = 10;
                    this.redChance = 5;
                    break;
            }
        }
        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            if(obj.victimBody == this.characterBody)
            {
                var rng = UnityEngine.Random.Range(0f, 100f);
                if (rng < redChance)
                {
                    PickupDropletController.CreatePickupDroplet(redItems[nextRedItem], thisTransform.position, (Vector3.up * 20f) + (5 * Vector3.right * Mathf.Cos(Offset)) + (5 * Vector3.forward * Mathf.Sin(Offset)));
                }
                else if (rng < greenChance + redChance)
                {
                    PickupDropletController.CreatePickupDroplet(greenItems[nextGreenItem], thisTransform.position, (Vector3.up * 20f) + (5 * Vector3.right * Mathf.Cos(Offset)) + (5 * Vector3.forward * Mathf.Sin(Offset)));
                }
                else if (rng < whiteChance + greenChance + redChance)
                {
                    PickupDropletController.CreatePickupDroplet(whiteItems[nextWhiteItem], thisTransform.position, (Vector3.up * 20f) + (5 * Vector3.right * Mathf.Cos(Offset)) + (5 * Vector3.forward * Mathf.Sin(Offset)));
                }
            }
        }
    }
}
