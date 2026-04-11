#nullable enable
using HG;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace VAPI
{
    [Serializable]
    public struct AddressableItemCountPair : ICloneable
    {
        public AddressReferencedItemDef? itemDef;
        public int count;

        public bool TryGetItemCountPair(out ItemCountPair asItemCountPair)
        {
            asItemCountPair = default;
            if(itemDef == null)
            {
                return false;
            }

            ItemDef item = itemDef.LoadAssetNow();
            if (!item)
                return false;

            asItemCountPair = new ItemCountPair { itemDef = item, count = count };
            return true;
        }

        public object Clone()
        {
            return new AddressableItemCountPair
            {
                count = count,
                itemDef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedItemDef, ItemDef>(itemDef)
            };
        }
    }

    [Serializable]
    public sealed class VariantInventoryDefinition : ICloneable
    {
        private struct DisposableVariantInventoryDefintionModification : IDisposable
        {
            private Inventory _modifiedInventory;
            private ItemCountPair[]? _channeledItemCountPairs;
            private EquipmentDef? _addedEquipment;

            public DisposableVariantInventoryDefintionModification(Inventory modifiedInventory, ItemCountPair[]? channeledItemCountPairs, EquipmentDef? addedEquipment)
            {
                _modifiedInventory = modifiedInventory;
                _channeledItemCountPairs = channeledItemCountPairs;
                _addedEquipment = addedEquipment;
            }

            public void Dispose()
            {
                if (!_modifiedInventory)
                    return;

                if(_channeledItemCountPairs != null)
                {
                    for(int i = 0; i < _channeledItemCountPairs.Length; i++)
                    {
                        _modifiedInventory.RemoveItemChanneled(_channeledItemCountPairs[i].itemDef.itemIndex, _channeledItemCountPairs[i].count);
                    }
                }

                if (_addedEquipment)
                {
                    _modifiedInventory.RemoveEquipment(_addedEquipment!.equipmentIndex);
                    _modifiedInventory.EnsureComponent<VariantEquipmentHandler>().RemoveEquipmentData(_addedEquipment.equipmentIndex);
                }
            }
        }
        [Serializable]
        public struct AddressableEquipmentInfo : ICloneable
        {
            public AddressReferencedEquipmentDef? equipmentDef;

            public bool canTriggerEquipment;
            public float aiMaxUseHealthFraction;
            public float aiMaxUseDistance;
            public float timeBetweenEquipmentSwitches;

            public object Clone()
            {
                return new AddressableEquipmentInfo
                {
                    aiMaxUseDistance = aiMaxUseDistance,
                    aiMaxUseHealthFraction = aiMaxUseHealthFraction,
                    canTriggerEquipment = canTriggerEquipment,
                    timeBetweenEquipmentSwitches = timeBetweenEquipmentSwitches,
                    equipmentDef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedEquipmentDef, EquipmentDef>(equipmentDef)
                };
            }
        }

        public AddressableItemCountPair[] itemsToGrant = Array.Empty<AddressableItemCountPair>();
        public AddressableEquipmentInfo equipmentInfo;

        private static List<ItemCountPair> _itemCountPairBuilder = new List<ItemCountPair>();
        public IDisposable? ApplyToInventory(Inventory targetInventory)
        {
            if (!targetInventory || !NetworkServer.active)
                return null;

            _itemCountPairBuilder.Clear();
            for(int i = 0; i < itemsToGrant.Length; i++)
            {
                if (!itemsToGrant[i].TryGetItemCountPair(out var itemCountPair))
                {
                    continue;
                }
                _itemCountPairBuilder.Add(itemCountPair);

                targetInventory.GiveItemChanneled(itemCountPair.itemDef.itemIndex, itemCountPair.count);
            }

            EquipmentDef? equipmentDef = null;
            if(equipmentInfo.equipmentDef != null)
            {
                equipmentDef = equipmentInfo.equipmentDef.LoadAssetNow();
                if(equipmentDef)
                {
                    var activeslot = targetInventory.activeEquipmentSlot;
                    for (uint i = 0; i <= targetInventory.GetEquipmentSetCount(activeslot); i++)
                    {
                        var indexForSlot = targetInventory.GetEquipment(activeslot, i).equipmentIndex;
                        if (indexForSlot == EquipmentIndex.None || indexForSlot == equipmentDef.equipmentIndex)
                        {
                            targetInventory.SetEquipmentIndexForSlot(equipmentDef.equipmentIndex, activeslot, i);
                            break;
                        }
                    }

                    if (equipmentInfo.canTriggerEquipment)
                    {
                        VariantEquipmentHandler handler = targetInventory.EnsureComponent<VariantEquipmentHandler>();
                        handler.AddEquipmentData(equipmentDef.equipmentIndex, equipmentInfo.aiMaxUseHealthFraction, equipmentInfo.aiMaxUseDistance, equipmentInfo.timeBetweenEquipmentSwitches);
                    }
                }
            }

            return new DisposableVariantInventoryDefintionModification(targetInventory, _itemCountPairBuilder.ToArray(), equipmentDef);
        }

        public object Clone()
        {
            VariantInventoryDefinition result = new VariantInventoryDefinition();
            HG.ArrayUtils.EnsureCapacity(ref result.itemsToGrant, itemsToGrant.Length);
            for(int i = 0; i < result.itemsToGrant.Length; i++)
            {
                result.itemsToGrant[i] = (AddressableItemCountPair)itemsToGrant[i].Clone();
            }
            result.equipmentInfo = (AddressableEquipmentInfo)equipmentInfo.Clone();
            return result;
        }
    }
}