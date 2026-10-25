
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI.Legacy
{
    [Obsolete("VarianceAPI's 2.0 systems are obsolete, there's a menu item under \"Tools/VAPI\" that's used to migrate VariantTierDef into CharacterVariantTierDef")]
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
