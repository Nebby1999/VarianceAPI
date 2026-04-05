using EntityStates.Gup;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    public static class GupVariantHelper
    {
        private static HashSet<CharacterVariantDef> _blacklistedVariants = new HashSet<CharacterVariantDef>();

        private static Dictionary<CharacterVariantDef, CharacterVariantDef> _gupToGeepVariantRelation = new Dictionary<CharacterVariantDef, CharacterVariantDef>();
        private static Dictionary<CharacterVariantDef, CharacterVariantDef> _geepToGipVariantRelation = new Dictionary<CharacterVariantDef, CharacterVariantDef>();

        public static void AddVariantToGupBlacklist(CharacterVariantDef def)
        {
            _blacklistedVariants.Add(def);
        }

        public static void AddGupVariantRelation(CharacterVariantDef gupVariant, CharacterVariantDef geepVariant, CharacterVariantDef gipVariant)
        {
            _gupToGeepVariantRelation.TryAdd(gupVariant, geepVariant);
            _geepToGipVariantRelation.TryAdd(geepVariant, gipVariant);
        }

        internal static void HandleDeathState(ILContext il)
        {
            //TODO: analyze health of this ILHook
            var cursor = new ILCursor(il);

            var success = cursor.TryGotoNext(x => x.MatchDup(),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<BaseSplitDeath>(nameof(BaseSplitDeath.moneyMultiplier)),
                x => x.MatchStfld<BodySplitter>(nameof(BodySplitter.moneyMultiplier)));

            if (!success)
            {
                VAPILog.Fatal("Failed to reach specific destination for handling Gup's death states!");
                IL.EntityStates.Gup.BaseSplitDeath.FixedUpdate -= HandleDeathState;
                return;
            }

            cursor.Emit(OpCodes.Dup);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<BodySplitter, BaseSplitDeath>>(HandleDeath);

            void HandleDeath(BodySplitter splitter, BaseSplitDeath baseSplitDeath)
            {
                if (!baseSplitDeath.characterBody)
                    return;

                if (!baseSplitDeath.characterBody.TryGetComponent<CharacterBodyVariantController>(out var bodyVariantController))
                    return;

                VariantMasterSummon wrapper = new VariantMasterSummon(splitter.masterSummon)
                {
                    deathRewardsCoefficient = 0.3f,
                    summonerDeathRewards = baseSplitDeath.characterBody.GetComponent<DeathRewards>(),
                    variantDefs = GetVariantDefs(bodyVariantController.characterVariantDefs, splitter.masterSummon.masterPrefab, baseSplitDeath.characterBody.bodyIndex)
                };
            }
        }

        private static List<CharacterVariantDef> _getVariantDefsBuffer = new List<CharacterVariantDef>();
        private static CharacterVariantDef[] GetVariantDefs(ReadOnlyArray<CharacterVariantDef> splittingBodyVariantDefs, GameObject masterPrefabToSplitTo, BodyIndex splittingBodyIndex)
        {
            _getVariantDefsBuffer.Clear();

            BodyIndex splittingResultBodyIndex = BodyIndex.None;
            if(masterPrefabToSplitTo.TryGetComponent<CharacterMaster>(out var masterToSplitTo) 
                && masterToSplitTo.bodyPrefab
                && masterToSplitTo.bodyPrefab.TryGetComponent<CharacterBody>(out var bodyToSplitTo))
            {
                splittingResultBodyIndex = bodyToSplitTo.bodyIndex;
            }

            foreach(var variantDef in splittingBodyVariantDefs)
            {
                //Do not pass this VariantDef
                if (_blacklistedVariants.Contains(variantDef))
                    continue;

                //If it's splitting into more of itself, add the variantDef directly.
                if(splittingResultBodyIndex == splittingBodyIndex)
                {
                    _getVariantDefsBuffer.Add(variantDef);
                    continue;
                }

                if(_gupToGeepVariantRelation.TryGetValue(variantDef, out var geepVariantDef))
                {
                    _getVariantDefsBuffer.Add(geepVariantDef);
                    continue;
                }

                if(_geepToGipVariantRelation.TryGetValue(variantDef, out var gipVariantDef))
                {
                    _getVariantDefsBuffer.Add(gipVariantDef);
                    continue;
                }
            }

            return _getVariantDefsBuffer.ToArray();
        }
    }
}