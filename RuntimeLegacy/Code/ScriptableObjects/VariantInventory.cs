
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using VAPI.Legacy.Components;

namespace VAPI.Legacy
{
    [Obsolete("VarianceAPI's 2.0 systems are obsolete, the capabilities of this ScriptableObject has been split between \"VariantInventoryDefintion\" and \"VariantBuffStorage\"")]
    public class VariantInventory : ScriptableObject
    {
        [Serializable]
        public class ItemPair
        {
            public AddressReferencedItemDef item = new AddressReferencedItemDef();
            public int amount;
        }
        [Serializable]
        public class BuffInfo
        {
            public AddressReferencedBuffDef buff = new AddressReferencedBuffDef();

            public float time;
            public int amount;
        }
        [Serializable]
        public class EquipmentInfo
        {
            public AddressReferencedEquipmentDef equipment = new AddressReferencedEquipmentDef();

            public bool usable;
            public float aiMaxUseHealthFraction;
            public float aiMaxUseDistance;
            public float aiUseDelayMax;
        }

        public ItemPair[] itemInventory = Array.Empty<ItemPair>();

        public BuffInfo[] buffInfos = Array.Empty<BuffInfo>();

        public EquipmentInfo equipmentInfo = new EquipmentInfo();
    }
}
