#nullable enable
using HG;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace VAPI
{
    /// <summary>
    /// Utility methods for VarianceAPI
    /// </summary>
    public static class VAPIUtils
    {
        /// <summary>
        /// A static readonly instance of <see cref="WaitForEndOfFrame"/>
        /// </summary>
        public static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        /// <summary>
        /// Retrieves the <see cref="BodyIndex"/> for <paramref name="bodyName"/>
        /// <br></br>
        /// Uses DebugToolkit's partial body recognition if installed
        /// </summary>
        public static BodyIndex GetBodyIndex(string? bodyName)
        {
            if(string.IsNullOrWhiteSpace(bodyName))
            {
                return BodyIndex.None;
            }

            if (MSU.MSUtil.IsModInstalled(DebugToolkit.DebugToolkit.GUID))
            {
                return GetBodyIndexDT(bodyName);
            }
            return BodyCatalog.FindBodyIndex(bodyName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static BodyIndex GetBodyIndexDT(string bodyName)
        {
            return DebugToolkit.StringFinder.Instance.GetBodyFromPartial(bodyName);
        }

        /// <summary>
        /// Retrieves the <see cref="MasterCatalog.MasterIndex"/> for <paramref name="masterName"/>
        /// <br></br>
        /// Uses DebugToolkit's partial master recognition if installed
        /// </summary>
        public static MasterCatalog.MasterIndex GetMasterIndex(string? masterName)
        {
            if(string.IsNullOrWhiteSpace(masterName))
            {
                return MasterCatalog.MasterIndex.none;
            }

            if(MSU.MSUtil.IsModInstalled(DebugToolkit.DebugToolkit.GUID))
            {
                return GetMasterIndexDT(masterName);
            }
            return MasterCatalog.FindMasterIndex(masterName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static MasterCatalog.MasterIndex GetMasterIndexDT(string masterName)
        {
            return DebugToolkit.StringFinder.Instance.GetAiFromPartial(masterName);
        }

        /// <summary>
        /// Method utilized to clone an <see cref="AddressReferencedAsset"/>.
        /// <br></br>
        /// returns an empty <see cref="AddressReferencedAsset"/> if <paramref name="other"/> is null
        /// </summary>
        /// <typeparam name="T1">The type of AddressReferencedAsset we're cloning</typeparam>
        /// <typeparam name="T2">The type of asset that <see cref="AddressReferencedAsset"/> is loading</typeparam>
        /// <param name="other">The <see cref="AddressReferencedAsset"/> that we're copying</param>
        public static T1 CloneAddressReferencedAsset<T1, T2>(T1? other) where T1 : AddressReferencedAsset<T2>, new() where T2 : UnityEngine.Object
        {
            var copy = new T1();
            if (other == null)
                return copy;

            var address = other.Address;
            if(!string.IsNullOrWhiteSpace(address))
            {
                copy.Address = address;
                return copy;
            }
            if(other.AssetExists)
            {
                copy.Asset = other.Asset;
                return copy;
            }
            return copy;
        }

        /// <summary>
        /// Modifies the <paramref name="spawnedInstance"/> to become a variant.
        /// <br></br>
        /// This is done by pre-assigning the <paramref name="variantDefs"/> into the master's <see cref="CharacterMasterVariantStorage"/>. This method is used particularly in <see cref="VariantMasterSummon"/> and <see cref="VariantDirectorSpawnRequest"/>
        /// </summary>
        /// <param name="spawnedInstance">The master object to turn into a variant</param>
        /// <param name="variantDefs">The variant defs for the master</param>
        /// <param name="baseDeathRewards">A base death rewards, can be used to make the master drop extra loot</param>
        /// <param name="deathRewardsCoefficient">Used only if <paramref name="baseDeathRewards"/> is not null. The gold and experience reward will be multiplied by this coefficient</param>
        public static void TransformSpawnedInstanceIntoVariant(GameObject? spawnedInstance, CharacterVariantDef[]? variantDefs, DeathRewards? baseDeathRewards = null, float deathRewardsCoefficient = 1f)
        {
            if (spawnedInstance == null || variantDefs == null)
                return;

            if (!NetworkServer.active)
                return;

            if(spawnedInstance.TryGetComponent<CharacterBody>(out CharacterBody spawnedBodyInstance) == false)
            {
                if(spawnedInstance.TryGetComponent(out CharacterMaster spawnedInstanceMaster))
                {
                    spawnedBodyInstance = spawnedInstanceMaster.GetBody();
                }
            }

            if (!spawnedBodyInstance)
                return;

            if(baseDeathRewards && spawnedBodyInstance.TryGetComponent<DeathRewards>(out var deathRewards))
            {
                deathRewards.expReward = (uint)(baseDeathRewards!.expReward * deathRewardsCoefficient);
                deathRewards.goldReward = (uint)(baseDeathRewards!.goldReward * deathRewardsCoefficient);
            }

            if(spawnedBodyInstance.TryGetComponent<CharacterBodyVariantController>(out var bodyVariantController))
            {
                bodyVariantController.SetCharacterVariantDefsServer(variantDefs);
                bodyVariantController.doNotRollForVariants = true;
            }
        }
    }

    /// <summary>
    /// Basic extensions class
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Gets or Finds a GenericSkill from within <paramref name="locator"/>.
        /// <br></br>
        /// If <paramref name="slot"/> is none, the GenericSkill is retrieved using <paramref name="fallbackSkillName"/>
        /// <br></br>
        /// This method returns null if the locator doesnt have a GenericSkill at slot <paramref name="slot"/>, or if it doesnt have a GenericSkill with the name <paramref name="fallbackSkillName"/>
        /// </summary>
        /// <param name="locator">The locator where we want to find a skill</param>
        /// <param name="slot">The skill slot</param>
        /// <param name="fallbackSkillName">The skill's name</param>
        public static GenericSkill? GetOrFindSkill(this SkillLocator? locator, SkillSlot slot, string? fallbackSkillName)
        {
            if (locator == null)
                return null;

            if(slot == SkillSlot.None)
            {
                if (string.IsNullOrWhiteSpace(fallbackSkillName))
                    return null;

                return locator.FindSkill(fallbackSkillName);
            }
            else
            {
                return locator.GetSkill(slot);
            }
        }

        internal static int? TryGetOptionalInt(this ConCommandArgs args, string startingIdentifier, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            for (int i = 0; i < args.userArgs.Count; i++)
            {
                if (args.TryGetArgString(i).StartsWith(startingIdentifier, stringComparison))
                {
                    string rawArg = args.userArgs[i];
                    if (int.TryParse(rawArg.Substring(startingIdentifier.Length), out int result))
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        internal static EquipmentIndex? TryGetOptionalEquipmentIndex(this ConCommandArgs args, string startingIdentifier)
        {
            string? optionalString = args.TryGetOptionalString(startingIdentifier);
            if (optionalString == null)
                return null;

            EquipmentIndex equipmentIndex = EquipmentCatalog.FindEquipmentIndex(optionalString);
            return equipmentIndex;
        }
    }
}