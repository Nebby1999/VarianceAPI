using EntityStates.Gup;
using IL.RoR2.Items;
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
            void HandleDeath(BodySplitter splitter, BaseSplitDeath baseSplitDeath)
            {
                if (!baseSplitDeath.characterBody)
                    return;

                if (!baseSplitDeath.characterBody.TryGetComponent<CharacterBodyVariantController>(out var bodyVariantController))
                    return;

                var originalMasterSummon = splitter.masterSummon;
                var newSummon = new VariantSummon
                {
                    masterPrefab = originalMasterSummon.masterPrefab,
                    ignoreTeamMemberLimit = false,
                    useAmbientLevel = null,
                    teamIndexOverride = null,
                    variantDefs = GetVariantDefs(bodyVariantController.variantsForBody, originalMasterSummon.masterPrefab, baseSplitDeath.characterBody.bodyIndex)
                };
                splitter.masterSummon = originalMasterSummon;
            }
        }

        private static List<CharacterVariantDef> _getVariantDefsBuffer = new List<CharacterVariantDef>();
        private static CharacterVariantDef[] GetVariantDefs(NetworkedVariantCollection splittingBodyVariantCollection, GameObject masterPrefabToSplitTo, BodyIndex splittingBodyIndex)
        {
            _getVariantDefsBuffer.Clear();

            BodyIndex splittingResultBodyIndex = BodyIndex.None;
            if(masterPrefabToSplitTo.TryGetComponent<CharacterMaster>(out var masterToSplitTo) 
                && masterToSplitTo.bodyPrefab
                && masterToSplitTo.bodyPrefab.TryGetComponent<CharacterBody>(out var bodyToSplitTo))
            {
                splittingResultBodyIndex = bodyToSplitTo.bodyIndex;
            }

            foreach(var variantDef in splittingBodyVariantCollection)
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