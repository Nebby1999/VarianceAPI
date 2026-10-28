using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    internal static class GhostVariantReplication
    {
        [SystemInitializer]
        private static void Init()
        {
            IL.RoR2.Util.TryToCreateGhost += GhostCopyOverVariants;
            IL.RoR2.NetworkedAffixDeathBehaviour.TrySpawnGhostBodyViaMasterSummon += StygianCopyOverVariantsMasterSummon;

            //This one doesnt work
            //IL.RoR2.NetworkedAffixDeathBehaviour.TrySpawnGhostBodyViaMasterCopySpawnCard += StygianCopyOVerVariantDirectorSpawnRequest;
        }

        // We want to wrap the DirectorSpawnRequest from NetworkedAffixDeathBehaviour.TrySpawnGhostBodyViaMasterCopySpawnCard inside a VariantDirectorSpawnRequest to ensure the variants are passed into the stygian ghost.
        // 
        // We'll need to retrieve the list of variants from the "sourceBody" arg's master storage, so we can shortcircuit if the master storage doesnt exist.
        // 
        // We'll do this by putting our cursor right before the MasterSummon.Perform() call, and then creating the VariantSummon wrapper if applicable.
        // 
        // DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest (masterCopySpawnCard, placementRule, DLC4ProgressionController.instance.rng)
        // {
        //     teamIndexOverride = attachedBody.teamComponent.teamIndex,
        //     ignoreTeamMemberLimit = true,
        //     summonerBodyObject = attachedBody.gameObject
        // };
        // <--- ILhook Cursor Goes here
        // GameObject gameObject = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        private static void StygianCopyOVerVariantDirectorSpawnRequest(ILContext il)
        {
            static DirectorSpawnRequest TurnStygianEliteMonsterGhostIntoVariant(DirectorSpawnRequest spawnRequest, CharacterBody sourceBody)
            {
                //Shortcircuit if no variant controller
                if (!sourceBody.TryGetComponent<CharacterBodyVariantController>(out var variantController))
                {
                    return spawnRequest;
                }

                //Shortcircuit if no master storage
                CharacterMasterVariantStorage variantStorage = variantController.GetCharacterMasterVariantStorage();
                if (variantStorage == null)
                    return spawnRequest;

                if (variantStorage.TryGetVariantsStoredForCharacter(sourceBody.bodyIndex, out var variants))
                {
                    //Encapsulate master summon with our variant master summon, set variants.
                    //I dont think this will be garbage collected immediatly so... :wario:
                    new VariantDirectorSpawnRequest(spawnRequest)
                    {
                        variantDefs = variants,
                    };
                }
                //Return the master summon
                return spawnRequest;
            }

            var cursor = new ILCursor(il);

            bool matchBeforeDirectorCoreTrySpawnObject = cursor.TryGotoNext(MoveType.Before, x =>
                x.MatchCallOrCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject)));

            if(!matchBeforeDirectorCoreTrySpawnObject)
            {
                VAPILog.Fatal($"Failed to hook NetworkedAffixDeathBehaviour.TrySpawnGhostBodyViaMasterCopySpawnCard(). CharacterVariantDefs won't be propagated to player stygian elite ghosts!");
                return;
            }

            //The DirectorSpawnRequest is on top of our stack
            //Emit load the source body, Ldarg_0 is the NetworkedAffixDeathBehaviour instance.
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Func<DirectorSpawnRequest, CharacterBody, DirectorSpawnRequest>>(TurnStygianEliteMonsterGhostIntoVariant);
        }

        // We want to wrap the MasterSummon from NetworkedAffixDeathBehaviour.TrySpawnGhostBodyViaMasterSummon inside a VariantMasterSummon to ensure the variants are passed into the stygian ghost.
        // 
        // We'll need to retrieve the list of variants from the "sourceBody" arg's master storage, so we can shortcircuit if the master storage doesnt exist.
        // 
        // We'll do this by putting our cursor right before the MasterSummon.Perform() call, and then creating the VariantSummon wrapper if applicable.
        // 
        // CharacterMaster characterMaster = new MasterSummon
        // {
        //     masterPrefab = gameObject,
        //     ignoreTeamMemberLimit = false,
        //     teamIndexOverride = sourceBody.teamComponent.teamIndex,
        //     position = sourceBody.footPosition,
        //     rotation = rotation,
        //     summonerBodyObject = attachedBody.gameObject,
        //     inventoryToCopy = sourceBody.inventory,
        //     preSpawnSetupCallback = PreSpawnSetupCallback
        // }
        // <--- ILHook Cursor goes here
        // .Perform();
        private static void StygianCopyOverVariantsMasterSummon(ILContext il)
        {
            static MasterSummon TurnStygianEliteMonsterGhostIntoVariant(MasterSummon masterSummon, CharacterBody sourceBody)
            {
                //Shortcircuit if no variant controller
                if (!sourceBody.TryGetComponent<CharacterBodyVariantController>(out var variantController))
                {
                    return masterSummon;
                }

                //Shortcircuit if no master storage
                CharacterMasterVariantStorage variantStorage = variantController.GetCharacterMasterVariantStorage();
                if (variantStorage == null)
                    return masterSummon;

                if (variantStorage.TryGetVariantsStoredForCharacter(sourceBody.bodyIndex, out var variants))
                {
                    //Encapsulate master summon with our variant master summon, set variants.
                    //I dont think this will be garbage collected immediatly so... :wario:
                    new VariantMasterSummon(masterSummon)
                    {
                        variantDefs = variants
                    };
                }
                //Return the master summon
                return masterSummon;
            }

            var cursor = new ILCursor(il);

            bool matchBeforeMasterSummonPerform = cursor.TryGotoNext(MoveType.Before, x =>
                x.MatchCallOrCallvirt<MasterSummon>(nameof(MasterSummon.Perform)));

            if(!matchBeforeMasterSummonPerform)
            {
                VAPILog.Fatal($"Failed to hook NetworkedAffixDeathBehaviour.TrySpawnGhostBodyViaMasterSummon(). CharacterVariantDefs won't be propagated to Monster Stygian Elite Ghosts!");
                return;
            }

            //The master summon is on top of our stack
            //Emit load the source body, Ldarg_0 is the NetworkedAffixDeathBehaviour instance
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Func<MasterSummon, CharacterBody, MasterSummon>>(TurnStygianEliteMonsterGhostIntoVariant);
        }

        /// <summary>
        /// We want to wrap the MasterSummon from the Util.TryToCreateGhost inside a VariantMasterSummon to ensure the Variants are passed onto the ghost.
        /// <br></br>
        /// We'll need to retrieve the list of variants from the "targetBody" arg's master storage, so we can shortcircuit if the master storage doesnt exist.
        /// <para></para>
        /// We'll do this by putting our cursor right before the targetBody.GetComponentCharacterDirection() call, and then creating the VariantSummon wrapper if applicable.
        /// <para></para>
        /// <code>
        /// MasterSummon obj = new MasterSummon
	    /// {
		///     masterPrefab = characterMaster.gameObject,
		///     ignoreTeamMemberLimit = false,
		///     position = targetBody.footPosition
        /// };
        /// <--- ILHook Cursor goes here
        /// CharacterDirection component = targetBody.GetComponentCharacterDirection();
        /// </code>
        /// </summary>
        private static void GhostCopyOverVariants(MonoMod.Cil.ILContext il)
        {
            static MasterSummon TurnHappiestMaskGhostIntoVariant(MasterSummon summon, CharacterBody targetBody)
            {
                //Shortcircuit if no variant controller
                if (!targetBody.TryGetComponent<CharacterBodyVariantController>(out var variantController))
                {
                    return summon;
                }

                //Shortcircuit if no master storage
                CharacterMasterVariantStorage variantStorage = variantController.GetCharacterMasterVariantStorage();
                if (variantStorage == null)
                    return summon;

                if (variantStorage.TryGetVariantsStoredForCharacter(targetBody.bodyIndex, out var variants))
                {
                    //Encapsulate master summon with our variant master summon, set variants.
                    //I dont think this will be garbage collected immediatly so... :wario:
                    new VariantMasterSummon(summon)
                    {
                        variantDefs = variants
                    };
                }
                //Return the master summon
                return summon;
            }

            var cursor = new ILCursor(il);

            //Puts us right before ldArg.0 folowed by the get component call.
            bool matchBeforeGetCharacterDirectionComponent = cursor.TryGotoNext(MoveType.After, x =>
                x.MatchStfld<MasterSummon>(nameof(MasterSummon.position)));

            if (!matchBeforeGetCharacterDirectionComponent)
            {
                VAPILog.Fatal($"Failed to hook Util.TryToCreateGhost(). CharacterVariantDefs won't be propagated to Happiest Mask ghosts!");
                return;
            }

            //the master summon is on top of our stack.
            //Emit load the target body
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<MasterSummon, CharacterBody, MasterSummon>>(TurnHappiestMaskGhostIntoVariant);
        }
    }
}