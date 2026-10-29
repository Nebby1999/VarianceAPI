#nullable enable
using HG;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static VAPI.VAPIUtils;
using Random = UnityEngine.Random;

namespace VAPI
{
    /// <summary>
    /// Utility methods for VarianceAPI
    /// </summary>
    public static partial class VAPIUtils
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

        public struct TransformSpawnedInstanceIntoVariantArgs
        {
            public GameObject? spawnedInstance;
            public CharacterVariantDef[]? variantDefs;
            public DeathRewards? baseDeathRewards;
            public float? deathRewardsCoefficient;
            public bool? supressArrivalMessage; 
        }

        public static void TransformSpawnedInstanceIntoVariant(TransformSpawnedInstanceIntoVariantArgs args)
        {
            GameObject? spawnedInstance = args.spawnedInstance;
            CharacterVariantDef[]? variantDefs = args.variantDefs;
            DeathRewards? baseDeathRewards = args.baseDeathRewards;
            float deathRewardsCoefficient = args.deathRewardsCoefficient ?? 1;
            bool supressArrivalMessage = args.supressArrivalMessage ?? false;

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

            if(baseDeathRewards != null && spawnedBodyInstance.TryGetComponent<DeathRewards>(out var deathRewards))
            {
                baseDeathRewards.expReward = (uint)(baseDeathRewards!.expReward * deathRewardsCoefficient);
                baseDeathRewards.goldReward = (uint)(baseDeathRewards!.goldReward * deathRewardsCoefficient);
            }

            if(spawnedBodyInstance.TryGetComponent<CharacterBodyVariantController>(out var bodyVariantController))
            {
                bodyVariantController.SetCharacterVariantDefsServer(variantDefs);
                bodyVariantController.doNotRollForVariants = true;
                bodyVariantController.supressArrivalMessageServer = supressArrivalMessage;
            }
        }

        /// <summary>
        /// Returns True if <paramref name="characterVariantDef"/> can spawn under the current run context.
        /// <para></para>
        /// This is done by checking if the VariantDef is enabled on the Run's rulebook, and then returning the value of <paramref name="characterVariantDef"/>'s spawn condition.
        /// <br></br>
        /// If no spawn condition exists and the variant is available in the rulebook, it returns true.
        /// </summary>
        public static bool IsVariantAvailable(CharacterVariantDef? characterVariantDef)
        {
            if (characterVariantDef == null)
                return false;

            if (!Run.instance || !VAPIRuleBook.IsVariantRuleEnabled(characterVariantDef, Run.instance.ruleBook))
            {
                return false;
            }

            return characterVariantDef.spawnCondition?.IsAvailable() ?? true;
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