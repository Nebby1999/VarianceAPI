#nullable enable
using BepInEx;
using BepInEx.Configuration;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// Scriptable object that defines a Tier for variants.
    /// </summary>
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
                    _affectedBody.RemoveBuff(VAPIContent.Buffs.VAPILinearArmorBonus.asset);
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

        [Tooltip("If true, variants of this tier will announce their arrival in chat. If multiple variants are assigned to the character then a random message is picked.")]
        public bool announceArrivalInChat;

        [Tooltip("An array of AddressableItemCountPair, used to give variants items")]
        public AddressableItemCountPair[] tierItems = Array.Empty<AddressableItemCountPair>();

        [Tooltip("Variants of this tier will have this buff")]
        public AddressReferencedBuffDef tierBuffDef = new AddressReferencedBuffDef();

        [Tooltip("A sound event that plays when the variant spawns")]
        public NetworkSoundEventDef? spawnedSoundEvent;

        [Tooltip("A flat bonus armor added to variants of this tier. This value is compounded if multiple variants are assigned to a character")]
        public float bonusArmor;

        [Tooltip("The reward coefficient for experience")]
        [Header("Reward")]
        [Min(0)]
        public float experienceRewardCoefficient;

        [Tooltip("The reward coefficient for gold")]
        [Min(0)]
        public float goldRewardCoefficient;

        /// <summary>
        /// Returns true if <see cref="commonItemRewardChance"/> is greater than 0
        /// </summary>
        public bool canDropCommon => commonItemRewardChance > 0;

        [Tooltip("The chance for variants of this tier to drop a white item")]
        [Min(0)]
        public float commonItemRewardChance;

        /// <summary>
        /// Returns true if <see cref="uncommonItemRewardChance"/> is greater than 0
        /// </summary>
        public bool canDropUncommon => uncommonItemRewardChance > 0;

        [Tooltip("The chance for variants of this tier to drop a green item")]
        [Min(0)]
        public float uncommonItemRewardChance;

        /// <summary>
        /// Returns true if <see cref="legendaryItemRewardChance"/> is greater than 0
        /// </summary>
        public bool canDropLegendary => legendaryItemRewardChance > 0;

        [Tooltip("The chance for variants of this tier to drop a green item")]
        [Min(0)]
        public float legendaryItemRewardChance;

        /// <summary>
        /// Returns true if <see cref="bossItemRewardChance"/> is greater than 0
        /// </summary>
        public bool canDropBoss => bossItemRewardChance > 0;

        [Tooltip("The chance for variants of this tier that are champions to drop their yellow item")]
        [Min(0)]
        public float bossItemRewardChance;

        /// <summary>
        /// The <see cref="CharacterVariantTierIndex"/> assigned to this VariantTierDef
        /// </summary>
        public CharacterVariantTierIndex characterVariantTierIndex { get; internal set; }

        /// <summary>
        /// The config file used to configure this variant tier. Set by the VariantPackManager
        /// </summary>
        public ConfigFile? associatedConfigFile { get; internal set; }

        /// <summary>
        /// The mod that added this Variant Tier. Set by the VariantPackManager.
        /// </summary>
        public BepInPlugin? ownerPlugin { get; internal set; }

        internal IDisposable? ModifyBody(CharacterBody characterBody)
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
                characterBody.AddBuff(VAPIContent.Buffs.VAPILinearArmorBonus.asset);
            }

            return new DisposableVariantTierBodyModifier(characterBody, buffDef, bonusArmorCount);
        }

        internal IDisposable? ModifyMaster(CharacterMaster master)
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

        /// <summary>
        /// Struct used to create an instance of <see cref="CharacterVariantTierDef"/>
        /// </summary>
        public struct CreateInstanceArgs
        {
            /// <summary>
            /// The name for the scriptable object, must be unique
            /// </summary>
            public string name;

            /// <summary>
            /// Defaults to false
            /// </summary>
            public bool? announceArrivalInChat;
            public List<AddressableItemCountPair>? tierItems;
            public AddressReferencedBuffDef? tierBuffDef;

            public string? spawnedSoundEvent;

            /// <summary>
            /// Defaults to 0
            /// </summary>
            public float? bonusArmor;

            /// <summary>
            /// Defaults to 1
            /// </summary>
            public float? expRewardCoefficient;

            /// <summary>
            /// Defaults to 1
            /// </summary>
            public float? goldRewardCoefficient;

            /// <summary>
            /// Defaults to 0
            /// </summary>
            public float? commonItemRewardChance;

            /// <summary>
            /// Defaults to 0
            /// </summary>
            public float? uncommonItemRewardChance;

            /// <summary>
            /// Defaults to 0
            /// </summary>
            public float? legendaryItemRewardChance;

            /// <summary>
            /// Defaults to 0
            /// </summary>
            public float? bossItemRewardChance;

            /// <summary>Chainable method, sets <see cref="name"/> to <paramref name="_name"/></summary>
            public CreateInstanceArgs SetName(string _name)
            {
                name = _name ?? throw new ArgumentNullException(nameof(_name));
                return this;
            }

            /// <summary>Chainable method, sets <see cref="announceArrivalInChat"/> to <paramref name="_announceArrivalInChat"/></summary>
            public CreateInstanceArgs SetAnnounceArrivalInChat(bool _announceArrivalInChat)
            {
                announceArrivalInChat = _announceArrivalInChat;
                return this;
            }

            /// <summary>Chainable method, adds <paramref name="item"/> to <see cref="tierItems"/></summary>
            public CreateInstanceArgs AddTierItem(AddressableItemCountPair item)
            {
                tierItems ??= new List<AddressableItemCountPair>();
                tierItems.Add(item);
                return this;
            }

            /// <summary>Chainable method, sets <see cref="tierBuffDef"/> to <paramref name="_tierBuffDef"/></summary>
            public CreateInstanceArgs SetTierBuff(AddressReferencedBuffDef _tierBuffDef)
            {
                tierBuffDef = _tierBuffDef;
                return this;
            }

            public CreateInstanceArgs SetSpawnedSoundEvent(string _spawnedSoundEvent)
            {
                spawnedSoundEvent = _spawnedSoundEvent;
                return this;
            }

            /// <summary>Chainable method, sets <see cref="bonusArmor"/> to <paramref name="_bonusArmor"/></summary>
            public CreateInstanceArgs SetBonusArmor(float _bonusArmor)
            {
                bonusArmor = _bonusArmor;
                return this;
            }

            /// <summary>Chainable method, sets <see cref="expRewardCoefficient"/> to <paramref name="_expRewardCoefficient"/></summary>
            public CreateInstanceArgs SetExpRewardCoefficient(float _expRewardCoefficient)
            {
                expRewardCoefficient = _expRewardCoefficient;
                return this;
            }

            /// <summary>Chainable method, sets <see cref="goldRewardCoefficient"/> to <paramref name="_goldRewardCoefficient"/></summary>
            public CreateInstanceArgs SetGoldRewardCoefficient(float _goldRewardCoefficient)
            {
                goldRewardCoefficient = _goldRewardCoefficient;
                return this;
            }

            /// <summary>Chainable method, sets <see cref="commonItemRewardChance"/> to <paramref name="_commonItemRewardChance"/></summary>
            public CreateInstanceArgs SetCommonItemRewardChance(float _commonItemRewardChance)
            {
                commonItemRewardChance = _commonItemRewardChance;
                return this;
            }

            /// <summary>Chainable method, sets <see cref="uncommonItemRewardChance"/> to <paramref name="_uncommonItemRewardChance"/></summary>
            public CreateInstanceArgs SetUncommonItemRewardChance(float _uncommonItemRewardChance)
            {
                uncommonItemRewardChance = _uncommonItemRewardChance;
                return this;
            }

            /// <summary>Chainable method, sets <see cref="legendaryItemRewardChance"/> to <paramref name="_legendaryItemRewardChance"/></summary>
            public CreateInstanceArgs SetLegendaryItemRewardChance(float _legendaryItemRewardChance)
            {
                legendaryItemRewardChance = _legendaryItemRewardChance;
                return this;
            }

            /// <summary>Chainable method, sets <see cref="bossItemRewardChance"/> to <paramref name="_bossItemRewardChance"/></summary>
            public CreateInstanceArgs SetBossItemRewardChance(float _bossItemRewardChance)
            {
                bossItemRewardChance = _bossItemRewardChance;
                return this;
            }
        }

        /// <summary>
        /// Creates an instance of a <see cref="CharacterVariantTierDef"/> based off the arguments passed in <paramref name="args"/>
        /// </summary>
        public static CharacterVariantTierDef CreateInstance(CreateInstanceArgs args)
        {
            CharacterVariantTierDef tierDef = CreateInstance<CharacterVariantTierDef>(args.name);
            tierDef.announceArrivalInChat = args.announceArrivalInChat ?? false;
            tierDef.tierItems = args.tierItems?.ToArray() ?? tierDef.tierItems;
            tierDef.tierBuffDef = args.tierBuffDef ?? tierDef.tierBuffDef;
            tierDef.spawnedSoundEvent = args.spawnedSoundEvent ?? "";
            tierDef.bonusArmor = args.bonusArmor ?? 0;
            tierDef.experienceRewardCoefficient = args.expRewardCoefficient ?? 0 + float.Epsilon;
            tierDef.goldRewardCoefficient = args.goldRewardCoefficient ?? 0 + float.Epsilon;
            tierDef.commonItemRewardChance = args.commonItemRewardChance ?? 0;
            tierDef.uncommonItemRewardChance = args.uncommonItemRewardChance ?? 0;
            tierDef.legendaryItemRewardChance = args.legendaryItemRewardChance ?? 0;
            tierDef.bossItemRewardChance = args.bossItemRewardChance ?? 0;
            return tierDef;
        }

        /// <summary>
        /// Creates a clone of <paramref name="other"/>, assigns its name to <paramref name="name"/>, and returns the clone.
        /// </summary>
        public static CharacterVariantTierDef CreateInstance(CharacterVariantTierDef other, string name)
        {
            CreateInstanceArgs args = new CreateInstanceArgs
            {
                name = name,
                announceArrivalInChat = other.announceArrivalInChat,
                spawnedSoundEvent = other.spawnedSoundEvent,
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