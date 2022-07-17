using Moonstorm.AddressableAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantTierDef", menuName = "VarianceAPI/VariantTierDef")]
    public class VariantTierDef : ScriptableObject
    {
        [SerializeField]
        private VariantTierIndex _tier;
        public bool announcesArrival;
        public NetworkSoundEventDef soundEvent;
        public List<AddressableItemDef> tierItems = new List<AddressableItemDef>();
        public AddressableBuffDef tierBuff;

        [Space]
        public float experienceMultiplier;
        public float goldMultiplier;
        public float whiteItemDropChance;
        public float greenItemDropChance;
        public float redItemDropChance;

        public float ExperienceMultiplierMinus1
        {
            get
            {
                if (experienceMultiplier < 1)
                    return experienceMultiplier;
                return experienceMultiplier - 1;
            }
        }

        public float GoldMultiplierMinus1
        {
            get
            {
                if (goldMultiplier < 1)
                    return goldMultiplier;
                return goldMultiplier - 1;
            }
        }
        
        public VariantTierIndex Tier { get => _tier; internal set => _tier = value; }

        public virtual void AddTierItems(Inventory targetInventory)
        {
            if (!NetworkServer.active)
                return;

            foreach(AddressableItemDef itemDef in tierItems)
            {
                if (itemDef.Asset)
                    targetInventory.GiveItem(itemDef.Asset);
            }
        }

        public virtual void AddTierBuff(CharacterBody targetBody)
        {
            if (!NetworkServer.active)
                return;

            targetBody.AddBuff(tierBuff.Asset);
        }

        public virtual VariantRewardInfo CreateVariantRewardInfo(Run runInstance)
        {
            var vri = new VariantRewardInfo(goldMultiplier, experienceMultiplier, whiteItemDropChance, greenItemDropChance, redItemDropChance);
            vri.SetIndicesAndNextItems(runInstance);
            return vri;
        }
    }
}
