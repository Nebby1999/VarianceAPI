#nullable enable
using BepInEx.Configuration;
using HG;
using HG.Coroutines;
using MSU.Config;
using RiskOfOptions.OptionConfigs;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// Index for <see cref="CharacterVariantTierDef"/>s
    /// </summary>
    public enum CharacterVariantTierIndex
    {
        /// <summary>
        /// Represents ain invalid index
        /// </summary>
        None = -1,
    }

    /// <summary>
    /// Catalog for <see cref="CharacterVariantTierDef"/>
    /// </summary>
    public static class CharacterVariantTierCatalog
    {
        /// <summary>
        /// A <see cref="ReadOnlyArray{T}"/> of the registered <see cref="CharacterVariantTierDef"/>s
        /// </summary>
        public static ReadOnlyArray<CharacterVariantTierDef> registeredCharacterVariantTierDefs => _characterVariantTierDefs;

        /// <summary>
        /// The amount of <see cref="CharacterVariantTierDef"/>s stored in the catalog.
        /// </summary>
        public static int characterVariantTierCount => _characterVariantTierDefs?.Length ?? -1;
        private static CharacterVariantTierDef[]? _characterVariantTierDefs;
        private static Dictionary<string, CharacterVariantTierIndex> _characterVariantTierNametoIndex = new Dictionary<string, CharacterVariantTierIndex>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Catalog availability. Catalog initializes after <see cref="VariantPackManager"/>
        /// </summary>
        public static ResourceAvailability catalogAvailability;

        /// <summary>
        /// Gets the <see cref="CharacterVariantTierDef"/> with the specified <paramref name="index"/>
        /// <br></br>
        /// Returns null if the <paramref name="index"/> is out of range
        /// </summary>
        /// <param name="index">The index of the <see cref="CharacterVariantTierDef"/></param>
        public static CharacterVariantTierDef? GetCharacterVariantTierDef(CharacterVariantTierIndex index)
        {
            ThrowIfUnavailable();
            return HG.ArrayUtils.GetSafe(_characterVariantTierDefs!, (int)index);
        }

        /// <summary>
        /// Finds the <see cref="CharacterVariantTierIndex"/> that has the <see cref="CharacterVariantTierDef"/> with the specified name in <paramref name="characterVariantTierName"/>
        /// <br></br>
        /// Returns <see cref="CharacterVariantTierIndex.None"/> if there's no <see cref="CharacterVariantTierDef"/> with the name <paramref name="characterVariantTierName"/>
        /// </summary>
        /// <param name="characterVariantTierName">The name of the <see cref="CharacterVariantTierDef"/></param>
        public static CharacterVariantTierIndex FindCharacterVariantTierIndex(string? characterVariantTierName)
        {
            ThrowIfUnavailable();

            if(string.IsNullOrWhiteSpace(characterVariantTierName))
            {
                return CharacterVariantTierIndex.None;
            }

            return _characterVariantTierNametoIndex.GetValueOrDefault(characterVariantTierName, CharacterVariantTierIndex.None);
        }

        [SystemInitializer(typeof(VariantPackManager))]
        private static IEnumerator Initialize()
        {
            _characterVariantTierNametoIndex.Clear();

            using (var coroutineWithResult = CoroutineWithResult<CharacterVariantTierDef[]>.Rent(RegisterTierDefs(VariantPackManager.characterVariantTierDefs)))
            {
                while(coroutineWithResult.MoveNext())
                {
                    yield return null;
                }

                _characterVariantTierDefs = coroutineWithResult.result;
            }

            VAPILog.Info($"CharacterVariantTierCatalog Initialized.");

            catalogAvailability.MakeAvailable();
        }

        private static IEnumerator<CharacterVariantTierDef[]?> RegisterTierDefs(ReadOnlyArray<CharacterVariantTierDef> tiers)
        {
            CharacterVariantTierDef[] sortedTiers = new CharacterVariantTierDef[tiers.Length];
            tiers.CopyTo(sortedTiers, 0);

            Array.Sort(sortedTiers, (a, b) => string.CompareOrdinal(a.cachedName, b.cachedName));

            for (CharacterVariantTierIndex index = 0; ((int)index) < sortedTiers.Length; index++)
            {
                yield return null;
                CharacterVariantTierDef tierDef = sortedTiers[(int)index];

                if (tierDef.associatedConfigFile != null)
                {
                    ConfigureTier(tierDef);
                }

                tierDef.characterVariantTierIndex = index;
                _characterVariantTierNametoIndex.Add(tierDef.cachedName, index);
            }

            yield return sortedTiers;
        }

        private static void ConfigureTier(CharacterVariantTierDef tierDef)
        {
            try
            {
                tierDef.goldRewardCoefficient = new ConfiguredFloat(tierDef.goldRewardCoefficient)
                {
                    section = $"{tierDef.cachedName} Tier",
                    key = "Gold Multiplier",
                    description = "The Gold Multiplier for this Tier.",
                    configFile = tierDef.associatedConfigFile,
                    modName = tierDef.ownerPlugin.Name,
                    modGUID = tierDef.ownerPlugin.GUID,
                    sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                    sliderConfig = new SliderConfig
                    {
                        FormatString = "{0:0.0}",
                        min = 0,
                        max = 100,
                        checkIfDisabled = () => !VAPIConfig._enableRewards
                    },
                }.WithConfigChange(f =>
                {
                    tierDef.goldRewardCoefficient = f;
                }).DoConfigure();

                tierDef.experienceRewardCoefficient = new ConfiguredFloat(tierDef.experienceRewardCoefficient)
                {
                    section = $"{tierDef.cachedName} Tier",
                    key = "Experience Multiplier",
                    description = "The Experience Multiplier for this Tier",
                    configFile = tierDef.associatedConfigFile,
                    modName = tierDef.ownerPlugin.Name,
                    modGUID = tierDef.ownerPlugin.GUID,
                    sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                    sliderConfig = new SliderConfig
                    {
                        FormatString = "{0:0.0}",
                        min = 0,
                        max = 100,
                        checkIfDisabled = () => !VAPIConfig._enableRewards
                    },
                }.WithConfigChange(f =>
                {
                    tierDef.experienceRewardCoefficient = f;
                }).DoConfigure();

                tierDef.commonItemRewardChance = new ConfiguredFloat(tierDef.commonItemRewardChance)
                {
                    section = $"{tierDef.cachedName} Tier",
                    key = "Common Item Drop Chance",
                    description = "The Chance for variants of this Tier to drop a Common Item",
                    configFile = tierDef.associatedConfigFile,
                    modName = tierDef.ownerPlugin.Name,
                    modGUID = tierDef.ownerPlugin.GUID,
                    sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                    sliderConfig = new SliderConfig
                    {
                        FormatString = "{0:0.0}",
                        min = 0,
                        max = 100,
                        checkIfDisabled = () => !VAPIConfig._enableRewards
                    },
                }.WithConfigChange(f =>
                {
                    tierDef.commonItemRewardChance = f;
                }).DoConfigure();

                tierDef.uncommonItemRewardChance = new ConfiguredFloat(tierDef.uncommonItemRewardChance)
                {
                    section = $"{tierDef.cachedName} Tier",
                    key = "Uncommon Item Drop Chance",
                    description = "The Chance for variants of this Tier to drop an Uncommon Item",
                    configFile = tierDef.associatedConfigFile,
                    modName = tierDef.ownerPlugin.Name,
                    modGUID = tierDef.ownerPlugin.GUID,
                    sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                    sliderConfig = new SliderConfig
                    {
                        FormatString = "{0:0.0}",
                        min = 0,
                        max = 100,
                        checkIfDisabled = () => !VAPIConfig._enableRewards
                    },
                }.WithConfigChange(f =>
                {
                    tierDef.uncommonItemRewardChance = f;
                }).DoConfigure();

                tierDef.legendaryItemRewardChance = new ConfiguredFloat(tierDef.legendaryItemRewardChance)
                {
                    section = $"{tierDef.cachedName} Tier",
                    key = "Red Item Drop Chance",
                    description = "The Chance for variants of this Tier to drop a Red Item",
                    configFile = tierDef.associatedConfigFile,
                    modName = tierDef.ownerPlugin.Name,
                    modGUID = tierDef.ownerPlugin.GUID,
                    sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                    sliderConfig = new SliderConfig
                    {
                        FormatString = "{0:0.0}",
                        min = 0,
                        max = 100,
                        checkIfDisabled = () => !VAPIConfig._enableRewards
                    },
                }.WithConfigChange(f =>
                {
                    tierDef.legendaryItemRewardChance = f;
                }).DoConfigure();

                tierDef.bossItemRewardChance = new ConfiguredFloat(tierDef.bossItemRewardChance)
                {
                    section = $"{tierDef.cachedName} Tier",
                    key = "Boss Item Drop Chance",
                    description = "The Chance for Champion Variants of this Tier to drop it's Boss Item",
                    configFile = tierDef.associatedConfigFile,
                    modName = tierDef.ownerPlugin.Name,
                    modGUID = tierDef.ownerPlugin.GUID,
                    sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                    sliderConfig = new SliderConfig
                    {
                        FormatString = "{0:0.0}",
                        min = 0,
                        max = 100,
                        checkIfDisabled = () => VAPIConfig._enableRewards
                    },
                }.WithConfigChange(f =>
                {
                    tierDef.bossItemRewardChance = f;
                }).DoConfigure();

                tierDef.bonusArmor = new ConfiguredFloat(tierDef.bonusArmor)
                {
                    section = $"{tierDef.cachedName} Tier",
                    key = "Tier Armor Bonus",
                    description = "Armor bonus applied to variants with this Tier, This value stacks additively when a Character has multiple CharacterVariantDefs applied.",
                    configFile = tierDef.associatedConfigFile,
                    modName = tierDef.ownerPlugin.Name,
                    modGUID = tierDef.ownerPlugin.GUID,
                    sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                    sliderConfig = new SliderConfig
                    {
                        min = 0,
                        max = 1000,
                    },
                }.WithConfigChange(f =>
                {
                    tierDef.bonusArmor = f;
                }).DoConfigure();
            }
            catch(Exception e)
            {
                VAPILog.Error($"Error configuring tier {tierDef}: {e}");
            }
        }

        private static void ThrowIfUnavailable()
        {
            if (!catalogAvailability.available)
            {
                throw new InvalidOperationException("Cannot access CharacterVariantTierCatalog when it's not available, consider subscribing to the catalogAvailability.");
            }
        }
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Writes a <see cref="CharacterVariantTierIndex"/> to <paramref name="writer"/>
        /// </summary>
        public static void Write(this NetworkWriter writer, CharacterVariantTierIndex index)
        {
            writer.WritePackedIndex32((int)index);
        }

        /// <summary>
        /// Reads a <see cref="CharacterVariantTierIndex"/> from <paramref name="reader"/>
        /// </summary>
        public static CharacterVariantTierIndex ReadCharacterVariantTierIndex(this NetworkReader reader)
        {
            return (CharacterVariantTierIndex)reader.ReadPackedIndex32();
        }
    }
}