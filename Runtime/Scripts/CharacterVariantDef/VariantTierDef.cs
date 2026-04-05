#nullable enable
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantTierDef", menuName = "VarianceAPI/VariantTierDef")]
    public class VariantTierDef : ScriptableObject
    {
        public bool announceArrivalInChat;
        public string? soundEvent;

        public AddressableItemCountPair[] tierItems = Array.Empty<AddressableItemCountPair>();
        public AddressReferencedBuffDef tierBuffDef = new AddressReferencedBuffDef();

        [Header("Reward")]
        [Min(1)]
        public float experienceRewardCoefficient;
        [Min(1)]
        public float goldRewardCoefficient;

        public bool canDropCommon => commonItemRewardChance > 0;
        [Min(0)]
        public float commonItemRewardChance;

        public bool canDropUncommon => uncommonItemRewardChance > 0;
        [Min(0)]
        public float uncommonItemRewardChance;

        public bool canDropLegendary => legendaryItemRewardChance > 0;
        [Min(0)]
        public float legendaryItemRewardChance;

        public bool canDropBoss => bossItemRewardChance > 0;
        [Min(0)]
        public float bossItemRewardChance;
    }
}