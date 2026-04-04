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

        public bool canDropCommon => !Mathf.Approximately(commonItemRewardChance, 0f);
        [Min(0)]
        public float commonItemRewardChance;

        public bool canDropUncommon => !Mathf.Approximately(uncommonItemRewardChance, 0f);
        [Min(0)]
        public float uncommonItemRewardChance;

        public bool canDropLegendary => !Mathf.Approximately(legendaryItemRewardChance, 0f);
        [Min(0)]
        public float legendaryItemRewardChance;

        public bool canDropBoss => !Mathf.Approximately(bossItemRewardChance, 0f);
        [Min(0)]
        public float bossItemRewardChance;
    }
}