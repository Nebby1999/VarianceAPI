using DebugToolkit;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using VAPI.Components;

namespace VAPI
{
    internal static class Commands
    {
        [ConCommand(commandName = "vapi_list_bodies", flags = ConVarFlags.None, helpText = "Lists all the bodies that have VariantDefs")]
        private static void CCvapi_ListBodies(ConCommandArgs args)
        {
            List<string> toLog = new List<string>();
            toLog.Add("Modified Bodies:");
            toLog.Add("----------------");

            foreach (BodyVariantDefProvider provider in BodyVariantDefProvider.instances)
            {
                var bodyPrefab = BodyCatalog.GetBodyPrefab(provider.tiedIndex);
                toLog.Add($"{bodyPrefab.name} (VariantDef count: {provider.totalVariantCount})");
            }

            Debug.Log(string.Join("\n", toLog));
        }

        [ConCommand(commandName = "vapi_list_variants", flags = ConVarFlags.None, helpText = "Lists all the VariantDefs associated to a body." +
            "\nargs[0] = bodyName")]
        private static void CCvapi_ListVariants(ConCommandArgs args)
        {
            if (args.Count == 0)
            {
                Debug.Log("No arguments given.");
                return;
            }

            BodyIndex character = StringFinder.Instance.GetBodyFromPartial(args[0]);
            if (character == BodyIndex.None)
            {
                Debug.Log("No body could be found with that name. To get a list of bodies that have variants, use \"vapi_list_bodies\".");
                return;
            }

            var bodyVariantDefProvider = BodyVariantDefProvider.FindProvider(character);
            if (bodyVariantDefProvider == null)
            {
                Debug.Log("The body provided does not have a BodyVariantDefProvider. To get a list of bodies that have variants, use \"vapi_list_bodies\".");
                return;
            }

            List<string> toLog = new List<string>();
            toLog.Add($"{character}'s Variants");
            toLog.Add("-----------------------");
            for (int i = 0; i < bodyVariantDefProvider.totalVariantCount; i++)
            {
                VariantDef def = bodyVariantDefProvider.GetVariantDef(i);
                toLog.Add($"{i} - {def.name}");
            }
            Debug.Log(string.Join("\n", toLog));
        }

        [ConCommand(commandName = "vapi_spawn_ai", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawns a/the secific(s) variant(s)." +
            "\nArg[0] = master name" +
            "\nArg[1 - Infinity] = VariantDef Names")]
        public static void SpawnVariant(ConCommandArgs args)
        {
            if (args.sender == null)
            {
                Debug.Log($"Sender does not exist");
                return;
            }

            if (args.Count == 0)
            {
                Debug.Log("No Arguments Given.");
                return;
            }

            MasterCatalog.MasterIndex master = StringFinder.Instance.GetAiFromPartial(args[0]);
            if (master == MasterCatalog.MasterIndex.none)
            {
                Debug.Log("Could not find master.");
                return;
            }

            var masterPrefab = MasterCatalog.GetMasterPrefab(master);

            string[] variantNames = Array.Empty<string>();
            for (int i = 1; i < args.Count; i++)
            {
                HG.ArrayUtils.ArrayAppend(ref variantNames, args[i]);
            }
            List<VariantDef> variants = new List<VariantDef>();
            var provider = BodyVariantDefProvider.FindProvider(master);
            var availableVariants = provider.GetAllVariants(false);
            foreach (string variantName in variantNames)
            {
                for (int i = 0; i < availableVariants.Length; i++)
                {
                    var vd = availableVariants[i];
                    if (vd.name.ToLowerInvariant().Contains(variantName.ToLowerInvariant()))
                    {
                        variants.Add(vd);
                        break;
                    }
                }
            }

            Vector3 location = args.sender.master.GetBody().transform.position;

            VariantSummon summon = new VariantSummon
            {
                variantDefs = variants.ToArray(),
                ignoreTeamMemberLimit = true,
                useAmbientLevel = false,
                masterPrefab = masterPrefab,
                position = location,
                rotation = Quaternion.identity,
                summonerBodyObject = null,
                teamIndexOverride = masterPrefab.GetComponent<CharacterMaster>().teamIndex,
            };
            summon.PreformSummon();

            List<string> toLog = new List<string>();
            toLog.Add($"Spawned a {masterPrefab.name} with the following VariantDefs");
            for (int i = 0; i < variants.Count; i++)
            {
                toLog.Add($"{i} - {variants[i].name}");
            }
            Debug.Log(string.Join("\n", toLog));
        }

        [ConCommand(commandName = "vapi_spawn_as", flags = ConVarFlags.ExecuteOnServer, helpText = "Respawns you as the specified body prefab with the specified VariantDefs\n" +
            "Arg[0] = body name\n" +
            "Arg[1 - Infinity] VariantDef names")]
        public static void SpawnAsVariant(ConCommandArgs args)
        {
            if (args.Count == 0)
            {
                Debug.Log("No Arguments Given");
                return;
            }

            BodyIndex body = StringFinder.Instance.GetBodyFromPartial(args[0]);
            if (body == BodyIndex.None)
            {
                Debug.Log("No body could be found with that name");
                return;
            }

            GameObject newBody = BodyCatalog.GetBodyPrefab(body);

            if (args.sender == null)
            {
                Debug.Log("Sender does not exist");
                return;
            }

            CharacterMaster master = args.senderMaster;

            if (!master.GetBody())
            {
                Debug.Log("Master has no body");
                return;
            }

            string[] variantNames = Array.Empty<string>();
            for (int i = 1; i < args.Count; i++)
            {
                HG.ArrayUtils.ArrayAppend(ref variantNames, args[i]);
            }
            List<VariantDef> variants = new List<VariantDef>();
            var provider = BodyVariantDefProvider.FindProvider(body);
            var availableVariants = provider.GetAllVariants(false);
            foreach (string variantName in variantNames)
            {
                for (int i = 0; i < availableVariants.Length; i++)
                {
                    var vd = availableVariants[i];
                    if (vd.name.ToLowerInvariant().Contains(variantName.ToLowerInvariant()))
                    {
                        variants.Add(vd);
                        break;
                    }
                }
            }

            master.bodyPrefab = newBody;
            List<string> toLog = new List<string>();
            toLog.Add($"{args.sender.userName} is spawning as {body} with the following VariantDefs:");

            RoR2.ConVar.BoolConVar stage1pod = ((RoR2.ConVar.BoolConVar)(typeof(Stage)).GetFieldCached("stage1PodConVar").GetValue(null));
            bool oldVal = stage1pod.value;
            stage1pod.SetBool(false);

            var characterBody = master.Respawn(master.GetBody().footPosition, master.GetBody().transform.rotation);
            if (characterBody)
            {
                characterBody.gameObject.AddComponent<DoNotTurnIntoVariant>();

                BodyVariantManager manager = characterBody.GetComponent<BodyVariantManager>();
                BodyVariantReward reward = characterBody.GetComponent<BodyVariantReward>();

                if (manager)
                {
                    manager.AddVariants(variants);
                }
                if (reward)
                {
                    reward.AddVariants(variants);
                }
            }

            for (int i = 0; i < variants.Count; i++)
            {
                toLog.Add($"{i} - {variants[i].name}");
            }

            Debug.Log(string.Join("\n", toLog));
            stage1pod.SetBool(oldVal);
        }
    }
}
