#nullable enable
using R2API.AddressReferencedAssets;
using RoR2;
using System;

namespace VAPI
{
    [Serializable]
    public struct AddressableItemCountPair
    {
        public AddressReferencedItemDef? itemDef;
        public int count;
    }
    [Serializable]
    public sealed class VariantInventoryDefinition
    {

        [Serializable]
        public struct AddressableEquipmentInfo
        {
            public AddressReferencedEquipmentDef? equipmentDef;

            public bool canTriggerEquipment;
            public float aiMaxUseHealthFraction;
            public float aiMaxUseDistance;
        }

        public AddressableItemCountPair[] itemsToGrant = Array.Empty<AddressableItemCountPair>();
        public AddressableEquipmentInfo equipmentInfo;
    }
}