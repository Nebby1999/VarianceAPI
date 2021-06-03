using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "CustomVariantRewards", menuName = "VarianceAPI/CustomVariantRewards", order = 1)]
    public class CustomVariantReward : ScriptableObject
    {
        public uint goldBonus;

        public float goldMultiplier;

        public uint xpBonus;

        public float xpMultiplier;
        
        /// <summary>
        /// The chance for the variant to drop a white item
        /// </summary>
        public float whiteItemChance;

        /// <summary>
        /// The chance for the variant to drop a green item
        /// </summary>
        public float greenItemChance;

        /// <summary>
        /// The Chance for the variant to drop a red item
        /// </summary>
        public float redItemChance;
    }
}
