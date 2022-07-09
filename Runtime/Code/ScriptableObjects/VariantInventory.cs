using Moonstorm.AddressableAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantInventory", menuName = "VarianceAPI/VariantInventory")]
    public class VariantInventory : ScriptableObject
    {
        public ItemPair[] itemInventory = Array.Empty<ItemPair>();

        public BuffInfo[] buffInfos = Array.Empty<BuffInfo>();

        public AddressableEquipmentDef euipmentDef;

        [Serializable]
        public class ItemPair
        {
            public AddressableItemDef itemDef;
            public int amount;
        }

        [Serializable]
        public class BuffInfo
        {
            public AddressableBuffDef buffDef;
            public float time;
            public int amount;
        }
    }
}
