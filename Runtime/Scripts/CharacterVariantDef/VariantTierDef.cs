#nullable enable
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using VAPI.AddressableAssets;

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
        public float expMultiplier;
        public float goldMultiplier;

        public TierRewardInfo[] rewardInfo = Array.Empty<TierRewardInfo>();
    }

    [Serializable]
    public struct TierRewardInfo
    {
        public AddressReferencedItemTierDef? itemTier;
        public float chance;
    }
}