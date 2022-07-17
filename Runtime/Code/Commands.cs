using DebugToolkit;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            foreach(BodyVariantDefProvider provider in BodyVariantDefProvider.instances)
            {
                var bodyPrefab = BodyCatalog.GetBodyPrefab(provider.TiedIndex);
                toLog.Add($"{bodyPrefab.name} (VariantDef count: {provider.TotalVariantCount})");
            }

            Debug.Log(string.Join("\n", toLog));
        }

        [ConCommand(commandName = "vapi_list_variants", flags = ConVarFlags.None, helpText = "Lists all the VariantDefs associated to a body." +
            "\nargs[0] = bodyName")]
        private static void CCvapi_ListVariants(ConCommandArgs args)
        {
            if(args.Count == 0)
            {
                Debug.Log("No arguments given.");
                return;
            }

            string character = StringFinder.Instance.GetBodyName(args[0]);
            if(character == null)
            {
                Debug.Log("No body could be found with that name. To get a list of bodies that have variants, use \"vapi_list_bodies\".");
                return;
            }

            var bodyVariantDefProvider = BodyVariantDefProvider.FindProvider(character);
            if(bodyVariantDefProvider == null)
            {
                Debug.Log("The body provided does not have a BodyVariantDefProvider. To get a list of bodies that have variants, use \"vapi_list_bodies\".");
                return;
            }

            List<string> toLog = new List<string>();
            toLog.Add($"{character}'s Variants");
            toLog.Add("-----------------------");
            for(int i = 0; i < bodyVariantDefProvider.TotalVariantCount; i++)
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
            if(args.sender == null)
            {
                Debug.Log($"Sender does not exist");
                return;
            }

            if(args.Count == 0)
            {
                Debug.Log("No Arguments Given.");
                return;
            }

            string master = StringFinder.Instance.GetMasterName(args[0]);
            if(master == null)
            {
                Debug.Log("Could not find master.");
                return;
            }

            var masterPrefab = MasterCatalog.FindMasterPrefab(master);
            string[] variantNames = Array.Empty<string>();
            for(int i = 1; i < args.Count; i++)
            {
                HG.ArrayUtils.ArrayAppend(ref variantNames, args[i]);
            }
            List<VariantDef> variants = new List<VariantDef>();
            foreach(string variantName in variantNames)
            {
                VariantIndex index = VariantCatalog.FindVariantIndex(variantName);
                if (index != VariantIndex.None)
                    variants.Add(VariantCatalog.GetVariantDef(index));
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
            summon.Perform();

            List<string> toLog = new List<string>();
            toLog.Add($"Spawned a {masterPrefab.name} with the following VariantDefs");
            for(int i = 0; i < variants.Count; i++)
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
            if(args.Count == 0)
            {
                Debug.Log("No Arguments Given");
                return;
            }

            string body = StringFinder.Instance.GetBodyName(args[0]);
            if(body == null)
            {
                Debug.Log("No body could be found with that name");
                return;
            }

            GameObject newBody = BodyCatalog.FindBodyPrefab(body);

            if(args.sender == null)
            {
                Debug.Log("Sender does not exist");
                return;
            }

            CharacterMaster master = args.senderMaster;

            if(!master.GetBody())
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
            foreach (string variantName in variantNames)
            {
                VariantIndex index = VariantCatalog.FindVariantIndex(variantName);
                if (index != VariantIndex.None)
                    variants.Add(VariantCatalog.GetVariantDef(index));
            }

            master.bodyPrefab = newBody;
            List<string> toLog = new List<string>();
            toLog.Add($"{args.sender.userName} is spawning as {body} with the following VariantDefs:");
            
            RoR2.ConVar.BoolConVar stage1pod = ((RoR2.ConVar.BoolConVar)(typeof(Stage)).GetFieldCached("stage1PodConVar").GetValue(null));
            bool oldVal = stage1pod.value;
            stage1pod.SetBool(false);

            var characterBody = master.Respawn(master.GetBody().footPosition, master.GetBody().transform.rotation);
            if(characterBody)
            {
                characterBody.gameObject.AddComponent<DoNotTurnIntoVariant>();

                BodyVariantManager manager = characterBody.GetComponent<BodyVariantManager>();
                BodyVariantReward reward = characterBody.GetComponent<BodyVariantReward>();

                if(manager)
                {
                    manager.AddVariants(variants);
                }
                if(reward)
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
