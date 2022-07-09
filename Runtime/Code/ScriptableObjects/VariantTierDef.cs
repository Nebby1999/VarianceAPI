using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantTierDef", menuName = "VarianceAPI/VariantTierDef")]
    public class VariantTierDef : ScriptableObject
    {
        [SerializeField]
        private VariantTierIndex _tier;
        public bool announcesArrival;
        public NetworkSoundEventDef soundEvent;

        [Space]
        public float experienceMultiplier;
        public float goldMultiplier;
        public float whiteItemDropChance;
        public float greenItemDropChance;
        public float redItemDropChance;
        
        public VariantTierIndex Tier { get => _tier; internal set => _tier = value; }
    }
}
