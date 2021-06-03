using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "ItemInfo", menuName = "VarianceAPI/ItemInfo", order = 3)]
    public class ItemInfo : ScriptableObject
    {
        /// <summary>
        /// The internal name of the item, Refer to the item's ITEMDEF.
        /// </summary>
        public string itemString;
        /// <summary>
        /// The amount of copies the variant has
        /// </summary>
        public int count;
    }
}
