using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "CustomVariantReward", menuName = "VarianceAPI/CustomVariantReward")]
    public class CustomVariantReward : ScriptableObject
    {
        [Header("Death Rewards")]
            [Tooltip("How much extra gold the Variant drops when it's killed, Bonus can be negative")]
            public uint goldBonus;
            
            [Tooltip("Multiplier that's applied to the Variant's gold reward when it's Killed")]
            [Min(0)]
            public float goldMultiplier;

            [Tooltip("How much extra gold the Variant drops when it's killed, Bonus can be negative")]
            public uint xpBonus;

            [Tooltip("Multiplier that's applied to the Variant's XP reward when it's Killed")]
            [Min(0)]
            public float xpMultiplier;
        
        [Header("Item Drop Chances")]
            [Tooltip("The Chance for the Variant to drop a White Item")]
            [Range(0, 100)]
            public float whiteItemChance;

            [Tooltip("The Chance for the Variant to drop a Green Item")]
            [Range(0, 100)]
            public float greenItemChance;
        
            [Tooltip("The Chance for the Variant to drop a Red Item")]
            [Range(0, 100)]
            public float redItemChance;
        [Header("Item List")]
            [Tooltip("The list of items the variant might drop on a succesful roll.\nAutomatically sorted into their respective tiers.")]
            public string[] ItemList;
    }
}
