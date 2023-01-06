using Moonstorm.AddressableAssets;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using VAPI.Components;

namespace VAPI
{
    /// <summary>
    /// A ScriptableObject that represents an Inventory of items, equipments and buffs to add to a variant
    /// </summary>
    [CreateAssetMenu(fileName = "New VariantInventory", menuName = "VarianceAPI/VariantInventory")]
    public class VariantInventory : ScriptableObject
    {
        /// <summary>
        /// Represents a pair of ItemDef and item count
        /// </summary>
        [Serializable]
        public class ItemPair
        {
            [Tooltip("A direct reference of address/item name for the item to give.\n" +
                "To the right you can specify the stacks.")]
            public AddressableItemDef itemDef;
            /// <summary>
            /// The amount oif items to give
            /// </summary>
            public int amount;
        }

        /// <summary>
        /// Represents a Buff to give to this variant
        /// </summary>
        [Serializable]
        public class BuffInfo
        {
            [Tooltip("A direct reference of address/buffDef name for the buff to give.")]
            public AddressableBuffDef buffDef;
            [Tooltip("The amount of time this buff lasts, set this to 0 to make it permanent.")]
            public float time;
            [Tooltip("how many stacks to give if time is greater than 0")]
            public int amount;
        }

        /// <summary>
        /// Represents an Equipment to give to this variant
        /// </summary>
        [Serializable]
        public class EquipmentInfo
        {
            [Tooltip("A direct reference of Address/EquipmentDef name for the equipment to give")]
            public AddressableEquipmentDef equipmentDef;
            [Tooltip("Wether the variant has the ability to trigger this equipment")]
            public bool usable;
            [Tooltip("The amount of health missing from the variant for this equipment to trigger")]
            public float aiMaxUseHealthFraction;
            [Tooltip("How far the variant's target must be for this equipment to not trigger")]
            public float aiMaxUseDistance;
            [Tooltip("How often this equipment may attempt to trigger, in seconds")]
            public float aiUseDelayMax;
        }

        [Tooltip("The items that this VariantInventory gives.")]
        public ItemPair[] itemInventory = Array.Empty<ItemPair>();

        [Tooltip("A list of buffs to give the variant.")]
        public BuffInfo[] buffInfos = Array.Empty<BuffInfo>();

        [Tooltip("An equipment to give to the variant.")]
        public EquipmentInfo equipmentInfo;

        /// <summary>
        /// Adds the items in <see cref="itemInventory"/> to the target inventory
        /// </summary>
        /// <param name="targetInventory">The inventory that will recieve the items</param>
        public virtual void AddItems(Inventory targetInventory)
        {
            if (!NetworkServer.active)
                return;

            foreach (ItemPair pair in itemInventory)
            {
                if (pair.itemDef.Asset)
                {
                    targetInventory.GiveItem(pair.itemDef.Asset, pair.amount);
                }
            }
        }

        /// <summary>
        /// Adds the buffs in <see cref="buffInfos"/> to the target body
        /// </summary>
        /// <param name="targetBody">The boddy that will recieve the buffs</param>
        public virtual void AddBuffs(CharacterBody targetBody)
        {
            if (!NetworkServer.active)
                return;

            foreach (BuffInfo buff in buffInfos)
            {
                if (buff.buffDef.Asset)
                {
                    if (buff.time <= 0)
                    {
                        targetBody.AddBuff(buff.buffDef.Asset);
                    }
                    else
                    {
                        for (int i = 0; i < buff.amount; i++)
                        {
                            targetBody.AddTimedBuff(buff.buffDef.Asset, buff.time);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the EquipmentInfo specified in <see cref="equipmentInfo"/> to a variant
        /// </summary>
        /// <param name="targetInventory">The variant's inventory</param>
        /// <param name="body">The variant's body, used to adding the <see cref="VariantEquipmentHandler"/></param>
        public virtual void SetEquipment(Inventory targetInventory, CharacterBody body)
        {
            if (!NetworkServer.active)
                return;

            if (equipmentInfo.equipmentDef.Asset)
                targetInventory.SetEquipmentIndex(equipmentInfo.equipmentDef.Asset.equipmentIndex);

            if (equipmentInfo.usable)
            {
                var equipHandler = body.gameObject.AddComponent<VariantEquipmentHandler>();
                equipHandler.aiMaxUseHealthFraction = equipmentInfo.aiMaxUseHealthFraction;
                equipHandler.aiMaxUseDistance = equipmentInfo.aiMaxUseDistance;
                equipHandler.aiUseDelayMax = equipmentInfo.aiUseDelayMax;
                equipHandler.body = body;
            }
        }
    }
}
