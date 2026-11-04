#nullable enable
using HG;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A class that provides <see cref="CharacterVariantDef"/>s for <see cref="BodyIndex"/>es
    /// </summary>
    public sealed class CharacterVariantProvider
    {
        /// <summary>
        /// The total amount of variants stored in this provider
        /// </summary>
        public int totalVariantCount => allVariants.Length;

        /// <summary>
        /// A ReadOnlyArray of all the variants stored in this provider
        /// </summary>
        public ReadOnlyArray<CharacterVariantDef> allVariants => _allVariants;
        private readonly CharacterVariantDef[] _allVariants;

        /// <summary>
        /// The <see cref="BodyIndex"/> associated with this provider, the <see cref="CharacterVariantDef"/>s stored here is for this body
        /// </summary>
        public readonly BodyIndex associatedBodyIndex;

        /// <summary>
        /// A ReadOnlyList of the <see cref="CharacterVariantDef"/>s that are unique and can spawn on the current stage
        /// </summary>
        public ReadOnlyList<CharacterVariantDef> filteredUniqueVariants => _filteredUniqueVariants;
        private readonly List<CharacterVariantDef> _filteredUniqueVariants;

        /// <summary>
        /// A ReadOnlyList of the <see cref="CharacterVariantDef"/>s that are not unique and can spawn on the current stage
        /// </summary>
        public ReadOnlyList<CharacterVariantDef> filteredNonUniqueVariants => _filteredNonUniqueVariants;
        private readonly List<CharacterVariantDef> _filteredNonUniqueVariants;

        /// <summary>
        /// Filtes the variant for the current stage, this is called automatically on stage start
        /// </summary>
        public void FilterVariants()
        {
            _filteredNonUniqueVariants.Clear();
            _filteredUniqueVariants.Clear();

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

        /// <summary>
        /// Defines a roll type to use.
        /// </summary>
        public enum RollType
        { 
            /// <summary>
            /// Unique variants are rolled first, if no uniques are picked, then non uniques are rolled.
            /// </summary>
            Default,
            /// <summary>
            /// Only roll unique variants
            /// </summary>
            Unique,
            /// <summary>
            /// Only roll non unique variants
            /// </summary>
            NonUnique
        }

        /// <summary>
        /// Arguments for rolling the <see cref="CharacterVariantDef"/> for a character
        /// </summary>
        public struct RollVariantDefsArgs
        {
            /// <summary>
            /// The RNG instance used in the rolling
            /// </summary>
            public Xoroshiro128Plus rng;

            /// <summary>
            /// A multiplier applied to the spawn chance of the variant
            /// </summary>
            public float? spawnChanceMultiplier;

            /// <summary>
            /// The <see cref="RollType"/> to use, defaults to <see cref="RollType.Default"/>
            /// </summary>
            public RollType? rollType;

            /// <summary>
            /// an array of VariantDefs that are guaranteed to be included on the final array.
            /// </summary>
            public CharacterVariantDef[]? additiveVariantDefs;
        }

        /// <summary>
        /// Rolls an array of <see cref="CharacterVariantDef"/>s from this provider.
        /// <br></br>
        /// If a Unique variant is rolled, only the unique variant will be returned. If no unique variants are rolled, then multiple variants may be selected, causing a merged variant
        /// </summary>
        /// <param name="args">The arguments for rolling the variants</param>
        public CharacterVariantDef[] RollVariantDefs(RollVariantDefsArgs args)
        {
            Xoroshiro128Plus rng = args.rng;
            float spawnChanceMultiplier = args.spawnChanceMultiplier ?? 1f;
            RollType rollType = args.rollType ?? RollType.Default;
            CharacterVariantDef[]? additiveVariantDefs = args.additiveVariantDefs;

            CharacterVariantDef[] resultFromProcedure;
            switch(rollType)
            {
                case RollType.Default:
                    DefaultRollProcedure(rng, spawnChanceMultiplier, out resultFromProcedure);
                    break;
                case RollType.Unique:
                    UniqueRollProcedure(rng, spawnChanceMultiplier, out resultFromProcedure);
                    break;
                case RollType.NonUnique:
                    NonUniqueRollProcedure(rng, spawnChanceMultiplier, out resultFromProcedure);
                    break;
                default:
                    resultFromProcedure = Array.Empty<CharacterVariantDef>();
                    break;
            }

            if(additiveVariantDefs == null)
            {
                return resultFromProcedure;
            }

            using var _ = ListPool<CharacterVariantDef>.RentCollection(out var resultWithAdditives);
            resultWithAdditives.AddRange(additiveVariantDefs);

            foreach(var result in resultFromProcedure)
            {
                if(!resultWithAdditives.Contains(result))
                {
                    resultWithAdditives.Add(result);
                }
            }

            return resultWithAdditives.ToArray();
        }

        private void DefaultRollProcedure(Xoroshiro128Plus rng, float spawnChanceMultiplier, out CharacterVariantDef[] resultFromProcedure)
        {
            if (filteredUniqueVariants.Count > 0 && RollUniques(rng, spawnChanceMultiplier, out CharacterVariantDef? uniqueResult))
            {
                resultFromProcedure = new CharacterVariantDef[] { uniqueResult! };
                return;
            }

            if (filteredNonUniqueVariants.Count > 0 && RollNonUniques(rng, spawnChanceMultiplier, out CharacterVariantDef[]? results))
            {
                resultFromProcedure = results!;
                return;
            }

            resultFromProcedure = Array.Empty<CharacterVariantDef>();
        }

        private void UniqueRollProcedure(Xoroshiro128Plus rng, float spawnChanceMultiplier, out CharacterVariantDef[] resultFromProcedure)
        {
            if (filteredUniqueVariants.Count > 0 && RollUniques(rng, spawnChanceMultiplier, out CharacterVariantDef? uniqueResult))
            {
                resultFromProcedure = new CharacterVariantDef[] { uniqueResult! };
                return;
            }

            resultFromProcedure = Array.Empty<CharacterVariantDef>();
        }

        private void NonUniqueRollProcedure(Xoroshiro128Plus rng, float spawnChanceMultiplier, out CharacterVariantDef[] resultFromProcedure)
        {
            if (filteredNonUniqueVariants.Count > 0 && RollNonUniques(rng, spawnChanceMultiplier, out CharacterVariantDef[]? results))
            {
                resultFromProcedure = results!;
                return;
            }

            resultFromProcedure = Array.Empty<CharacterVariantDef>();
        }

        private readonly WeightedSelection<int> uniqueVariantIndexSelector = new WeightedSelection<int>();
        private bool RollUniques(Xoroshiro128Plus spawnRng, float spawnChanceMultiplier, out CharacterVariantDef? uniqueResult)
        {
            //Clear the index selector
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

        private bool RollNonUniques(Xoroshiro128Plus spawnRng, float spawnChanceMultiplier, out CharacterVariantDef[]? results)
        {
            using var _ = ListPool<CharacterVariantDef>.RentCollection(out var notUniquesPick);

            for (int i = 0; i < filteredNonUniqueVariants.Count; i++)
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

        internal CharacterVariantProvider(CharacterVariantDef[] variantDefsForCharacter, BodyIndex associatedBodyIndex)
        {
            _allVariants = variantDefsForCharacter;
            this.associatedBodyIndex = associatedBodyIndex;

            _filteredNonUniqueVariants = new List<CharacterVariantDef>();
            _filteredUniqueVariants = new List<CharacterVariantDef>();
        }
    }
}