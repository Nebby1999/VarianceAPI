#nullable enable
using HG;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    public sealed class CharacterVariantProvider
    {
        public int totalVariantCount => allVariants.Length;
        public ReadOnlyArray<CharacterVariantDef> allVariants => _allVariants;
        private readonly CharacterVariantDef[] _allVariants;
        public readonly BodyIndex associatedBodyIndex;
        public readonly MasterCatalog.MasterIndex associatedMasterIndex;

        //These are filtered per stage.
        public ReadOnlyList<CharacterVariantDef> filteredUniqueVariants => _filteredUniqueVariants;
        private readonly List<CharacterVariantDef> _filteredUniqueVariants;
        public ReadOnlyList<CharacterVariantDef> filteredNonUniqueVariants => _filteredNonUniqueVariants;
        private readonly List<CharacterVariantDef> _filteredNonUniqueVariants;

        public void FilterVariants()
        {
            for(int i = 0; i < totalVariantCount; i++)
            {
                var variant = _allVariants[i];

                if(!variant.IsAvailable())
                {
                    continue;
                }

                if(variant.isUnique)
                {
                    _filteredUniqueVariants.Add(variant);
                }
                else
                {
                    _filteredNonUniqueVariants.Add(variant);
                }
            }
        }

        public struct RollVariantDefsArgs
        {
            public Xoroshiro128Plus rng;
            public float? spawnChanceMultiplier;
        }

        public CharacterVariantDef[]? RollVariantDefs(RollVariantDefsArgs args)
        {
            if(filteredUniqueVariants.Count > 0 && RollUniques(args.rng, args.spawnChanceMultiplier ?? 1f, out CharacterVariantDef? uniqueResult))
            {
                return new CharacterVariantDef[] { uniqueResult! };
            }

            if(filteredNonUniqueVariants.Count > 0 && RollNonUniques(args.rng, args.spawnChanceMultiplier ?? 1f, out CharacterVariantDef[] results))
            {
                return results!;
            }

            return null;
        }

        private readonly WeightedSelection<int> uniqueVariantIndexSelector = new WeightedSelection<int>();
        private bool RollUniques(Xoroshiro128Plus spawnRng, float spawnChanceMultiplier, out CharacterVariantDef? uniqueResult)
        {
            uniqueVariantIndexSelector.Clear();
            float notUniqueChance = 0;
            for(int i = 0; i < filteredUniqueVariants.Count; i++)
            {
                CharacterVariantDef variant = filteredUniqueVariants[i];
                float spawnChance = variant.spawnRate * spawnChanceMultiplier;
                uniqueVariantIndexSelector.AddChoice(i, Mathf.Min(100, spawnChance));
                notUniqueChance += Mathf.Max(0, 100 - spawnChance);
            }
            uniqueVariantIndexSelector.AddChoice(-1, notUniqueChance);

            var uniqueVariantIndex = uniqueVariantIndexSelector.Evaluate(spawnRng.nextNormalizedFloat);
            bool success = uniqueVariantIndex != -1;

            uniqueResult = success ? filteredUniqueVariants[uniqueVariantIndex] : null;
            return success;
        }

        private readonly List<CharacterVariantDef> notUniquesPick = new List<CharacterVariantDef>();
        private bool RollNonUniques(Xoroshiro128Plus spawnRng, float spawnChanceMultiplier, out CharacterVariantDef[]? results)
        {
            notUniquesPick.Clear();
            for(int i = 0; i < filteredNonUniqueVariants.Count; i++)
            {
                CharacterVariantDef variant = filteredNonUniqueVariants[i];
                var spawnRate = Mathf.Min(100, variant.spawnRate * spawnChanceMultiplier);
                if (spawnRate <= 0)
                    continue;

                if(spawnRng.RangeFloat(0, 100) <= spawnRate)
                {
                    notUniquesPick.Add(variant);
                }
            }

            bool success = notUniquesPick.Count != 0;
            results = notUniquesPick.ToArray();
            return success;
        }

        public CharacterVariantProvider(CharacterVariantDef[] variantDefsForCharacter, BodyIndex? associatedBodyIndex, MasterCatalog.MasterIndex? associatedMasterIndex)
        {
            _allVariants = variantDefsForCharacter;
            this.associatedBodyIndex = associatedBodyIndex ?? BodyIndex.None;
            this.associatedMasterIndex = associatedMasterIndex ?? MasterCatalog.MasterIndex.none;

            _filteredNonUniqueVariants = new List<CharacterVariantDef>();
            _filteredUniqueVariants = new List<CharacterVariantDef>();
        }
    }
}