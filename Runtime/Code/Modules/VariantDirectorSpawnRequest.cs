using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VAPI.Components;

namespace VAPI
{
    public class VariantDirectorSpawnRequest : DirectorSpawnRequest
    {
        public VariantDef[] variantDefs;
        public bool applyOnStart;
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
            if(directorSpawnRequest is VariantDirectorSpawnRequest variantInfo && resultingMasterObject)
            {
                var characterMaster = resultingMasterObject.GetComponent<CharacterMaster>();
                characterMaster.spawnOnStart = false;
                var bodyObject = characterMaster.Respawn(characterMaster.transform.position, characterMaster.transform.rotation).gameObject;
                bodyObject.AddComponent<DoNotTurnIntoVariant>();

                var bodyVariantManager = bodyObject.GetComponent<BodyVariantManager>();
                var bodyVariantReward = bodyObject.GetComponent<BodyVariantReward>();

                if(bodyVariantManager)
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
            return resultingMasterObject;
        }
    }
}