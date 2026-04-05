#nullable enable
using HG;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using UnityEngine.Networking;

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
            public float timeBetweenEquipmentSwitches;
        }

        public AddressableItemCountPair[] itemsToGrant = Array.Empty<AddressableItemCountPair>();
        public AddressableEquipmentInfo equipmentInfo;

        public void ApplyToInventory(Inventory inventory)
        {
            if (!inventory || !NetworkServer.active)
                return;

            foreach (var itemToGrant in itemsToGrant)
            {
                if (itemToGrant.itemDef == null)
                    continue;

                ItemDef item = itemToGrant.itemDef.LoadAssetNow();
                if (!item)
                    continue;

                inventory.GiveItemChanneled(item.itemIndex, itemToGrant.count);
            }

            if(equipmentInfo.equipmentDef == null)
            {
                return;
            }

            EquipmentDef equipmentDef = equipmentInfo.equipmentDef.LoadAssetNow();
            if (!equipmentDef)
                return;

            var activeslot = inventory.activeEquipmentSlot;
            for(uint i = 0; i <= inventory.GetEquipmentSetCount(activeslot); i++)
            {
                var indexForSlot = inventory.GetEquipment(activeslot, i).equipmentIndex;
                if(indexForSlot == EquipmentIndex.None || indexForSlot == equipmentDef.equipmentIndex)
                {
                    inventory.SetEquipmentIndexForSlot(equipmentDef.equipmentIndex, activeslot, i);
                    break;
                }
            }

            if(equipmentInfo.canTriggerEquipment)
            {
                VariantEquipmentHandler handler = inventory.EnsureComponent<VariantEquipmentHandler>();
                handler.AddEquipmentData(equipmentDef.equipmentIndex, equipmentInfo.aiMaxUseHealthFraction, equipmentInfo.aiMaxUseDistance, equipmentInfo.timeBetweenEquipmentSwitches);
            }
        }

        internal void UnapplyToInventory(Inventory inventory)
        {
            if (!inventory || !NetworkServer.active)
                return;

            foreach(var itemToGrant in itemsToGrant)
            {
                if (itemToGrant.itemDef == null)
                    continue;

                ItemDef item = itemToGrant.itemDef.LoadAssetNow();
                if (!item)
                    continue;

                inventory.RemoveItemChanneled(item.itemIndex, itemToGrant.count);
            }

            if (equipmentInfo.equipmentDef == null)
            {
                return;
            }

            EquipmentDef equipmentDef = equipmentInfo.equipmentDef.LoadAssetNow();
            if (!equipmentDef)
                return;

            inventory.RemoveEquipment(equipmentDef.equipmentIndex);

            if (equipmentInfo.canTriggerEquipment)
            {
                VariantEquipmentHandler handler = inventory.EnsureComponent<VariantEquipmentHandler>();
                handler.RemoveEquipmentData(equipmentDef.equipmentIndex);
            }
        }
    }
}