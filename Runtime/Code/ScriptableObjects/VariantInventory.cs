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

        [Serializable]
        public class EquipmentInfo
        {
            public AddressableEquipmentDef equipmentDef;
            public bool usable;
            public float aiMaxUseHealthFraction;
            public float aiMaxUseDistance;
            public float aiUseDelayMax;
        }

        public ItemPair[] itemInventory = Array.Empty<ItemPair>();

        public BuffInfo[] buffInfos = Array.Empty<BuffInfo>();

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
