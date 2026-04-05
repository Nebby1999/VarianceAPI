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

        /*
         * We want to wrap the BodySplitter's MasterSummon inside a VariantMasterSummon to ensure Gup variants are passed into it's children.
         * 
         * We will do this by putting our cursor right before the Perform call, and then creating a VariantMasterSummon wrapper.
         * 
         *  bodySplitter.moneyMultiplier = moneyMultiplier;
         *  <---- ILHook cursor goes here.
		 *  bodySplitter.Perform();
         */
        internal static void HandleDeathState(ILContext il)
        {
            var cursor = new ILCursor(il);

            //First we'll get our cursor right before BodySplitter perform.
            bool matchBeforeBodySplitterPerform = cursor.TryGotoNext(x => x.MatchCallOrCallvirt<BodySplitter>(nameof(BodySplitter.Perform)));

            if(!matchBeforeBodySplitterPerform)
            {
                return;
            }

            //Emit the BaseSplitDeath instance
            cursor.Emit(OpCodes.Ldarg_0);

            //Emit the delegate, which will put the BodySplitter back in the stack... The original method expects it to be there for the perform call, so this should work just fine i think.
            cursor.EmitDelegate<Func<BodySplitter, BaseSplitDeath, BodySplitter>>(MakeBodySplitterIntoAVariantSummon);
        }

        private static BodySplitter MakeBodySplitterIntoAVariantSummon(BodySplitter toReturn, BaseSplitDeath baseSplitDeath)
        {
            if (!baseSplitDeath.characterBody)
                return toReturn;

            if (!baseSplitDeath.characterBody.TryGetComponent<CharacterBodyVariantController>(out var bodyVariantController))
                return toReturn;

            VariantMasterSummon wrapper = new VariantMasterSummon(toReturn.masterSummon)
            {
                deathRewardsCoefficient = 0.3f,
                summonerDeathRewards = baseSplitDeath.characterBody.GetComponent<DeathRewards>(),
                variantDefs = GetVariantDefs(bodyVariantController.characterVariantDefs, toReturn.masterSummon.masterPrefab, baseSplitDeath.characterBody.bodyIndex)
            };
            return toReturn;
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