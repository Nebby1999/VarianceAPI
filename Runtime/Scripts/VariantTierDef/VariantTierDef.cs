#nullable enable
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantTierDef", menuName = "VarianceAPI/VariantTierDef")]
    public class VariantTierDef : ScriptableObject
    {
        private struct DisposableVariantTierBodyModifier : IDisposable
        {
            private CharacterBody _affectedBody;
            private BuffDef _buffDefToRemove;
            private int _armorCountToRemove;

            public DisposableVariantTierBodyModifier(CharacterBody affectedBody, BuffDef buffDefToRemove, int armorBonusToRemove)
            {
                _affectedBody = affectedBody;
                _buffDefToRemove = buffDefToRemove;
                _armorCountToRemove = armorBonusToRemove;
            }

            public void Dispose()
            {
                if (!_affectedBody)
                    return;

                _affectedBody.RemoveBuff(_buffDefToRemove);

                for(int i = 0; i < _armorCountToRemove; i++)
                {
                    _affectedBody.RemoveBuff(VAPIContent.Buffs.LinearArmorBonus!);
                }
            }
        }
        private struct DisposableVariantTierMasterModifier : IDisposable
        {
            private ItemCountPair[] _countPairs;
            private CharacterMaster _affectedMaster;

            public DisposableVariantTierMasterModifier(CharacterMaster affectedMaster, ItemCountPair[] itemCountPairs)
            {
                _countPairs = itemCountPairs;
                _affectedMaster = affectedMaster;
            }
            public void Dispose()
            {
                if (!_affectedMaster || !_affectedMaster.inventory || !NetworkServer.active)
                    return;

                if (_countPairs == null || _countPairs.Length == 0)
                    return;

                for(int i = 0; i < _countPairs.Length; i++)
                {
                    _affectedMaster.inventory.RemoveItemChanneled(_countPairs[i].itemDef.itemIndex, _countPairs[i].count);
                }
            }
        }

        public bool announceArrivalInChat;
        public AddressableItemCountPair[] tierItems = Array.Empty<AddressableItemCountPair>();
        public AddressReferencedBuffDef tierBuffDef = new AddressReferencedBuffDef();
        public float bonusArmor;

        public IDisposable? ModifyBody(CharacterBody characterBody)
        {
            if(!characterBody && !NetworkServer.active)
            {
                return null;
            }

            BuffDef buffDef = tierBuffDef.LoadAssetNow();
            if(buffDef)
            {
                characterBody.AddBuff(buffDef);
            }

            int bonusArmorCount = Mathf.CeilToInt(bonusArmor);
            for(int i = 0; i < bonusArmorCount; i++)
            {
                characterBody.AddBuff(VAPIContent.Buffs.LinearArmorBonus);
            }

            return new DisposableVariantTierBodyModifier(characterBody, buffDef, bonusArmorCount);
        }

        public IDisposable? ModifyMaster(CharacterMaster master)
        {
            if(!master || !master.inventory || !NetworkServer.active)
            {
                return null;
            }

            List<ItemCountPair> items = new List<ItemCountPair>();
            for(int i = 0; i < tierItems.Length; i++)
            {
                if (tierItems[i].TryGetItemCountPair(out var itemCountPair))
                {
                    items.Add(itemCountPair);
                    master.inventory.GiveItemChanneled(itemCountPair.itemDef.itemIndex, itemCountPair.count);
                }
            }

            return new DisposableVariantTierMasterModifier(master, items.ToArray());
        }

        [Header("Reward")]
        [Min(1)]
        public float experienceRewardCoefficient;
        [Min(1)]
        public float goldRewardCoefficient;

        public bool canDropCommon => commonItemRewardChance > 0;
        [Min(0)]
        public float commonItemRewardChance;

        public bool canDropUncommon => uncommonItemRewardChance > 0;
        [Min(0)]
        public float uncommonItemRewardChance;

        public bool canDropLegendary => legendaryItemRewardChance > 0;
        [Min(0)]
        public float legendaryItemRewardChance;

        public bool canDropBoss => bossItemRewardChance > 0;
        [Min(0)]
        public float bossItemRewardChance;
    }
}