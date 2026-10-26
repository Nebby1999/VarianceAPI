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
    /// <summary>
    /// Static class that handles proper variant propagation for Gups, Geeps and Gips.
    /// <br></br>
    /// This class is used to check which variants should be used when a Gup/Geep dies, and has to spawn new monsters. The class's utilities goes unused if <see cref="VAPIConfig._modifyGupDeathStates"/> is false.
    /// <para></para>
    /// Say for example you add a new "Gup" type called "Explosive", which explodes and dies when it uses it's spike attack. If you only want "Explosive Gup" to spawn "Explosive Geep", and "Explosive Geep" spawn "Explosive Gip", you can use this class to ensure that's the case.
    /// </summary>
    public static class GupVariantHelper
    {
        private static HashSet<CharacterVariantDef> _blacklistedVariants = new HashSet<CharacterVariantDef>();

        private static Dictionary<CharacterVariantDef, CharacterVariantDef> _gupToGeepVariantRelation = new Dictionary<CharacterVariantDef, CharacterVariantDef>();
        private static Dictionary<CharacterVariantDef, CharacterVariantDef> _geepToGipVariantRelation = new Dictionary<CharacterVariantDef, CharacterVariantDef>();

        /// <summary>
        /// Adds a variant to the gup blacklist.
        /// <br></br>
        /// If a Gup/Geep dies and has <paramref name="def"/> in it's variant defs, it will not be propagated to it's children
        /// </summary>
        /// <param name="def">The variant def to blacklist</param>
        public static void AddVariantToGupBlacklist(CharacterVariantDef def)
        {
            _blacklistedVariants.Add(def);
        }

        /// <summary>
        /// Adds a new relation between 3 variant defs.
        /// <br></br>
        /// If modifyGupDeathStates is true in VAPI's config. When a Gup dies that has <paramref name="gupVariant"/> in it's list. The <paramref name="geepVariant"/> will be added to the resulting geep.
        /// <br></br>
        /// Same thing applies to Geeps dying and spawning Gips
        /// </summary>
        /// <param name="gupVariant">The Gup character variant def</param>
        /// <param name="geepVariant">The Geep character variant def</param>
        /// <param name="gipVariant">The Gip character variant def</param>
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

            //Wrap the master summon with the variant summon, which will ensure the Geep/Gip gets turned into a variant.
            VariantMasterSummon wrapper = new VariantMasterSummon(toReturn.masterSummon)
            {
                deathRewardsCoefficient = 0.3f,
                summonerDeathRewards = baseSplitDeath.characterBody.GetComponent<DeathRewards>(),
                variantDefs = GetVariantDefs(bodyVariantController.GetCharacterVariantDefs(), toReturn.masterSummon.masterPrefab, baseSplitDeath.characterBody.bodyIndex)
            };
            return toReturn;
        }

        private static CharacterVariantDef[] GetVariantDefs(ReadOnlyArray<CharacterVariantDef> splittingBodyVariantDefs, GameObject masterPrefabToSplitTo, BodyIndex splittingBodyIndex)
        {
            using var _ = ListPool<CharacterVariantDef>.RentCollection(out var getVariantDefsBuffer);

            getVariantDefsBuffer.Clear();

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
                    getVariantDefsBuffer.Add(variantDef);
                    continue;
                }

                if(_gupToGeepVariantRelation.TryGetValue(variantDef, out var geepVariantDef))
                {
                    getVariantDefsBuffer.Add(geepVariantDef);
                    continue;
                }

                if(_geepToGipVariantRelation.TryGetValue(variantDef, out var gipVariantDef))
                {
                    getVariantDefsBuffer.Add(gipVariantDef);
                    continue;
                }
            }

            return getVariantDefsBuffer.ToArray();
        }
    }
}