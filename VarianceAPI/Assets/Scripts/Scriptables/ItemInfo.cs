using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "ItemInfo", menuName = "VarianceAPI/ItemInfo", order = 4)]
    public class ItemInfo : ScriptableObject
    {
        [Header("Item Info")]
        
            [Tooltip("The item to give to the Variant\nMUST be the same as the Item's ItemDef!")]
            public string itemString;

            [Tooltip("How many items to give")]
            [Min(0)]
            public int count;
    }
}
