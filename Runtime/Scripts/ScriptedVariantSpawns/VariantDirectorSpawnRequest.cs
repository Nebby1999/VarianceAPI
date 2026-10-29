#nullable enable

using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    //This class is also very neat, the constructor registers to DirectorSpawnREquest's event, and the deconstructor will unregister.
    //As soon as a DirectorSpawnRequest happens, and the spawn request that happeend is the one stored in this instance, it'll turn it into a vairant.

    /// <summary>
    /// A <see cref="DirectorSpawnRequest"/> class encapsulator that will turn the spawned character into a variant
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
            public readonly CharacterVariantDef[] variantDefs;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="spawnResult">The base <see cref="SpawnCard.SpawnResult"/> from the DirectorSpawnREquest</param>
            /// <param name="variantDefs">The character's variant defs</param>
            public VariantDirectorSpawnResult(SpawnCard.SpawnResult spawnResult, CharacterVariantDef[] variantDefs)
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
        public CharacterVariantDef[] variantDefs = Array.Empty<CharacterVariantDef>();

        /// <summary>
        /// The death rewards from the requester, if supplied the summon will have the same gold and experience reward
        /// </summary>
        public DeathRewards? requesterDeathRewards;

        /// <summary>
        /// If true, the announcement system will not announce the variant's arrival.
        /// </summary>
        public bool supressArrivalMessage;

        /// <summary>
        /// A reward coefficient of <see cref="requesterDeathRewards"/>'s gold and experience amounts
        /// </summary>
        public float deathRewardsCoefficient = 1;

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
                spawnRequest.onSpawnedServer -= OnCharacterSpawnedServer;

                VAPIUtils.TransformSpawnedInstanceIntoVariant(new VAPIUtils.TransformSpawnedInstanceIntoVariantArgs
                {
                    spawnedInstance = spawnCardResult.spawnedInstance,
                    variantDefs = variantDefs,
                    supressArrivalMessage = supressArrivalMessage,
                    baseDeathRewards = requesterDeathRewards,
                    deathRewardsCoefficient = deathRewardsCoefficient
                });
                var variantSpawnResult = new VariantDirectorSpawnResult(spawnCardResult, variantDefs);
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

        ~VariantDirectorSpawnRequest()
        {
            spawnRequest.onSpawnedServer -= OnCharacterSpawnedServer;
        }
    }
}