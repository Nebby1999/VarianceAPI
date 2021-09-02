using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.ScriptableObjects
{
    [CreateAssetMenu(fileName = "VariantInventoryInfo", menuName = "VarianceAPI/VariantInventoryInfo")]
    public class VariantInventoryInfo : ScriptableObject
    {
        [Serializable]
        public struct VariantInventory
        {
            [Tooltip("The Item to give to the variant\nMust match the ItemDef's name.")]
            public string itemDefName;

            [Tooltip("The amount of items to give.")]
            public int amount;
        }
        
        [Serializable]
        public struct VariantBuff
        {
            [Tooltip("The buff to give to the variant\nMust match the BuffDef's name.")]
            public string buffDefName;

            [Tooltip("How long this buff lasts, Set this to 0 for it to be permanent.")]
            public float time;

            [Tooltip("The amount of stacks to give.")]
            public int amount;
        }
        public VariantInventory[] ItemInventory = Array.Empty<VariantInventory>();

        public VariantBuff[] Buffs = Array.Empty<VariantBuff>();

        [Header("Variant Equipment")]

        [Tooltip("The equipment to give to the variant\nMust match the EquipmentDef's name.")]
        public string equipmentDefName;

        [Tooltip("The logic behind how the variant uses the equipment.\nY: The chance for the variant to use the equipment, where 0 is 0% and 1 is 100%.\nX: The amount of missing health the variant needs before it uses the equipment, where 0 is 0% missing health and 1 is 100% missing health.")]
        public AnimationCurve fireCurve;
    }
}
