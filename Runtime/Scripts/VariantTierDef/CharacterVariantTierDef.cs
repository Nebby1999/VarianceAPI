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
    public sealed class CharacterVariantTierDef : CachedNameScriptableObject
    {
        private struct DisposableVariantTierBodyModifier : IDisposable
        {
            private CharacterBody _affectedBody;
            private BuffDef? _buffDefToRemove;
            private int _armorCountToRemove;

            public DisposableVariantTierBodyModifier(CharacterBody affectedBody, BuffDef? buffDefToRemove, int armorBonusToRemove)
            {
                _affectedBody = affectedBody;
                _buffDefToRemove = buffDefToRemove;
                _armorCountToRemove = armorBonusToRemove;
            }

            public void Dispose()
            {
                if (!_affectedBody)
                    return;

                if(_buffDefToRemove)
                    _affectedBody.RemoveBuff(_buffDefToRemove);

                for(int i = 0; i < _armorCountToRemove; i++)
                {
                    _affectedBody.RemoveBuff(VAPIContent.Buffs.LinearArmorBonus.asset);
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

        [Header("Reward")]
        [Min(0 + float.Epsilon)]
        public float experienceRewardCoefficient;
        [Min(0 + float.Epsilon)]
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

        public IDisposable? ModifyBody(CharacterBody characterBody)
        {
            if (!characterBody && !NetworkServer.active)
            {
                return null;
            }

            BuffDef? buffDef = tierBuffDef.LoadAssetNow();
            if (buffDef)
            {
                characterBody.AddBuff(buffDef);
            }

            int bonusArmorCount = Mathf.CeilToInt(bonusArmor);
            for (int i = 0; i < bonusArmorCount; i++)
            {
                characterBody.AddBuff(VAPIContent.Buffs.LinearArmorBonus.asset);
            }

            return new DisposableVariantTierBodyModifier(characterBody, buffDef, bonusArmorCount);
        }

        public IDisposable? ModifyMaster(CharacterMaster master)
        {
            if (!master || !master.inventory || !NetworkServer.active)
            {
                return null;
            }

            List<ItemCountPair> items = new List<ItemCountPair>();
            for (int i = 0; i < tierItems.Length; i++)
            {
                if (tierItems[i].TryGetItemCountPair(out var itemCountPair))
                {
                    items.Add(itemCountPair);
                    master.inventory.GiveItemChanneled(itemCountPair.itemDef.itemIndex, itemCountPair.count);
                }
            }

            return new DisposableVariantTierMasterModifier(master, items.ToArray());
        }

        public struct CreateInstanceArgs
        {
            public string name;
            public bool? announceArrivalInChat;
            public List<AddressableItemCountPair>? tierItems;
            public AddressReferencedBuffDef? tierBuffDef;
            public float? bonusArmor;

            public float? expRewardCoefficient;
            public float? goldRewardCoefficient;

            public float? commonItemRewardChance;
            public float? uncommonItemRewardChance;
            public float? legendaryItemRewardChance;
            public float? bossItemRewardChance;

            public CreateInstanceArgs SetName(string _name)
            {
                name = _name ?? throw new ArgumentNullException(nameof(_name));
                return this;
            }

            public CreateInstanceArgs SetAnnounceArrivalInChat(bool _announceArrivalInChat)
            {
                announceArrivalInChat = _announceArrivalInChat;
                return this;
            }

            public CreateInstanceArgs AddTierItem(AddressableItemCountPair item)
            {
                tierItems ??= new List<AddressableItemCountPair>();
                tierItems.Add(item);
                return this;
            }

            public CreateInstanceArgs SetTierBuff(AddressReferencedBuffDef _tierBuffDef)
            {
                tierBuffDef = _tierBuffDef;
                return this;
            }

            public CreateInstanceArgs SetBonusArmor(float _bonusArmor)
            {
                bonusArmor = _bonusArmor;
                return this;
            }

            public CreateInstanceArgs SetExpRewardCoefficient(float _expRewardCoefficient)
            {
                expRewardCoefficient = _expRewardCoefficient;
                return this;
            }

            public CreateInstanceArgs SetGoldRewardCoefficient(float _goldRewardCoefficient)
            {
                goldRewardCoefficient = _goldRewardCoefficient;
                return this;
            }

            public CreateInstanceArgs SetCommonItemRewardChance(float _commonItemRewardChance)
            {
                commonItemRewardChance = _commonItemRewardChance;
                return this;
            }

            public CreateInstanceArgs SetUncommonItemRewardChance(float _uncommonItemRewardChance)
            {
                uncommonItemRewardChance = _uncommonItemRewardChance;
                return this;
            }

            public CreateInstanceArgs SetLegendaryItemRewardChance(float _legendaryItemRewardChance)
            {
                legendaryItemRewardChance = _legendaryItemRewardChance;
                return this;
            }

            public CreateInstanceArgs SetBossItemRewardChance(float _bossItemRewardChance)
            {
                bossItemRewardChance = _bossItemRewardChance;
                return this;
            }
        }
        public static CharacterVariantTierDef CreateInstance(CreateInstanceArgs args)
        {
            CharacterVariantTierDef tierDef = CreateInstance<CharacterVariantTierDef>();
            tierDef.cachedName = args.name;
            tierDef.announceArrivalInChat = args.announceArrivalInChat ?? false;
            tierDef.tierItems = args.tierItems?.ToArray() ?? tierDef.tierItems;
            tierDef.tierBuffDef = args.tierBuffDef ?? tierDef.tierBuffDef;
            tierDef.bonusArmor = args.bonusArmor ?? 0;
            tierDef.experienceRewardCoefficient = args.expRewardCoefficient ?? 0 + float.Epsilon;
            tierDef.goldRewardCoefficient = args.goldRewardCoefficient ?? 0 + float.Epsilon;
            tierDef.commonItemRewardChance = args.commonItemRewardChance ?? 0;
            tierDef.uncommonItemRewardChance = args.uncommonItemRewardChance ?? 0;
            tierDef.legendaryItemRewardChance = args.legendaryItemRewardChance ?? 0;
            tierDef.bossItemRewardChance = args.bossItemRewardChance ?? 0;
            return tierDef;
        }

        public static CharacterVariantTierDef CreateInstance(CharacterVariantTierDef other, string name)
        {
            CreateInstanceArgs args = new CreateInstanceArgs
            {
                name = name,
                announceArrivalInChat = other.announceArrivalInChat,
                bonusArmor = other.bonusArmor,
                bossItemRewardChance = other.bossItemRewardChance,
                commonItemRewardChance = other.commonItemRewardChance,
                expRewardCoefficient = other.experienceRewardCoefficient,
                goldRewardCoefficient = other.goldRewardCoefficient,
                legendaryItemRewardChance = other.legendaryItemRewardChance,
                uncommonItemRewardChance = other.uncommonItemRewardChance,
            };
            args.SetTierBuff(VAPIUtils.CloneAddressReferencedAsset<AddressReferencedBuffDef, BuffDef>(other.tierBuffDef));
            for(int i = 0; i < other.tierItems.Length; i++)
            {
                args.AddTierItem((AddressableItemCountPair)other.tierItems[i].Clone());
            }

            return CreateInstance(args);
        }
    }
}