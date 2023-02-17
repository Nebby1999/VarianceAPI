using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VAPI.Components;

namespace VAPI
{
    /// <summary>
    /// An extended version of the game's <see cref="DirectorSpawnRequest"/>, which can allow you to request a specific variant spawn using the game's Director system
    /// </summary>
    public class VariantDirectorSpawnRequest : DirectorSpawnRequest
    {
        /// <summary>
        /// The VariantDefs that'll be given to the variant when spawned
        /// </summary>
        public VariantDef[] variantDefs = Array.Empty<VariantDef>();
        /// <summary>
        /// Wether or not to apply the variants to the Body on Start
        /// </summary>
        public bool applyOnStart;
        /// <summary>
        /// Wether or not to supress the reward for the summoned Body
        /// </summary>
        public bool supressRewards;
        public VariantDirectorSpawnRequest(SpawnCard spawnCard, DirectorPlacementRule placementRule, Xoroshiro128Plus rng) : base(spawnCard, placementRule, rng)
        {
        }

        [SystemInitializer]
        private static void SystemInit()
        {
            On.RoR2.DirectorCore.TrySpawnObject += HandleCustomRequest;
        }

        private static GameObject HandleCustomRequest(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            var resultingMasterObject = orig(self, directorSpawnRequest);
            try
            {
                if(directorSpawnRequest is VariantDirectorSpawnRequest variantInfo && resultingMasterObject)
                {
                    var characterMaster = resultingMasterObject.GetComponent<CharacterMaster>();
                    var bodyObject = characterMaster.bodyInstanceObject;
                    bodyObject.AddComponent<DoNotTurnIntoVariant>();

                    var bodyVariantManager = bodyObject.GetComponent<BodyVariantManager>();
                    var bodyVariantReward = bodyObject.GetComponent<BodyVariantReward>();

                    if(bodyVariantManager && NetworkServer.active)
                    {
                        bodyVariantManager.AddVariants(variantInfo.variantDefs);
                        bodyVariantManager.applyOnStart = variantInfo.applyOnStart;
                    }
                    if(bodyVariantReward)
                    {
                        if(variantInfo.supressRewards)
                        {
                            bodyVariantReward.AddVariants(variantInfo.variantDefs);
                        }
                        bodyVariantReward.applyOnStart = variantInfo.applyOnStart;
                    }
                }
            }
            catch(Exception e)
            {
                VAPILog.Error($"Exception on DirectorCore.TrySpawnObject hook for VariantDirectorSpawnRequest: {e}");
            }
            return resultingMasterObject;
        }
    }
}