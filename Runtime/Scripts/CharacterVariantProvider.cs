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
        }

        /// <summary>
        /// Rolls an array of <see cref="CharacterVariantDef"/>s from this provider.
        /// <br></br>
        /// If a Unique variant is rolled, only the unique variant will be returned. If no unique variants are rolled, then multiple variants may be selected, causing a merged variant
        /// </summary>
        /// <param name="args">The arguments for rolling the variants</param>
        public CharacterVariantDef[]? RollVariantDefs(RollVariantDefsArgs args)
        {
            if(filteredUniqueVariants.Count > 0 && RollUniques(args.rng, args.spawnChanceMultiplier ?? 1f, out CharacterVariantDef? uniqueResult))
            {
                return new CharacterVariantDef[] { uniqueResult! };
            }

            if(filteredNonUniqueVariants.Count > 0 && RollNonUniques(args.rng, args.spawnChanceMultiplier ?? 1f, out CharacterVariantDef[]? results))
            {
                return results!;
            }

            return null;
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

        internal CharacterVariantProvider(CharacterVariantDef[] variantDefsForCharacter, BodyIndex associatedBodyIndex)
        {
            _allVariants = variantDefsForCharacter;
            this.associatedBodyIndex = associatedBodyIndex;

            _filteredNonUniqueVariants = new List<CharacterVariantDef>();
            _filteredUniqueVariants = new List<CharacterVariantDef>();
        }
    }
}