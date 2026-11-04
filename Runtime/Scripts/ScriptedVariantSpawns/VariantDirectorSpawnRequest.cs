#nullable enable

using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A <see cref="DirectorSpawnRequest"/> class encapsulator that will turn the spawned character into a variant.
    /// <para></para>
    /// This is done by subscribing to the <see cref="DirectorSpawnRequest.onSpawnedServer"/>, and then applying any variants and modifiers.
    /// </summary>
    public sealed class VariantDirectorSpawnRequest
    {
        /// <summary>
        /// The report of the director spawn request
        /// </summary>
        public readonly struct VariantDirectorSpawnResult
        {
            /// <summary>
            /// The base <see cref="SpawnCard.SpawnResult"/> from the DirectorSpawnRequest
            /// </summary>
            public readonly SpawnCard.SpawnResult spawnResult;

            /// <summary>
            /// The <see cref="CharacterVariantDef"/>s that the character has
            /// </summary>
            public readonly CharacterVariantDef[]? variantDefs;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="spawnResult">The base <see cref="SpawnCard.SpawnResult"/> from the DirectorSpawnREquest</param>
            /// <param name="variantDefs">The character's variant defs</param>
            public VariantDirectorSpawnResult(SpawnCard.SpawnResult spawnResult, CharacterVariantDef[]? variantDefs)
            {
                this.spawnResult = spawnResult;
                this.variantDefs = variantDefs;
            }
        }

        /// <summary>
        /// The DirectorSpawnRequest that will be turned into a variant
        /// </summary>
        public readonly DirectorSpawnRequest spawnRequest;

        /// <summary>
        /// The VariantDefs for the character
        /// </summary>
        public CharacterVariantDef[]? preassignedVariantDefs;

        /// <summary>
        /// If true, <see cref="preassignedVariantDefs"/> will be considered "Additive".
        /// <br></br>
        /// The variant spawn manager will still roll for variants, but it is guaranteed that the spawned variant has the variant defs in the array.
        /// </summary>
        public bool preassignedVariantDefsAreAdditive;

        /// <summary>
        /// The SpawnManager's process for rolling variants will multiply all spawn rates by this value.
        /// </summary>
        public float spawnManagerMultiplier = 1;

        /// <summary>
        /// The death rewards from the requester, if supplied the summon will have the same gold and experience reward
        /// </summary>
        public DeathRewards? requesterDeathRewards;

        /// <summary>
        /// A reward coefficient of <see cref="requesterDeathRewards"/>'s gold and experience amounts
        /// </summary>
        public float deathRewardsCoefficient = 1;

        /// <summary>
        /// If true, the announcement system will not announce the variant's arrival.
        /// </summary>
        public bool supressArrivalMessage;

        /// <summary>
        /// If true, when the summoned variant dies it is guaranteed to never drop items
        /// </summary>
        public bool noItemReward;

        /// <summary>
        /// If true, when the summoned variant is spawned it is guaranteed to not recieve any reward modifications from the applied variants.
        /// </summary>
        public bool noGoldExpModifications;

        /// <summary>
        /// Event called when the DirectorSpawnRequest is performed, and the result is turned into a variant
        /// </summary>
        public event Action<VariantDirectorSpawnResult>? onSpawnPerformed;

        /// <summary>
        /// Event called when any DirectorSpawnRequest is performed, and it's result is turned into a variant
        /// </summary>
        public static event Action<VariantDirectorSpawnResult>? onServerVariantSpawnedGlobal;
        
        private void OnCharacterSpawnedServer(SpawnCard.SpawnResult spawnCardResult)
        {
            //If the spawn request that just happened is the one stored in this, unhook from the event and turn the result into a variant
            if(spawnCardResult.spawnRequest == spawnRequest)
            {
                VAPIUtils.TransformSpawnedInstanceIntoVariant(new VAPIUtils.TransformSpawnedInstanceIntoVariantArgs
                {
                    spawnedInstance = spawnCardResult.spawnedInstance,
                    preassignedVariantDefs = preassignedVariantDefs,
                    supressArrivalMessage = supressArrivalMessage,
                    baseDeathRewards = requesterDeathRewards,
                    deathRewardsCoefficient = deathRewardsCoefficient,
                    noItemRewards = noItemReward,
                    noGoldExpModifications = noGoldExpModifications,
                    preassignedVariantDefsAreAdditive = preassignedVariantDefsAreAdditive,
                    spawnManagerMultiplier = spawnManagerMultiplier,
                });
                var variantSpawnResult = new VariantDirectorSpawnResult(spawnCardResult, preassignedVariantDefs);
                onSpawnPerformed?.Invoke(variantSpawnResult);
                onServerVariantSpawnedGlobal?.Invoke(variantSpawnResult);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="spawnRequest">The DirectorSpawnREquest, from which once performed succesfully, will be turned into a variant.</param>
        public VariantDirectorSpawnRequest(DirectorSpawnRequest spawnRequest)
        {
            this.spawnRequest = spawnRequest;
            spawnRequest.onSpawnedServer += OnCharacterSpawnedServer;
        }
    }
}