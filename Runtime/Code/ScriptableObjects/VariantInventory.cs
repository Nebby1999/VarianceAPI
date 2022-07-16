using Moonstorm.AddressableAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VAPI.Components;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantInventory", menuName = "VarianceAPI/VariantInventory")]
    public class VariantInventory : ScriptableObject
    {
        [Serializable]
        public class ItemPair
        {
            [Tooltip("A direct reference of address/item name for the item to give.\n" +
                "To the right you can specify the stacks.")]
            public AddressableItemDef itemDef;
            public int amount;
        }

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

        public virtual void AddItems(Inventory targetInventory)
        {
            if (!NetworkServer.active)
                return;

            foreach (ItemPair pair in itemInventory)
            {
                if(pair.itemDef.Asset)
                {
                    targetInventory.GiveItem(pair.itemDef.Asset, pair.amount);
                }
            }
        }

        public virtual void AddBuffs(CharacterBody targetBody)
        {
            if (!NetworkServer.active)
                return;

            foreach(BuffInfo buff in buffInfos)
            {
                if (buff.buffDef.Asset)
                {
                    if (buff.time <= 0)
                    {
                        targetBody.AddBuff(buff.buffDef.Asset);
                    }
                    else
                    {
                        for(int i = 0; i < buff.amount; i++)
                        {
                            targetBody.AddTimedBuff(buff.buffDef.Asset, buff.time);
                        }
                    }
                }
            }
        }

        public virtual void SetEquipment(Inventory targetInventory, CharacterBody body)
        {
            if (!NetworkServer.active)
                return;

            if (equipmentInfo.equipmentDef.Asset)
                targetInventory.SetEquipmentIndex(equipmentInfo.equipmentDef.Asset.equipmentIndex);

            if(equipmentInfo.usable)
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
