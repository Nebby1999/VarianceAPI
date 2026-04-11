#nullable enable
using HG;
using R2API.AddressReferencedAssets;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    public static class VAPIUtils
    {
        public static BodyIndex GetBodyIndex(string bodyName)
        {
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

        public static MasterCatalog.MasterIndex GetMasterIndex(string masterName)
        {
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

        public static void ModifyMasterToBecomeVariant(GameObject masterObject, CharacterVariantDef[] variantDefs, DeathRewards? baseDeathRewards = null, float deathRewardsCoefficient = 1f)
        {
            if (!masterObject || variantDefs == null)
                return;

            if (!NetworkServer.active)
                return;

            if(!masterObject.TryGetComponent<CharacterMaster>(out var master))
            {
                return;
            }

            if(master.TryGetComponent<CharacterMasterVariantStorage>(out var variantStorage))
            {
                variantStorage.SetVariantDefsForCharacter(variantDefs);
                variantStorage.doNotRollForVariants = true;
            }

            var body = master.GetBody();
            if(!body)
            {
                return;
            }

            if(baseDeathRewards && body.TryGetComponent<DeathRewards>(out var deathRewards))
            {
                deathRewards.expReward = (uint)(baseDeathRewards!.expReward * deathRewardsCoefficient);
                deathRewards.goldReward = (uint)(baseDeathRewards!.goldReward * deathRewardsCoefficient);
            }

            if(body.TryGetComponent<CharacterBodyVariantController>(out var bodyVariantController) && !variantStorage)
            {
                bodyVariantController.SetFallbackCharacterVariantDefs(variantDefs);
                bodyVariantController.doNotRollForVariants = true;
            }
        }
    }
    public static partial class Extensions
    {
        public static bool CheckRoll0To1(this Xoroshiro128Plus rng, float zeroToOnceChance, float luck = 0f, CharacterMaster? effectOriginMaster = null)
        {
            return CheckRoll(rng, zeroToOnceChance * 100f, luck, effectOriginMaster);
        }

        public static bool CheckRoll(this Xoroshiro128Plus rng, float percentchance, float luck = 0f, CharacterMaster? effectOriginMaster = null)
        {
            if(rng == null)
            {
                return false;
            }

            CharacterBody? characterBody = effectOriginMaster.AsValidOrNull()?.GetBody();
            if (percentchance <= 0f)
            {
                return false;
            }

            bool wasLucky = false;
            int rerollCount = Mathf.CeilToInt(Mathf.Abs(luck));
            float randomValue = rng.RangeFloat(0f, 100f);
            float firstRollValue = randomValue;
            for(int i = 0; i < rerollCount; i++)
            {
                float rerollValue = Random.Range(0f, 100f);
                randomValue = (luck > 0) ? Mathf.Min(randomValue, rerollValue) : Mathf.Max(randomValue, rerollValue);
            }

            if(randomValue <= percentchance)
            {
                wasLucky = firstRollValue > percentchance;
                if(characterBody)
                {
                    characterBody!.wasLucky = wasLucky;
                }
                return true;
            }
            return false;
        }

        public static GenericSkill? GetOrFindSkill(this SkillLocator locator, SkillSlot slot, string fallbackSkillName)
        {
            if(slot == SkillSlot.None)
            {
                return locator.FindSkill(fallbackSkillName);
            }
            else
            {
                return locator.GetSkill(slot);
            }
        }
    }
}