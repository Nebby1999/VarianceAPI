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

        [Space]
        public float experienceMultiplier;
        public float goldMultiplier;
        public float whiteItemDropChance;
        public float greenItemDropChance;
        public float redItemDropChance;
        
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

        public virtual VariantRewardInfo CreateVariantRewardInfo(Run runInstance)
        {
            var vri = new VariantRewardInfo(goldMultiplier, experienceMultiplier, whiteItemDropChance, greenItemDropChance, redItemDropChance);
            vri.SetIndicesAndNextItems(runInstance);
            return vri;
        }
    }
}
