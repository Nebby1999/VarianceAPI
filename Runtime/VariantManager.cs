#nullable enable
using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    public struct CharacterVariantIndex : IEquatable<CharacterVariantIndex>
    {
        public CharacterVariantIndex(int i)
        {
            this.i = i;
        }
        private readonly int i;
        public bool isValid => i >= 0;

        public static readonly CharacterVariantIndex none = new CharacterVariantIndex(-1);

        public static explicit operator int(CharacterVariantIndex masterIndex) => masterIndex.i;
        public static explicit operator CharacterVariantIndex(int value) => new CharacterVariantIndex(value);
        public bool Equals(CharacterVariantIndex other) => i == other.i;
        public override bool Equals(object obj) => obj is CharacterVariantIndex other && Equals(other);
        public override int GetHashCode() => i;
        public static bool operator ==(CharacterVariantIndex a, CharacterVariantIndex b) => a.i == b.i;
        public static bool operator !=(CharacterVariantIndex a, CharacterVariantIndex b) => a.i != b.i;
    }

    [Serializable]
    public struct NetworkCharacterVariantIndex : IEquatable<NetworkCharacterVariantIndex>
    {
        public uint i;
        public static implicit operator NetworkCharacterVariantIndex(CharacterVariantIndex masterIndex)
        {
            return new NetworkCharacterVariantIndex { i = (uint)((int)masterIndex + 1) };
        }
        public static implicit operator CharacterVariantIndex(NetworkCharacterVariantIndex networkMasterIndex)
        {
            return new CharacterVariantIndex(((int)networkMasterIndex.i) - 1);
        }
        public bool Equals(NetworkCharacterVariantIndex other) => i == other.i;
        public override bool Equals(object obj) => obj is CharacterVariantIndex other && Equals(other);
        public override int GetHashCode() => (int)i;
    }

    public static class CharacterVariantManager
    {
        public static int variantCount => _variantDefs?.Length ?? -1;
        private static CharacterVariantDef[]? _variantDefs;
        private static Dictionary<string, CharacterVariantIndex> _characterVariantNameToIndex = new Dictionary<string, CharacterVariantIndex>(StringComparer.OrdinalIgnoreCase);

        private static CharacterVariantProvider[]? _characterVariantProviders;

        public static ResourceAvailability managerAvailability;

        public static CharacterVariantDef? GetCharacterVariantDef(CharacterVariantIndex index)
        {
            ThrowIfUnavailable();
            return HG.ArrayUtils.GetSafe(_variantDefs!, (int)index);
        }

        public static CharacterVariantIndex FindCharacterVariantIndex(string characterVariantDefName)
        {
            ThrowIfUnavailable();
            if (_characterVariantNameToIndex.TryGetValue(characterVariantDefName, out var index))
            {
                return index;
            }
            return CharacterVariantIndex.none;
        }

        public static CharacterVariantProvider? FindCharacterVariantProvider(BodyIndex bodyIndex)
        {
            ThrowIfUnavailable();
            for(int i = 0; i < _characterVariantProviders!.Length; i++)
            {
                if (_characterVariantProviders[i].associatedBodyIndex == bodyIndex)
                {
                    return _characterVariantProviders[i];
                }
            }
            return null;
        }

        public static CharacterVariantProvider? FindCharacterVariantProvider(MasterCatalog.MasterIndex masterIndex)
        {
            ThrowIfUnavailable();
            for (int i = 0; i < _characterVariantProviders!.Length; i++)
            {
                if (_characterVariantProviders[i].associatedMasterIndex == masterIndex)
                {
                    return _characterVariantProviders[i];
                }
            }
            return null;
        }

        [SystemInitializer(typeof(BodyCatalog), typeof(MasterCatalog))]
        private static void Initialize()
        {
            CharacterVariantDef[] inputVariantDefs = Array.Empty<CharacterVariantDef>();

            //The actual value for the VariantIndex
            int variantIndex = 0;
            //Dictionaries to create the VariantProviders
            Dictionary<BodyIndex, List<CharacterVariantDef>> bodyToVariants = new Dictionary<BodyIndex, List<CharacterVariantDef>>();
            Dictionary<MasterCatalog.MasterIndex, List<CharacterVariantDef>> masterToVariants = new Dictionary<MasterCatalog.MasterIndex, List<CharacterVariantDef>>();
            //VariantDefs that passes the filtering, IE: TargetCharacter has valid component value.
            List<CharacterVariantDef> validVariantDefs = new List<CharacterVariantDef>();

            //Helpers for filling the bodyToVariants and masterToVariants
            List<BodyIndex> bodyIndicesAssociatedWithVariant = new List<BodyIndex>();
            List<MasterCatalog.MasterIndex> masterIndicesAssociatedWithVariant = new List<MasterCatalog.MasterIndex>();

            Array.Sort(inputVariantDefs, (a, b) => string.CompareOrdinal(a.name, b.name));
            
            foreach(var variantDef in inputVariantDefs)
            {
                bodyIndicesAssociatedWithVariant.Clear();
                masterIndicesAssociatedWithVariant.Clear();

                Component? characterComponent = variantDef.targetCharacter.LoadCharacterComponent();

                if(characterComponent == null)
                {
                    continue;
                }

                //If the component is a body, we will add the VariantDef to ANY master that uses this body prefab
                if(characterComponent is CharacterBody body)
                {
                    AddAssociatedMasterIndices(body, masterIndicesAssociatedWithVariant);
                    ListUtils.AddIfUnique(bodyIndicesAssociatedWithVariant, body.bodyIndex);
                    bodyIndicesAssociatedWithVariant.Add(body.bodyIndex);
                }
                //If the component is a master, we will add the VariantDef to EXCLUSIVELY said master and it's body.
                else if(characterComponent is CharacterMaster master)
                {
                    masterIndicesAssociatedWithVariant.Add(master.masterIndex);
                    if(master.bodyPrefab && master.bodyPrefab.TryGetComponent<CharacterBody>(out var masterBody))
                    {
                        ListUtils.AddIfUnique(bodyIndicesAssociatedWithVariant, masterBody.bodyIndex);
                    }
                }
                else
                {
                    continue;
                }

                //Add the variantDef to the list associated with the body indices
                foreach(var bodyIndexAssociatedWithVariant in bodyIndicesAssociatedWithVariant)
                {
                    bodyToVariants.TryAdd(bodyIndexAssociatedWithVariant, new List<CharacterVariantDef>());
                    bodyToVariants[bodyIndexAssociatedWithVariant].Add(variantDef);
                }

                //Add the variantDef to the list associated with master indices
                foreach(var masterIndexAssociatedWithVariant in masterIndicesAssociatedWithVariant)
                {
                    masterToVariants.TryAdd(masterIndexAssociatedWithVariant, new List<CharacterVariantDef>());
                    masterToVariants[masterIndexAssociatedWithVariant].Add(variantDef);
                }

                //Assign index, add to dictionary, add to list, increment indexer
                variantDef.characterVariantIndex = (CharacterVariantIndex)variantIndex;
                _characterVariantNameToIndex.Add(variantDef.name, variantDef.characterVariantIndex);
                validVariantDefs.Add(variantDef);
                variantIndex++;
            }

            //Finalize initialization
            _variantDefs = validVariantDefs.ToArray();
            List<CharacterVariantProvider> characterVariantProviders = new List<CharacterVariantProvider>();
            foreach (var (bodyIndex, variantDefs) in bodyToVariants)
            {
                characterVariantProviders.Add(new CharacterVariantProvider(variantDefs.ToArray(), bodyIndex, null));

                CharacterBody characterBody = BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
                characterBody.gameObject.AddComponent<CharacterBodyVariantController>();

                if(VAPIConfig._enableRewards)
                {
                    characterBody.gameObject.AddComponent<VariantDeathRewards>();
                }
            }

            foreach(var (masterIndex, variantDefs) in masterToVariants)
            {
                characterVariantProviders.Add(new CharacterVariantProvider(variantDefs.ToArray(), null, masterIndex));

                CharacterMaster characterMaster = MasterCatalog.GetMasterPrefab(masterIndex).GetComponent<CharacterMaster>();
                characterMaster.gameObject.AddComponent<CharacterMasterVariantStorage>();

                if(characterMaster.bodyPrefab)
                {
                    //Ensure the component, we don't want to call Add because there's a chance that the previous foreach added the component.
                    characterMaster.bodyPrefab.EnsureComponent<CharacterBodyVariantController>();
                    if(VAPIConfig._enableRewards)
                    {
                        characterMaster.bodyPrefab.EnsureComponent<VariantDeathRewards>();
                    }
                }
            }

            managerAvailability.MakeAvailable();
        }

        private static void AddAssociatedMasterIndices(CharacterBody body, in List<MasterCatalog.MasterIndex> output)
        {
            for(int i = 0; i < MasterCatalog.masterPrefabMasterComponents.Length; i++)
            {
                CharacterMaster characterMaster = MasterCatalog.masterPrefabMasterComponents[i];
                if(characterMaster.bodyPrefab && characterMaster.bodyPrefab == body.gameObject)
                {
                    ListUtils.AddIfUnique(output, characterMaster.masterIndex);
                }
            }
        }

        private static void Hook()
        {
            Stage.onStageStartGlobal += FilterCharacterVariantProviders;
        }

        private static void FilterCharacterVariantProviders(Stage obj)
        {
            for (int i = 0; i < _characterVariantProviders!.Length; i++)
            {
                _characterVariantProviders[i].FilterVariants();
            }
        }

        private static void ThrowIfUnavailable()
        {
            if(!managerAvailability.available)
            {
                throw new InvalidOperationException("Cannot access CharacterVariantManager when it's not available, consider subscribing to the managerAvailability.");
            }
        }

        #region Commands
        [ConCommand(commandName = "vapi_list_bodies", flags = ConVarFlags.None, helpText = "Lists all the bodies that have VariantDefs")]
        private static void CCvapiListBodies(ConCommandArgs args)
        {
            ThrowIfUnavailable();

            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            stringBuilder.AppendLine("Bodies with Variants:");
            stringBuilder.AppendLine("---------------------");

            foreach(CharacterVariantProvider characterVariantProvider in _characterVariantProviders!)
            {
                if(characterVariantProvider.associatedBodyIndex == BodyIndex.None)
                {
                    continue;
                }

                GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(characterVariantProvider.associatedBodyIndex);
                stringBuilder.AppendLine($"{bodyPrefab.name} (Variant Count: {characterVariantProvider.totalVariantCount}).");
            }

            Debug.Log(stringBuilder.ToString());
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
        }

        [ConCommand(commandName = "vapi_list_masters", flags = ConVarFlags.None, helpText = "Lists all the Masters that have VariantDefs")]
        private static void CCvapiListMasters(ConCommandArgs args)
        {
            ThrowIfUnavailable();

            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            stringBuilder.AppendLine("Masters with Variants:");
            stringBuilder.AppendLine("----------------------");

            foreach (CharacterVariantProvider characterVariantProvider in _characterVariantProviders!)
            {
                if (characterVariantProvider.associatedMasterIndex == MasterCatalog.MasterIndex.none)
                {
                    continue;
                }

                GameObject masterPrefab = MasterCatalog.GetMasterPrefab(characterVariantProvider.associatedMasterIndex);
                stringBuilder.AppendLine($"{masterPrefab.name} (Variant Count: {characterVariantProvider.totalVariantCount}).");
            }

            Debug.Log(stringBuilder.ToString());
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
        }

        [ConCommand(commandName = "vapi_list_body_variants", helpText = "Lists all the VariantDefs associated to a body.")]
        private static void CCvapi_ListBodyVariants(ConCommandArgs args)
        {
            ThrowIfUnavailable();

            if(args.Count == 0)
            {
                Debug.Log("No arguments given.");
                return;
            }

            string bodyName = args[0];
            BodyIndex bodyIndex = VAPIUtils.GetBodyIndex(bodyName);
            if(bodyIndex == BodyIndex.None)
            {
                Debug.Log($"No body could be found with the name {bodyName}. To get a list of bodies that have variants use \"vapi_list_bodies\".");
                return;
            }

            CharacterVariantProvider? provider = FindCharacterVariantProvider(bodyIndex);
            if(provider == null)
            {
                Debug.Log($"The provided body does not have a CharacterVariantProvider. To get a list of bodies that have variants use \"vapi_list_bodies\".");
                return;
            }

            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            stringBuilder.AppendLine($"{BodyCatalog.GetBodyPrefab(bodyIndex).name}'s Variants");
            stringBuilder.AppendLine("-------------------------------------------------------");
            for(int i = 0; i < provider.totalVariantCount; i++)
            {
                CharacterVariantDef variantDef = provider.allVariants[i];
                stringBuilder.AppendLine($"[{i}] = {variantDef.name}");
            }
            Debug.Log(stringBuilder.ToString());
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
        }


        [ConCommand(commandName = "vapi_list_master_variants", helpText = "Lists all the VariantDefs associated to a Master.")]
        private static void CCvapi_ListMasterVariants(ConCommandArgs args)
        {
            ThrowIfUnavailable();

            if (args.Count == 0)
            {
                Debug.Log("No arguments given.");
                return;
            }

            string masterName = args[0];
            MasterCatalog.MasterIndex masterIndex = VAPIUtils.GetMasterIndex(masterName);
            if (masterIndex == MasterCatalog.MasterIndex.none)
            {
                Debug.Log($"No Master could be found with the name {masterName}. To get a list of masters that have variants use \"vapi_list_masters\".");
                return;
            }

            CharacterVariantProvider? provider = FindCharacterVariantProvider(masterIndex);
            if (provider == null)
            {
                Debug.Log($"The provided master does not have a CharacterVariantProvider. To get a list of masters that have variants use \"vapi_list_masters\".");
                return;
            }

            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            stringBuilder.AppendLine($"{MasterCatalog.GetMasterPrefab(masterIndex).name}'s Variants");
            stringBuilder.AppendLine("-------------------------------------------------------");
            for (int i = 0; i < provider.totalVariantCount; i++)
            {
                CharacterVariantDef variantDef = provider.allVariants[i];
                stringBuilder.AppendLine($"[{i}] = {variantDef.name}");
            }
            Debug.Log(stringBuilder.ToString());
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
        }
        #endregion
    }

    public static partial class Extensions
    {
        public static void Write(this NetworkWriter writer, CharacterVariantIndex index)
        {
            writer.WritePackedIndex32((int)index);
        }

        public static CharacterVariantIndex ReadCharacterVariantIndex(this NetworkReader reader)
        {
            return (CharacterVariantIndex)reader.ReadPackedIndex32();
        }
    }
}