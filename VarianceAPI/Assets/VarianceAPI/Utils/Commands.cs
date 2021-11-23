using DebugToolkit;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using VarianceAPI.Components;
using VarianceAPI.ScriptableObjects;

namespace VarianceAPI.Utils
{
    public static class Commands
    {
        [ConCommand(commandName = "list_modified_bodies", flags = ConVarFlags.None, helpText = "List all the bodies that are modified in the VariantRegister.")]
        public static void ListModifiedBodies(ConCommandArgs args)
        {
            List<string> toLog = new List<string>();
            toLog.Add("Modified Bodies:");
            toLog.Add("----------------");
            foreach (var kvp in VariantRegister.RegisteredVariants)
            {
                toLog.Add(kvp.Key);
            }

            Debug.Log(string.Join("\n", toLog));
        }

        [ConCommand(commandName = "list_variants_from_body", flags = ConVarFlags.None, helpText = "Lists all the VariantInfos that a body contains.\nargs[0] = bodyName")]
        public static void ListAllVariantsFromBody(ConCommandArgs args)
        {
            if (args.Count == 0)
            {
                Debug.Log("No arguments given.");
                return;
            }

            string character = StringFinder.Instance.GetBodyName(args[0]);
            if (character == null)
            {
                Debug.Log("No body could be found with that name. to get a list of bodies that have variants, use list_modified_bodies.");
                return;
            }

            if (VariantRegister.RegisteredVariants.TryGetValue(character, out var list))
            {
                List<string> toLog = new List<string>();
                toLog.Add($"{character}'s variants.");
                toLog.Add($"-----------------------");
                foreach (var thing in list)
                {
                    toLog.Add($"{thing.name} - {thing.identifier}");
                }
                Debug.Log(string.Join("\n", toLog));
            }
            else
            {
                Debug.Log("Body inputted has no variants. to get a list of bodies that have variants, use list_modified_bodies.");
                return;
            }
        }

        [ConCommand(commandName = "spawn_variant", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawns a/the specific(s) variant(s).\nArg[0] = Master name\nArg[1 - Infinity] VariantInfo's Identifier.")]
        public static void SpawnVariant(ConCommandArgs args)
        {
            if (args.sender == null)
            {
                Debug.Log($"Sender does not exist.");
                return;
            }
            if (args.Count == 0)
            {
                Debug.Log("No arguments given.");
                return;
            }

            string character = StringFinder.Instance.GetMasterName(args[0]);
            if (character == null)
            {
                Debug.Log("Could not find master.");
                return;
            }

            var masterPrefab = MasterCatalog.FindMasterPrefab(character);
            var body = masterPrefab.GetComponent<CharacterMaster>().bodyPrefab;
            if (VariantRegister.RegisteredVariants.ContainsKey(body.name))
            {
                string[] identifiers = Array.Empty<string>();
                for (int i = 1; i < args.Count; i++)
                {
                    HG.ArrayUtils.ArrayAppend(ref identifiers, args[i]);
                }

                Vector3 location = args.sender.master.GetBody().transform.position;

                var bodyGameObject = UnityEngine.Object.Instantiate<GameObject>(masterPrefab, location, Quaternion.identity);
                CharacterMaster master = bodyGameObject.GetComponent<CharacterMaster>();
                NetworkServer.Spawn(bodyGameObject);
                master.bodyPrefab = body;
                var charBody = master.SpawnBody(args.sender.master.GetBody().transform.position, Quaternion.identity);

                var SpawnHandlerComponent = charBody.GetComponent<VariantSpawnHandler>();
                if (SpawnHandlerComponent)
                {
                    SpawnHandlerComponent.customSpawning = true;

                    List<int> variantInfosForSpawning = new List<int>();
                    List<string> toLog = new List<string>();

                    toLog.Add($"Spawned a {charBody.name} with the following variantInfos");

                    List<VariantInfo> variantInfos = SpawnHandlerComponent.variantInfos.ToList();
                    for (int i = 0; i < identifiers.Length; i++)
                    {
                        var current = identifiers[i];
                        var index = variantInfos.FindIndex(X => X.identifier.ToLower().Contains(current.ToLower()));
                        if (index != -1)
                        {
                            toLog.Add($"{variantInfos[index]} ({variantInfos[index].identifier}");
                            variantInfosForSpawning.Add(index);
                        }
                    }
                    Debug.Log(string.Join("\n", toLog));
                    SpawnHandlerComponent.RpcModifyComponents(variantInfosForSpawning.ToArray(), VariantSpawnHandler.RPCVariantInfo.All);
                }
            }
            else
            {
                Debug.Log($"The given characterMaster's body ({body.name}) has no variants.");
            }
        }

        [ConCommand(commandName = "spawn_as_variant", flags = ConVarFlags.ExecuteOnServer, helpText = "Respawns you as a variant using the specified body prefab and variant infos.\nArg[0] = Body Name\nArg[1 - Infinity] VariantInfo's Identifiers.")]
        public static void SpawnAsVariant(ConCommandArgs args)
        {
            if (args.Count == 0)
            {
                Debug.Log("No arguments given.");
                return;
            }
            string character = StringFinder.Instance.GetBodyName(args[0]);
            if (character == null)
            {
                Debug.Log("No body could be found with that name.");
                return;
            }
            GameObject newBody = BodyCatalog.FindBodyPrefab(character); ;

            if (args.sender == null)
            {
                Debug.Log($"Sender does not exist.");
                return;
            }

            CharacterMaster master = args.sender?.master;

            if (!master.GetBody())
            {
                Debug.Log("Master has no body.");
                return;
            }

            if (VariantRegister.RegisteredVariants.ContainsKey(newBody.name))
            {
                master.bodyPrefab = newBody;
                List<string> toLog = new List<string>();
                toLog.Add($"{args.sender.userName} is spawning as {character} with the following variantInfos:");
                RoR2.ConVar.BoolConVar stage1pod = ((RoR2.ConVar.BoolConVar)(typeof(Stage)).GetFieldCached("stage1PodConVar").GetValue(null));
                bool oldVal = stage1pod.value;
                stage1pod.SetBool(false);
                string[] identifiers = Array.Empty<string>();
                for (int i = 1; i < args.Count; i++)
                {
                    HG.ArrayUtils.ArrayAppend(ref identifiers, args[i]);
                }
                var charBody = master.Respawn(master.GetBody().transform.position, master.GetBody().transform.rotation);

                var SpawnHandlerComponent = charBody.GetComponent<VariantSpawnHandler>();
                if (SpawnHandlerComponent)
                {
                    SpawnHandlerComponent.customSpawning = true;

                    List<int> variantInfosForSpawning = new List<int>();

                    List<VariantInfo> variantInfos = SpawnHandlerComponent.variantInfos.ToList();
                    for (int i = 0; i < identifiers.Length; i++)
                    {
                        var current = identifiers[i];
                        var index = variantInfos.FindIndex(X => X.identifier.ToLower().Contains(current.ToLower()));
                        if (index != -1)
                        {
                            toLog.Add($"{variantInfos[index]} ({variantInfos[index].identifier}");
                            variantInfosForSpawning.Add(index);
                        }
                    }
                    Debug.Log(string.Join("\n", toLog));
                    SpawnHandlerComponent.RpcModifyComponents(variantInfosForSpawning.ToArray(), VariantSpawnHandler.RPCVariantInfo.All);
                }
                stage1pod.SetBool(oldVal);
            }
            else
            {
                Debug.Log($"The given CharacterBody ({newBody.name}) has no variants.");
            }
        }
    }
}
