
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI.Legacy
{
    /// <summary>
    /// A ScriptableObject used to represent a variant's Tier
    /// </summary>
    public class VariantTierDef : ScriptableObject
    {
        [SerializeField]
        private VariantTierIndex _tier;
        public bool announcesArrival;
        public NetworkSoundEventDef soundEvent;

        public List<AddressReferencedItemDef> tierItemDefs = new List<AddressReferencedItemDef>();

        public AddressReferencedBuffDef tierBuffDef;

        [Space]
        public float experienceMultiplier;
        public float goldMultiplier;
        public float whiteItemDropChance;
        public float greenItemDropChance;
        public float redItemDropChance;
        public float armorBonus;
        public VariantTierIndex tier { get => _tier; internal set => _tier = value; }
    }
}
