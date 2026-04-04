#nullable enable

using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    public sealed class VariantDirectorSpawnRequest
    {
        public readonly struct VariantDirectorSpawnResult
        {
            public readonly SpawnCard.SpawnResult spawnResult;
            public readonly CharacterVariantDef[] variantDefs;

            public VariantDirectorSpawnResult(SpawnCard.SpawnResult spawnResult, CharacterVariantDef[] variantDefs)
            {
                this.spawnResult = spawnResult;
                this.variantDefs = variantDefs;
            }
        }

        public readonly DirectorSpawnRequest spawnRequest;

        public CharacterVariantDef[] variantDefs = Array.Empty<CharacterVariantDef>();
        public DeathRewards? requesterDeathRewards;
        public float deathRewardsCoefficient;

        public event Action<VariantDirectorSpawnResult>? onSpawnPerformed;
        public static event Action<VariantDirectorSpawnResult>? onServerVariantSpawnedGlobal;
        
        private void OnCharacterSpawnedServer(SpawnCard.SpawnResult spawnCardResult)
        {
            if(spawnCardResult.spawnRequest == spawnRequest)
            {
                spawnRequest.onSpawnedServer -= OnCharacterSpawnedServer;

                VAPIUtils.ModifyMasterToBecomeVariant(spawnCardResult.spawnedInstance, variantDefs, requesterDeathRewards, deathRewardsCoefficient);
                var variantSpawnResult = new VariantDirectorSpawnResult(spawnCardResult, variantDefs);
                onSpawnPerformed?.Invoke(variantSpawnResult);
                onServerVariantSpawnedGlobal?.Invoke(variantSpawnResult);
            }
        }


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