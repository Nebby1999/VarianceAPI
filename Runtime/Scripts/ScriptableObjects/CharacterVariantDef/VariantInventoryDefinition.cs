#nullable enable
using HG;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// A Version of <see cref="RoR2.ItemCountPair"/>, but using a <see cref="AddressReferencedItemDef"/> to store the item def. Supports cloning
    /// </summary>
    [Serializable]
    public struct AddressableItemCountPair : ICloneable
    {
        [Tooltip("The item")]
        public AddressReferencedItemDef? itemDef;
        [Min(0)]
        [Tooltip("the amount")]
        public int count;

        private ItemCountPair? _itemCountPair;
        private bool _itemDefFailedToLoad;

        /// <summary>
        /// Tries to retrieve the equivalent <see cref="ItemCountPair"/> for this.
        /// <br></br>
        /// This method returns false if <see cref="itemDef"/> is null or if it resolves to an invalid Item.
        /// </summary>
        public bool TryGetItemCountPair(out ItemCountPair asItemCountPair)
        {
            if (_itemCountPair.HasValue)
            {
                asItemCountPair = _itemCountPair.Value;
                return true;
            }


            asItemCountPair = default;
            if (_itemDefFailedToLoad)
            {
                return false;
            }

            if (itemDef == null)
            {
                _itemDefFailedToLoad = true;
                return false;
            }

            ItemDef item = itemDef.LoadAssetNow();
            if (!item)
            {
                _itemDefFailedToLoad = true;
                return false;
            }

            _itemCountPair = new ItemCountPair { itemDef = item, count = count };
            asItemCountPair = _itemCountPair.Value;
            return true;
        }

        /// <summary>
        /// Clones the metadata in this into a new instance of <see cref="AddressableItemCountPair"/>
        /// </summary>
        public object Clone()
        {
            return new AddressableItemCountPair
            {
                count = count,
                itemDef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedItemDef, ItemDef>(itemDef)
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="AddressableItemCountPair"/> with the specified <paramref name="itemName"/> and <paramref name="count"/>
        /// </summary>
        public static AddressableItemCountPair Create(string itemName, int count = 1)
        {
            return new AddressableItemCountPair { itemDef = itemName, count = count };
        }

        /// <summary>
        /// Creates a new instance of <see cref="AddressableItemCountPair"/> with the specified <paramref name="item"/> and <paramref name="count"/>
        /// </summary>
        public static AddressableItemCountPair Create(ItemDef item, int count = 1)
        {
            return new AddressableItemCountPair { itemDef = item, count = count };
        }
    }

    /// <summary>
    /// A Class that represents an inventory for a variant. Can be cloned.
    /// <para></para>
    /// Items granted this way are Channeled. Given to the master when the variant is applied and being revoked when unapplied.
    /// </summary>
    [Serializable]
    public sealed class VariantInventoryDefinition : ICloneable
    {
        private struct DisposableVariantInventoryDefintionModification : IDisposable
        {
            private Inventory _modifiedInventory;
            private ItemCountPair[]? _channeledItemCountPairs;
            private EquipmentDef? _addedEquipment;

            private EquipmentIndex? _originalIndex;

            public DisposableVariantInventoryDefintionModification(Inventory modifiedInventory, ItemCountPair[]? channeledItemCountPairs, EquipmentDef? addedEquipment, EquipmentIndex? originalIndex)
            {
                _modifiedInventory = modifiedInventory;
                _channeledItemCountPairs = channeledItemCountPairs;
                _addedEquipment = addedEquipment;
                _originalIndex = originalIndex;
            }

            public void Dispose()
            {
                if (!_modifiedInventory)
                    return;

                _modifiedInventory.InventoryChangedHandler(DisposeInventoryModifications);
            }

            private void DisposeInventoryModifications(Inventory _modifiedInventory)
            {
                if (_channeledItemCountPairs != null)
                {
                    for (int i = 0; i < _channeledItemCountPairs.Length; i++)
                    {
                        _modifiedInventory.RemoveItemChanneled(_channeledItemCountPairs[i].itemDef.itemIndex, _channeledItemCountPairs[i].count);
                    }
                }

                if (_addedEquipment)
                {
                    if(_originalIndex.HasValue)
                    {
                        _modifiedInventory.SetEquipmentIndex(_originalIndex.Value, new EquipmentLocation { set = 0, slot = 0 });
                    }
                    else
                    {
                        _modifiedInventory.RemoveEquipment(_addedEquipment!.equipmentIndex);
                    }
                    _modifiedInventory.EnsureComponent<VariantEquipmentHandler>().RemoveEquipmentData(_addedEquipment!.equipmentIndex);
                }
            }
        }
        /// <summary>
        /// Represents information about an equipment for a variant.
        /// </summary>
        [Serializable]
        public struct AddressableEquipmentInfo : ICloneable
        {
            [Tooltip("the equipment to grant")]
            public AddressReferencedEquipmentDef? equipmentDef;

            [Tooltip("Overrides the existing equipment, this will replace elite equipments if set to true.")]
            public bool overrideEquipment;

            [Tooltip("If true, the variant can trigger the equipment")]
            public bool canTriggerEquipment;
            [Tooltip("The maximum amount of health required for the variant to trigger the equipment")]
            public float aiMaxUseHealthFraction;
            [Tooltip("The maximum distance required for the variant to trigger the equipment")]
            public float aiMaxUseDistance;

            [Obsolete("The ability to add new equipment sets with variants is not supported, this float is not used")]
            [HideInInspector]
            public float timeBetweenEquipmentSwitches;


            /// <summary>
            /// Clones the metadata in this into a new instance of <see cref="AddressableEquipmentInfo"/>
            /// </summary>
            public object Clone()
            {
                return new AddressableEquipmentInfo
                {
                    aiMaxUseDistance = aiMaxUseDistance,
                    aiMaxUseHealthFraction = aiMaxUseHealthFraction,
                    canTriggerEquipment = canTriggerEquipment,
                    equipmentDef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedEquipmentDef, EquipmentDef>(equipmentDef)
                };
            }

            /// <summary>
            /// Creates a new instance of <see cref="AddressableEquipmentInfo"/> with the specified <paramref name="equipmentName"/>
            /// </summary>
            public static AddressableEquipmentInfo Create(string equipmentName) => new AddressableEquipmentInfo { equipmentDef = equipmentName };
            /// <summary>
            /// Creates a new instance of <see cref="AddressableEquipmentInfo"/> with the specified <paramref name="equipment"/>
            /// </summary>
            public static AddressableEquipmentInfo Create(EquipmentDef equipment) => new AddressableEquipmentInfo { equipmentDef = equipment };
        }

        [Tooltip("An array of items to grant, these items are granted as channeled items and will exists as long as the variant's applied.")]
        public AddressableItemCountPair[] itemsToGrant = Array.Empty<AddressableItemCountPair>();
        [Tooltip("An equipment to grant to the variant, this equipment is removed if the variant is unapplied")]
        public AddressableEquipmentInfo equipmentInfo;

        private static List<ItemCountPair> _itemCountPairBuilder = new List<ItemCountPair>();

        /// <summary>
        /// Applies the metadata in this definition to <paramref name="targetInventory"/>
        /// <para></para>
        /// This returns null if <paramref name="targetInventory"/> is null or if this method is called outside of a Server Context.
        /// <br></br>
        /// Otherwise, returns a disposable object used to revoke all items and equipment gained this way.
        /// </summary>
        public IDisposable? ApplyToInventory(Inventory targetInventory)
        {
            if (!targetInventory || !NetworkServer.active)
                return null;

            _itemCountPairBuilder.Clear();
            EquipmentDef? equipmentDef = null;
            EquipmentIndex? originalIndex = null;

            targetInventory.InventoryChangedHandler((inv) =>
            {
                for (int i = 0; i < itemsToGrant.Length; i++)
                {
                    if (!itemsToGrant[i].TryGetItemCountPair(out var itemCountPair))
                    {
                        LogWarning($"AddressableItemCountPair at index {i} failed to transform into an ItemCountPair.");
                        continue;
                    }
                    _itemCountPairBuilder.Add(itemCountPair);

                    targetInventory.GiveItemChanneled(itemCountPair.itemDef.itemIndex, itemCountPair.count);
                }

                if (equipmentInfo.equipmentDef != null && equipmentInfo.equipmentDef.IsInvalid == false)
                {
                    equipmentDef = equipmentInfo.equipmentDef.LoadAssetNow();
                    if (equipmentDef)
                    {
                        EquipmentLocation defaultLocation = new EquipmentLocation
                        {
                            set = 0,
                            slot = 0
                        };

                        EquipmentState state = targetInventory.GetEquipment(defaultLocation);

                        if(equipmentInfo.overrideEquipment == true || state.equipmentIndex == EquipmentIndex.None)
                        {
                            originalIndex = state.equipmentIndex;
                            targetInventory.SetEquipmentIndex(equipmentDef.equipmentIndex, defaultLocation);
                        }

                        if(equipmentInfo.canTriggerEquipment)
                        {
                            VariantEquipmentHandler handler = targetInventory.EnsureComponent<VariantEquipmentHandler>();
                            handler.SetEquipmentData(equipmentDef.equipmentIndex, equipmentInfo.aiMaxUseHealthFraction, equipmentInfo.aiMaxUseDistance);
                        }
                    }
                    else
                    {
                        LogWarning($"AddressableEquipmentInfo failed to load it's EquipmentDef");
                    }
                }
            });


            return new DisposableVariantInventoryDefintionModification(targetInventory, _itemCountPairBuilder.ToArray(), equipmentDef, originalIndex);
        }

        /// <summary>
        /// Clones the metadata in this into a new instance of <see cref="VariantInventoryDefinition"/>
        /// </summary>
        public object Clone()
        {
            VariantInventoryDefinition result = new VariantInventoryDefinition();
            HG.ArrayUtils.EnsureCapacity(ref result.itemsToGrant, itemsToGrant.Length);
            for (int i = 0; i < result.itemsToGrant.Length; i++)
            {
                result.itemsToGrant[i] = (AddressableItemCountPair)itemsToGrant[i].Clone();
            }
            result.equipmentInfo = (AddressableEquipmentInfo)equipmentInfo.Clone();
            return result;
        }

        #region Constructors
        public VariantInventoryDefinition(AddressableItemCountPair singleItem) : this(singleItem, default) { }
        public VariantInventoryDefinition(AddressableItemCountPair[] itemsToGrant) : this(itemsToGrant, default) { }
        public VariantInventoryDefinition(AddressableEquipmentInfo equipmentInfo) : this(Array.Empty<AddressableItemCountPair>(), equipmentInfo) { }
        public VariantInventoryDefinition(AddressableItemCountPair singleItem, AddressableEquipmentInfo equipmentInfo) : this(new AddressableItemCountPair[] { singleItem }, equipmentInfo) { }
        public VariantInventoryDefinition(AddressableItemCountPair[] itemsToGrant, AddressableEquipmentInfo equipmentInfo)
        {
            this.equipmentInfo = equipmentInfo;
            this.itemsToGrant = itemsToGrant;
        }
        public VariantInventoryDefinition() { }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogWarning(object data)
        {
#if DEBUG
            VAPILog.Warning(data);
#endif
        }
    }
}