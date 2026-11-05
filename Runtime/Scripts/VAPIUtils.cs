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
        /// Struct used for the arguments of <see cref="TransformSpawnedInstanceIntoVariant(TransformSpawnedInstanceIntoVariantArgs)"/>
        /// </summary>
        public struct TransformSpawnedInstanceIntoVariantArgs
        {
            /// <summary>
            /// The spawned instance to turn into a variant
            /// </summary>
            public GameObject? spawnedInstance;

            /// <summary>
            /// If true, the arrival messages and sfx are supressed. <b>Defaults to false</b>
            /// </summary>
            public bool? supressArrivalMessage;
            
            /// <summary>
            /// An array of <see cref="CharacterVariantDef"/> to set to the body. If left null, the <see cref="VariantSpawnManager"/> will roll for the character's variants. <b>Defaults to null</b>
            /// </summary>
            public CharacterVariantDef[]? preassignedVariantDefs;
            /// <summary>
            /// If true, the <see cref="preassignedVariantDefs"/> are considered additive. <see cref="VariantSpawnManager"/> will roll for variants to add on top of the ones preassigned. <b>Defaults to false</b>
            /// </summary>
            public bool? preassignedVariantDefsAreAdditive;
            /// <summary>
            /// A multiplied that's applied on top of any existing spawn rate multipliers. <b>Defaults to 1</b>
            /// </summary>
            public float? spawnManagerMultiplier;
            
            /// <summary>
            /// If supplied, the DeathRewards for the spawned variant will recieve this's gold and exp rewards. <b>Defaults to null</b>
            /// </summary>
            public DeathRewards? baseDeathRewards;
            /// <summary>
            /// If <see cref="baseDeathRewards"/> is not null, the base gold and exp rewards are multiplied by this value. <b>Defaults to 1</b>
            /// </summary>
            public float? deathRewardsCoefficient;
            
            /// <summary>
            /// If true, the spawned variant will not drop items. <b>Defaults to false</b>
            /// </summary>
            public bool? noItemRewards;

            /// <summary>
            /// If true, the spawned variant will not recieve any gold or exp modifications. <b>Defaults to false</b>
            /// </summary>
            public bool? noGoldExpModifications;
        }

        /// <summary>
        /// Method utilized for transforming a newly spawned character into a Variant. Both <see cref="VariantMasterSummon"/> and <see cref="VariantDirectorSpawnRequest"/> utilize this method.
        /// <br></br>
        /// See <see cref="TransformSpawnedInstanceIntoVariantArgs"/> for more information
        /// </summary>
        /// <param name="args">The arguments for this method</param>
        public static void TransformSpawnedInstanceIntoVariant(TransformSpawnedInstanceIntoVariantArgs args)
        {
            GameObject? spawnedInstance = args.spawnedInstance;
            CharacterVariantDef[]? preAssignedVariantDefs = args.preassignedVariantDefs;
            DeathRewards? baseDeathRewards = args.baseDeathRewards;
            float deathRewardsCoefficient = args.deathRewardsCoefficient ?? 1;
            bool supressArrivalMessage = args.supressArrivalMessage ?? false;
            bool noGoldExpModifications = args.noGoldExpModifications ?? false;
            bool noItemRewards = args.noItemRewards ?? false;
            bool preAssignedVariantDefsAreAdditive = args.preassignedVariantDefsAreAdditive ?? false;
            float? spawnManagerMultiplier = args.spawnManagerMultiplier ?? 1;

            if (spawnedInstance == null)
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
                bodyVariantController.supressArrivalMessageServer = supressArrivalMessage;

                /*
                 * Create the pre assigned spawn data struct, assign our multiplier.
                 * If the pre assigned array is meant to be additive, add it to the data, then assign the data to the controller.
                 * 
                 * Finally, if the variant def array is not null and they're not additive, set them directly.
                 */

                PreAssignedSpawnData preAssignedSpawnData = new PreAssignedSpawnData { spawnRateMultiplier = spawnManagerMultiplier, additiveVariantDefs = preAssignedVariantDefsAreAdditive ? preAssignedVariantDefs : null };
                bodyVariantController.preAssignedSpawnDataServer = preAssignedSpawnData;

                if(preAssignedVariantDefs != null && !preAssignedVariantDefsAreAdditive)
                {
                    bodyVariantController.SetCharacterVariantDefsServer(preAssignedVariantDefs);
                    bodyVariantController.doNotRollForVariants = true;
                }
            }

            if (spawnedBodyInstance.TryGetComponent<VariantDeathRewards>(out var variantDeathRewards))
            {
                variantDeathRewards.noItemRewardServer = noItemRewards;
                variantDeathRewards.noGoldExpModificationsServer = noGoldExpModifications;
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

            if(VariantSpawnManager.instance == null || VariantSpawnManager.instance.IsVariantAvailable(characterVariantDef.characterVariantIndex) == false)
            {
                return false;
            }

            return characterVariantDef.spawnCondition?.IsAvailable() ?? true;
        }

        public static AssetReferenceT<T> CloneAssetReferenceT<T>(AssetReferenceT<T>? assetRef) where T : UnityEngine.Object
        {
            if(assetRef == null)
            {
                return new AssetReferenceT<T>("");
            }

            var guid = assetRef.AssetGUID;
            var subObjectName = assetRef.SubObjectName;

            var result = new AssetReferenceT<T>(guid);
            result.SubObjectName = subObjectName;
            return result;
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