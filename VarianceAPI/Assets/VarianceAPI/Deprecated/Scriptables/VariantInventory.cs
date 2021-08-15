using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantInventory", menuName = "VarianceAPI/VariantInventory", order = 4)]
    public class VariantInventory : ScriptableObject
    {
        [Header("Variant Inventory")]

            [Tooltip("The item to give to the Variant\nMUST be the same as the Item's ItemDef!")]
            public string[] itemStrings;
        
            [Tooltip("How many items to give")]
            [Min(0)]
            public int[] counts;
    }
}
