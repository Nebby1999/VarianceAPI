#nullable enable
using HG;
using HG.Coroutines;
using MSU.Config;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// Index struct for <see cref="CharacterVariantIndex"/>
    /// </summary>
    public struct CharacterVariantIndex : IEquatable<CharacterVariantIndex>
    {
        /// <summary>
        /// Constructor for a <see cref="CharacterVariantIndex"/>
        /// </summary>
        /// <param name="i">The index of the variant</param>
        public CharacterVariantIndex(int i)
        {
            this.i = i;
        }
        private readonly int i;

        /// <summary>
        /// Returns true if the index is valid
        /// </summary>
        public bool isValid => i >= 0;

        /// <summary>
        /// Represents an invalid index
        /// </summary>
        public static readonly CharacterVariantIndex none = new CharacterVariantIndex(-1);

        /// <summary>
        /// Explicitly casts a <see cref="CharacterVariantIndex"/> into an <see cref="int"/>
        /// </summary>
        public static explicit operator int(CharacterVariantIndex masterIndex) => masterIndex.i;

        /// <summary>
        /// Explicitly casts an <see cref="int"/> int a <see cref="CharacterVariantIndex"/>
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator CharacterVariantIndex(int value) => new CharacterVariantIndex(value);

        /// <summary>
        /// Returns true if <paramref name="other"/> and <see cref="this"/> have the same indices
        /// </summary>
        public bool Equals(CharacterVariantIndex other) => i == other.i;

        /// <summary>
        /// Returns true if <paramref name="obj"/> is a boxed <see cref="CharacterVariantIndex"/>, and it has the same index as <see cref="this"/>
        /// </summary>
        public override bool Equals(object obj) => obj is CharacterVariantIndex other && Equals(other);

        /// <summary>
        /// Returns the hash code for this <see cref="CharacterVariantIndex"/>
        /// </summary>
        public override int GetHashCode() => i;

        /// <summary>
        /// Equality operator, see <see cref="Equals(CharacterVariantIndex)"/>
        /// </summary>
        public static bool operator ==(CharacterVariantIndex a, CharacterVariantIndex b) => a.i == b.i;

        /// <summary>
        /// Inequality operator, negates the result of <see cref="Equals(CharacterVariantIndex)"/>
        /// </summary>
        public static bool operator !=(CharacterVariantIndex a, CharacterVariantIndex b) => a.i != b.i;
    }

    /// <summary>
    /// A Networkable version of <see cref="CharacterVariantIndex"/>
    /// </summary>
    [Serializable]
    public struct NetworkCharacterVariantIndex : IEquatable<NetworkCharacterVariantIndex>
    {
        /// <summary>
        /// The index being networked
        /// </summary>
        public uint i;

        /// <summary>
        /// Implicitly casts a <see cref="CharacterVariantIndex"/> into a <see cref="NetworkCharacterVariantIndex"/>
        /// </summary>
        public static implicit operator NetworkCharacterVariantIndex(CharacterVariantIndex masterIndex)
        {
            return new NetworkCharacterVariantIndex { i = (uint)((int)masterIndex + 1) };
        }

        /// <summary>
        /// Implicitly casts a <see cref="NetworkCharacterVariantIndex"/> into a <see cref="CharacterVariantIndex"/>
        /// </summary>
        /// <param name="networkMasterIndex"></param>
        public static implicit operator CharacterVariantIndex(NetworkCharacterVariantIndex networkMasterIndex)
        {
            return new CharacterVariantIndex(((int)networkMasterIndex.i) - 1);
        }

        /// <summary>
        /// Returns true if the <see cref="NetworkCharacterVariantIndex"/> passed in <paramref name="other"/> has the same index as <see cref="this"/>
        /// </summary>
        public bool Equals(NetworkCharacterVariantIndex other) => i == other.i;

        /// <summary>
        /// Returns true if the <paramref name="obj"/> is a boxed <see cref="NetworkCharacterVariantIndex"/>, and it's index is equal to <see cref="this"/>'s index
        /// </summary>
        public override bool Equals(object obj) => obj is CharacterVariantIndex other && Equals(other);

        /// <summary>
        /// Retrieves the hash code for this <see cref="NetworkCharacterVariantIndex"/>
        /// </summary>
        public override int GetHashCode() => (int)i;
    }

    /// <summary>
    /// Catalog for <see cref="CharacterVariantDef"/>s
    /// </summary>
    public static class CharacterVariantCatalog
    {
        /// <summary>
        /// A <see cref="ReadOnlyArray{T}"/> of the registered <see cref="CharacterVariantDef"/>s
        /// </summary>
        public static ReadOnlyArray<CharacterVariantDef> registeredCharacterVariantDefs => _characterVariantDefs;

        /// <summary>
        /// The amount of <see cref="CharacterVariantDef"/>s stored in the catalog
        /// </summary>
        public static int characterVariantCount => _characterVariantDefs?.Length ?? -1;
        private static CharacterVariantDef[]? _characterVariantDefs;
        private static Dictionary<string, CharacterVariantIndex> _characterVariantNameToIndex = new Dictionary<string, CharacterVariantIndex>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<CharacterVariantDef, List<CharacterVariantDef>> _dependeeVariantToDependantVariants = new Dictionary<CharacterVariantDef, List<CharacterVariantDef>>();

        private static CharacterVariantProvider?[] _characterVariantProviders = Array.Empty<CharacterVariantProvider>();

        /// <summary>
        /// Catalog availability. Catalog initializes after <see cref="BodyCatalog"/>, <see cref="MasterCatalog"/> and <see cref="VariantPackManager"/>
        /// </summary>
        public static ResourceAvailability catalogAvailability;

        /// <summary>
        /// Gets the <see cref="CharacterVariantDef"/> with the specified <paramref name="index"/>
        /// <br></br>
        /// Returns null if the <paramref name="index"/> is out of range
        /// </summary>
        /// <param name="index">The index of the <see cref="CharacterVariantDef"/></param>
        public static CharacterVariantDef? GetCharacterVariantDef(CharacterVariantIndex index)
        {
            ThrowIfUnavailable();
            return HG.ArrayUtils.GetSafe(_characterVariantDefs!, (int)index);
        }

        /// <summary>
        /// Finds the <see cref="CharacterVariantIndex"/> that has the <see cref="CharacterVariantDef"/> with the specified name in <paramref name="characterVariantDefName"/>
        /// <br></br>
        /// Returns <see cref="CharacterVariantIndex.none"/> if there's no <see cref="CharacterVariantDef"/> with the name <paramref name="characterVariantDefName"/>
        /// </summary>
        /// <param name="characterVariantDefName">The name of the <see cref="CharacterVariantDef"/></param>
        public static CharacterVariantIndex FindCharacterVariantIndex(string? characterVariantDefName)
        {
            ThrowIfUnavailable();

            if(string.IsNullOrWhiteSpace(characterVariantDefName))
            {
                return CharacterVariantIndex.none;
            }

            return _characterVariantNameToIndex.GetValueOrDefault(characterVariantDefName, CharacterVariantIndex.none);
        }

        public static CharacterVariantProvider? FindCharacterVariantProvider(GameObject? bodyPrefab) => FindCharacterVariantProvider(bodyPrefab != null ? bodyPrefab.GetComponent<CharacterBody>() : null);
        public static CharacterVariantProvider? FindCharacterVariantProvider(CharacterBody? bodyComponent) => GetCharacterVariantProvider(bodyComponent != null ? bodyComponent.bodyIndex : BodyIndex.None);

        /// <summary>
        /// Finds the <see cref="CharacterVariantProvider"/> for the specified body in <paramref name="bodyIndex"/>.
        /// <br></br>
        /// returns null if <paramref name="bodyIndex"/> has no variant provider
        /// </summary>
        /// <param name="bodyIndex">The index to retrieve it's variant provider</param>
        public static CharacterVariantProvider? GetCharacterVariantProvider(BodyIndex bodyIndex)
        {
            ThrowIfUnavailable();

            //The providers array is mapped to body indices, so using get safe will get that body's variants.
            return HG.ArrayUtils.GetSafe(_characterVariantProviders, (int)bodyIndex);
        }



        [SystemInitializer(typeof(BodyCatalog), typeof(VariantPackManager))]
        private static IEnumerator Initialize()
        {
            _characterVariantNameToIndex.Clear();

            using(var coroutineWithResult = CoroutineWithResult<CharacterVariantDef[]>.Rent(RegisterVariantDefs(VariantPackManager.characterVariantDefs)))
            {
                while(coroutineWithResult.MoveNext())
                {
                    yield return null;
                }

                _characterVariantDefs = coroutineWithResult.result;
            }

            for(int i = 0; i < _characterVariantDefs!.Length; i++)
            {
                var def = _characterVariantDefs[i];

                //If we get a def that's hidden, and the def has a dependant availability condition, then we need to make sure the config data from the dependee gets passed to the dependant.
                if(def.isHidden && def.spawnCondition is VariantDependantCondition dependantCondition)
                {
                    var dependee = dependantCondition.dependantVariantDef.GetCharacterVariantDef();
                    if(dependee != null)
                    {
                        _dependeeVariantToDependantVariants.TryAdd(dependee, new List<CharacterVariantDef>());
                        _dependeeVariantToDependantVariants[dependee].Add(def);
                    }
                    else
                    {
                        VAPILog.Warning($"{def} has a VariantDependantCondition spawn condition, but the dependantVariantDef doesnt exist!");
                    }
                }

                try
                {
                    def.additionalVariantComponents?.Initialize();
                }
                catch(Exception ex)
                {
                    VAPILog.Error($"Failed to initialize variant components for {def}: {ex}");
                }
            }

            //Once we have the dependee dictionary done, that's when we config
            for(int i = 0; i < _characterVariantDefs.Length; i++)
            {
                var def = _characterVariantDefs[i];
                ConfigureVariant(def);
            }

            Stage.onStageStartGlobal += FilterCharacterVariantProviders;

            VAPILog.Info($"CharacterVariantCatalog Initialized.");
            catalogAvailability.MakeAvailable();
        }

        private static IEnumerator<CharacterVariantDef[]?> RegisterVariantDefs(ReadOnlyArray<CharacterVariantDef> readOnlyVariantDefs)
        {

            //Sort the variants
            CharacterVariantDef[] sortedVariants = new CharacterVariantDef[readOnlyVariantDefs.Length];
            readOnlyVariantDefs.CopyTo(sortedVariants, 0);

            Array.Sort(sortedVariants, (a, b) => string.CompareOrdinal(a.cachedName, b.cachedName));

            //Dictionary to map a body index to the list of variants
            Dictionary<BodyIndex, List<CharacterVariantDef>> bodyToVariants = new Dictionary<BodyIndex, List<CharacterVariantDef>>();

            //A list of variants without providers, these are caused by invalid target characters (direct ref null, address is invalid, or doesnt resolve to an actual body)
            List<CharacterVariantDef> providerlessVariants = new List<CharacterVariantDef>();

            for(int i = 0; i < sortedVariants.Length; i++)
            {
                yield return null;
                
                CharacterVariantIndex index = (CharacterVariantIndex)i;
                CharacterVariantDef variantDef = sortedVariants[i];

                void AddToCatalog()
                {
                    variantDef.characterVariantIndex = index;
                    _characterVariantNameToIndex.Add(variantDef.cachedName, variantDef.characterVariantIndex);
                }

                //Checking if this variant will be assigned to a provider.
                //If the target body fails to load or is invalid, we'll still add it to the catalog to keep the indices consistent.

                //target metadata is invallid. no provider.
                if(!variantDef.targetBody.IsKeyValid())
                {
                    providerlessVariants.Add(variantDef);
                    AddToCatalog();
                    continue;
                }
                //target metadata is valid, but doesnt resolve to a body. No provider
                if(!variantDef.targetBody.TryLoadCharacterComponent(out var characterBody))
                {
                    providerlessVariants.Add(variantDef);
                    AddToCatalog();
                    continue;
                }
                //Invalid body index. no provider
                if(characterBody!.bodyIndex == BodyIndex.None)
                {
                    providerlessVariants.Add(variantDef);
                    AddToCatalog();
                    continue;
                }
                //Add the body's index to the dictionary, this dictionary is later used to create the provider.
                bodyToVariants.TryAdd(characterBody!.bodyIndex, new List<CharacterVariantDef>());
                bodyToVariants[characterBody!.bodyIndex].Add(variantDef);

                //Provider success
                variantDef.hasProvider = true;
                AddToCatalog();
            }

            VAPILog.Info($"Finished associating CharacterVariantDefs with bodies");
            //Finalize initialization
            //Map for BodyIndex to provider, faster lookups.
            CharacterVariantProvider?[] characterVariantProviders = new CharacterVariantProvider?[BodyCatalog.bodyCount];
            foreach(var (bodyIndex, variantDefs) in bodyToVariants)
            {
                yield return null;
                //Create the providers for the body
                characterVariantProviders[(int)bodyIndex] = new CharacterVariantProvider(variantDefs.ToArray(), bodyIndex);

                CharacterBody characterBody = BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
                characterBody.gameObject.EnsureComponent<CharacterBodyVariantController>(); //Ensure our component, add death rewards if enabled.
                if(VAPIConfig._enableRewards)
                {
                    characterBody.gameObject.EnsureComponent<VariantDeathRewards>();
                }
            }

            //We now need to add the master storage to the master prefabs.
            foreach(var masterPrefab in MasterCatalog.masterPrefabs)
            {
                //If the master has a body prefab, and the body prefab has a body, and the body to variants dictionary has it's body prefab's body index, add the storage.
                //if that chain fails, check if the master prefab has the player character master controller, if so, add the storage anyways.

                if((masterPrefab.TryGetComponent<CharacterMaster>(out var master) && 
                    master.bodyPrefab && 
                    master.bodyPrefab.TryGetComponent<CharacterBody>(out var body) && 
                    bodyToVariants.ContainsKey(body.bodyIndex)) || masterPrefab.TryGetComponent<PlayerCharacterMasterController>(out _))
                {
                    masterPrefab.AddComponent<CharacterMasterVariantStorage>();
                }
            }

            VAPILog.Info($"Created providers and added the necessary behaviours to bodies and masters");
            _characterVariantProviders = characterVariantProviders;
            if(providerlessVariants.Count > 0)
            {
                using(HG.StringBuilderPool.RentStringBuilder(out var sb))
                {
                    sb.AppendLine($"The following variants did not resolve a valid CharacterBody, and as a result, have no CharacterVariantProviders. They're still added to the catalog but they won't be spawned by the built in systems. If they're meant to be for a vanilla charater, consider validating the AddressReferencedBody you're using. If it's for a modded character, consider adding the VariantDef only if the mod is present, and checking that the catalog key for AddressReferencedBody is typed correctly:");
                    foreach(var variant in providerlessVariants)
                    {
                        sb.AppendLine(variant.ToString());
                    }
                    VAPILog.Message(sb.ToString());
                }
            }

            yield return sortedVariants;
        }

        private static void FilterCharacterVariantProviders(Stage obj)
        {
            for (int i = 0; i < _characterVariantProviders!.Length; i++)
            {
                _characterVariantProviders[i]?.FilterVariants();
            }
        }

        private static void ConfigureVariant(CharacterVariantDef variantDef)
        {
            if (variantDef.isHidden)
                return;

            try
            {
                variantDef.spawnRate = new ConfiguredFloat(variantDef.spawnRate)
                {
                    //Only variants with providers are configured, and to be in a provider, you need to have valid targetCharacter reference.
                    section = $"{variantDef.targetBody.LoadCharacterComponent()!.name} Variants",
                    key = $"{variantDef.cachedName} Spawn Rate",
                    description = $"Chance for the {variantDef.cachedName} variant to spawn. (Percentage, 0-100)",
                    configFile = variantDef.associatedConfigFile,
                    modGUID = variantDef.ownerPlugin!.GUID,
                    modName = variantDef.ownerPlugin!.Name,
                    sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
                    sliderConfig = new RiskOfOptions.OptionConfigs.SliderConfig
                    {
                        min = 0,
                        max = 100
                    }
                }
                .WithConfigChange(f =>
                {
                    variantDef.spawnRate = f;
                    UpdateDependentDefSpawnRates(variantDef);
                }).DoConfigure();

                variantDef.isUnique = new ConfiguredBool(variantDef.isUnique)
                {
                    section = $"{variantDef.targetBody.LoadCharacterComponent()!.name} Variants",
                    key = $"{variantDef.cachedName} Uniqueness",
                    description = $"Wether or not {variantDef.cachedName} is Unique. Unique variants cannot merge with other ones.",
                    configFile = variantDef.associatedConfigFile,
                    modGUID = variantDef.ownerPlugin!.GUID,
                    modName = variantDef.ownerPlugin!.Name,
                }.WithConfigChange(b =>
                {
                    variantDef.isUnique = b;
                    UpdateDependentDefUniqueness(variantDef);
                }).DoConfigure();
            }
            catch(Exception e)
            {
                VAPILog.Error($"Error Configuring Variant {variantDef}: {e}");
            }
        }

        private static void UpdateDependentDefSpawnRates(CharacterVariantDef dependee)
        {
            if(_dependeeVariantToDependantVariants.TryGetValue(dependee, out var variantDefs))
            {
                foreach(var variantDef in variantDefs)
                {
                    variantDef.spawnRate = dependee.spawnRate;
                }
            }
        }

        private static void UpdateDependentDefUniqueness(CharacterVariantDef dependee)
        {
            if(_dependeeVariantToDependantVariants.TryGetValue(dependee, out var variantDefs))
            {
                foreach(var variantDef in variantDefs)
                {
                    variantDef.isUnique = dependee.isUnique;
                }
            }
        }

        private static void ThrowIfUnavailable()
        {
            if(!catalogAvailability.available)
            {
                throw new InvalidOperationException("Cannot access CharacterVariantCatalog when it's not available, consider subscribing to the catalogAvailability.");
            }
        }

        #region Commands
        [ConCommand(commandName = "vapi_list_bodies", flags = ConVarFlags.None, helpText = "Lists all the bodies that have VariantDefs")]
        private static void CCVapiListBodies(ConCommandArgs args)
        {
            ThrowIfUnavailable();

            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            stringBuilder.AppendLine("Bodies with Variants:");
            stringBuilder.AppendLine("---------------------");

            foreach(CharacterVariantProvider characterVariantProvider in _characterVariantProviders!)
            {
                if(characterVariantProvider == null || characterVariantProvider.associatedBodyIndex == BodyIndex.None)
                {
                    continue;
                }

                GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(characterVariantProvider.associatedBodyIndex);
                stringBuilder.AppendLine($"{bodyPrefab.name} (Variant Count: {characterVariantProvider.totalVariantCount}).");
            }

            Debug.Log(stringBuilder.ToString());
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
        }

        [ConCommand(commandName = "vapi_list_body_variants", helpText = "Lists all the VariantDefs associated to a body.")]
        private static void CCVapiListBodyVariants(ConCommandArgs args)
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

            CharacterVariantProvider? provider = GetCharacterVariantProvider(bodyIndex);
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
                stringBuilder.AppendLine($"[{i}] = {variantDef.cachedName}");
            }
            Debug.Log(stringBuilder.ToString());
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
        }

        [ConCommand(commandName = "vapi_spawn_ai", flags= ConVarFlags.ExecuteOnServer, helpText = "Spawns a specific master with a specific set of variant defs.\n" +
            "Mandatory Arguments:\n" +
            "[0] = masterName\n" +
            "[1] = CharacterVariantDef name list. (Encapsulate in \"\". Separate by \",\". Spaces are allowed.)\n" +
            "Optional Arguments (Must be after CharacterVariantDef names):" +
            "\"count:\" Amount of characters to spawn (count:1)\n" +
            "\"equipment:\" Equipment name for the character (equipment:none)\n" +
            "\"noai:\" Wether the Variant has AI or not (noai:false)\n" +
            "\"team:\" Which team is the variant in (team:monster)")]
        private static void CCVapiSpawnAI(ConCommandArgs args)
        {
            if(!VariantSpawnManager.instance)
            {
                Debug.LogWarning($"VarianceAPI's Variant Spawning commands are not available when the VarianceAPI expansion is disabled.");
                return;
            }

            if(args.Count == 0)
            {
                Debug.Log("No Arguments Given.");
                return;
            }

            if(!args.senderBody)
            {
                Debug.Log($"Sender must have a body");
                return;
            }

            MasterCatalog.MasterIndex masterIndex = VAPIUtils.GetMasterIndex(args[0]);
            if(masterIndex == MasterCatalog.MasterIndex.none)
            {
                Debug.Log("Could not find master.");
                return;
            }
            var characterMaster = MasterCatalog.GetMasterPrefab(masterIndex).GetComponent<CharacterMaster>();

            var provider = FindCharacterVariantProvider(characterMaster.bodyPrefab);
            if(provider == null)
            {
                Debug.Log($"CharacterMaster with name {args[0]} does not have a CharacterVariantProvider");
                return;
            }

            var variantDefNamesAsListString = args[1];
            string[] variantDefNames = variantDefNamesAsListString.Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<CharacterVariantDef> variantDefs = new List<CharacterVariantDef>();
            var availableVariants = provider.allVariants;
            foreach(string variantName in variantDefNames)
            {
                foreach(var variantDef in availableVariants)
                {
                    if(variantDef.cachedName.Contains(variantName, StringComparison.OrdinalIgnoreCase))
                    {
                        variantDefs.Add(variantDef);
                        break;
                    }
                }
            }
            variantDefs.Sort((a, b) =>
            {
                return ((int)a.characterVariantIndex).CompareTo(((int)b.characterVariantIndex));
            });

            var senderBody = args.senderBody;
            Vector3 location = args.sender.master.GetBody().transform.position;
            if(senderBody.TryGetComponent<InputBankTest>(out var inputBankTest))
            {
                if(Physics.Raycast(inputBankTest.GetAimRay(), out RaycastHit raycastHit, 1024, LayerIndex.world.mask))
                {
                    location = raycastHit.point;
                }
            }

            int aiCount = args.TryGetOptionalInt("count:") ?? 1;
            EquipmentIndex equipmentIndex = args.TryGetOptionalEquipmentIndex("equipment:") ?? EquipmentIndex.None;
            bool noAI = args.TryGetOptionalBool("noai:") ?? false;
            TeamIndex teamIndex = args.TryGetOptionalEnum<TeamIndex>("team:") ?? TeamIndex.Monster;

            MasterSummon masterSummon = new MasterSummon()
            {
                masterPrefab = characterMaster.gameObject,
                ignoreTeamMemberLimit = true,
                useAmbientLevel = false,
                position = location,
                rotation = Quaternion.identity,
                summonerBodyObject = null,
                teamIndexOverride = teamIndex,
            };
            VariantMasterSummon variantMasterSummon = new VariantMasterSummon(masterSummon)
            {
                variantDefs = variantDefs.ToArray(),
            };
            variantMasterSummon.onSummonPerformed += (report) =>
            {
                var master = report.masterSummonReport.summonMasterInstance;
                if(equipmentIndex != EquipmentIndex.None)
                {
                    master.inventory.SetEquipmentIndex(equipmentIndex, false);
                    EliteDef eliteDef = EliteCatalog.GetEliteDefFromEquipmentIndex(equipmentIndex);
                    if(eliteDef)
                    {
                        master.inventory.GiveItemPermanent(RoR2Content.Items.BoostHp, Mathf.RoundToInt((eliteDef.healthBoostCoefficient - 1) * 10));
                        master.inventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, Mathf.RoundToInt(eliteDef.damageBoostCoefficient - 1) * 10);
                    }
                }
                if(noAI)
                {
                    foreach(var ai in master.aiComponents)
                    {
                        UnityEngine.Object.Destroy(ai);
                    }
                    master.aiComponents = Array.Empty<BaseAI>();
                }
            };

            for(int i = 0; i < aiCount; i++)
            {
                variantMasterSummon.Perform();
            }
        }

        [ConCommand(commandName = "vapi_spawn_body", flags = ConVarFlags.ExecuteOnServer, helpText = "Spawns a Masterless Body Prefab with the specified VariantDefs.\n" +
            "Mandatory Arguments:\n" +
            "[0] = bodyName (must be flagged as masterless)" +
            "[1] = CharacterVariantDef name list. (Encapsulate in quotes (\"). Separate by commas (,). Spaces are allowed.)")]
        private static void CCVapiSpawnBody(ConCommandArgs args)
        {
            if (!VariantSpawnManager.instance)
            {
                Debug.LogWarning($"VarianceAPI's Variant Spawning commands are not available when the VarianceAPI expansion is disabled.");
                return;
            }

            if (args.Count == 0)
            {
                Debug.Log($"No Arguments Given");
                return;
            }

            if (!args.senderBody)
            {
                Debug.Log($"Sender must have a body");
                return;
            }

            BodyIndex bodyIndex = VAPIUtils.GetBodyIndex(args[0]);
            if(bodyIndex == BodyIndex.None)
            {
                Debug.Log("No body could be found with that name");
                return;
            }

            CharacterBody bodyPrefab = BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);

            if(!bodyPrefab.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
            {
                Debug.Log($"Cannot spawn {bodyPrefab} as a variant via vapi_spawn_body as it's not flagged as a masterless body. Utilize vapi_spawn_ai instead.");
                return;
            }

            var provider = GetCharacterVariantProvider(bodyIndex);
            if(provider == null)
            {
                Debug.Log($"CharacterBody with name {args[0]} does not have a CharacterVariantProvider");
                return;
            }

            var variantDefNamesAsListString = args.TryGetArgString(1) ?? "";
            string[] variantDefNames = variantDefNamesAsListString.Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<CharacterVariantDef> variantDefs = new List<CharacterVariantDef>();

            var availableVariants = provider.allVariants;
            foreach (string variantName in variantDefNames)
            {
                foreach (var variantDef in availableVariants)
                {
                    if (variantDef.cachedName.Contains(variantName, StringComparison.OrdinalIgnoreCase))
                    {
                        variantDefs.Add(variantDef);
                        break;
                    }
                }
            }
            variantDefs.Sort((a, b) =>
            {
                return ((int)a.characterVariantIndex).CompareTo(((int)b.characterVariantIndex));
            });

            BodySpawnCard bodySpawnCard = ScriptableObject.CreateInstance<BodySpawnCard>();
            bodySpawnCard.sendOverNetwork = true;
            bodySpawnCard.prefab = bodyPrefab.gameObject;
            bodySpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            bodySpawnCard.directorCreditCost = 0;

            var spawnRequest = new DirectorSpawnRequest(bodySpawnCard, new DirectorPlacementRule
            {
                position = args.senderBody.corePosition,
                maxDistance = 5,
                minDistance = 1,
                placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                spawnOnTarget = args.senderBody.transform
            }, VariantSpawnManager.instance!.variantSpawnRng);

            VariantDirectorSpawnRequest spawnRequestVariant = new VariantDirectorSpawnRequest(spawnRequest)
            {
                variantDefs = variantDefs.ToArray()
            };

            bodySpawnCard.DoSpawn(args.senderBody.corePosition, Quaternion.identity, spawnRequest);

            UnityEngine.Object.Destroy(bodySpawnCard);
        }

        [ConCommand(commandName = "vapi_spawn_as", flags = ConVarFlags.ExecuteOnServer, helpText = "Respawns you as the specified body prefab with the specified VariantDefs.\n" +
            "Mandatory Arguments:\n" +
            "[0] = bodyName\n" +
            "[1] = CharacterVariantDef name list. (Encapsulate in quotes (\"). Separate by \",\". Spaces are allowed.)")]
        private static void CCVapiSpawnAs(ConCommandArgs args)
        {
            if (!VariantSpawnManager.instance)
            {
                Debug.LogWarning($"VarianceAPI's Variant Spawning commands are not available when the VarianceAPI expansion is disabled.");
                return;
            }

            if (args.Count == 0)
            {
                Debug.Log("No arguments Given");
                return;
            }

            BodyIndex body = VAPIUtils.GetBodyIndex(args[0]);
            if(body == BodyIndex.None)
            {
                Debug.Log("No body could be found with that name");
                return;
            }

            GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(body);

            var provider = GetCharacterVariantProvider(body);
            if (provider == null)
            {
                Debug.Log($"CharacterBody with name {args[0]} does not have a CharacterVariantProvider");
                return;
            }

            if(!args.sender)
            {
                Debug.Log("Sender does not exist.");
                return;
            }

            CharacterMaster senderMaster = args.senderMaster;
            if(!senderMaster)
            {
                Debug.Log("Sender Master does not exist.");
                return;
            }

            var variantDefNamesAsListString = args.TryGetArgString(1) ?? "";
            string[] variantDefNames = variantDefNamesAsListString.Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<CharacterVariantDef> variantDefs = new List<CharacterVariantDef>();

            var availableVariants = provider.allVariants;
            foreach (string variantName in variantDefNames)
            {
                foreach (var variantDef in availableVariants)
                {
                    if (variantDef.cachedName.Contains(variantName, StringComparison.OrdinalIgnoreCase))
                    {
                        variantDefs.Add(variantDef);
                        break;
                    }
                }
            }
            variantDefs.Sort((a, b) =>
            {
                return ((int)a.characterVariantIndex).CompareTo(((int)b.characterVariantIndex));
            });

            senderMaster.bodyPrefab = bodyPrefab;

            RoR2.ConVar.BoolConVar stage1pod = ((RoR2.ConVar.BoolConVar)(typeof(Stage)).GetFieldCached("stage1PodConVar").GetValue(null));
            bool oldVal = stage1pod.value;
            stage1pod.SetBool(false);

            if(senderMaster.TryGetComponent<CharacterMasterVariantStorage>(out var variantStorage))
            {
                variantStorage.RemoveVariantStorageForCharacter(body);
            }

            var characterBody = senderMaster.Respawn(senderMaster.GetBody().footPosition, senderMaster.GetBody().transform.rotation, false);
            if(characterBody && characterBody.TryGetComponent<CharacterBodyVariantController>(out var variantController))
            {
                variantController.doNotRollForVariants = true;
                variantController.SetCharacterVariantDefsServer(variantDefs.ToArray());
            }
        }
        #endregion
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Writes a <see cref="NetworkCharacterVariantIndex"/> to <paramref name="writer"/>
        /// </summary>
        public static void Write(this NetworkWriter writer, NetworkCharacterVariantIndex index)
        {
            writer.WritePackedUInt32(index.i);
        }

        /// <summary>
        /// Reads a <see cref="NetworkCharacterVariantIndex"/> from <paramref name="reader"/>
        /// </summary>
        public static NetworkCharacterVariantIndex ReadNetworkCharacterVariantIndex(this NetworkReader reader)
        {
            return new NetworkCharacterVariantIndex() { i = reader.ReadPackedUInt32() };
        }

        /// <summary>
        /// Writes a <see cref="CharacterVariantIndex"/> to <paramref name="writer"/>
        /// </summary>
        public static void Write(this NetworkWriter writer, CharacterVariantIndex index)
        {
            writer.Write((NetworkCharacterVariantIndex)index);
        }

        /// <summary>
        /// Reads a <see cref="CharacterVariantIndex"/> from <paramref name="reader"/>
        /// </summary>
        public static CharacterVariantIndex ReadCharacterVariantIndex(this NetworkReader reader)
        {
            return (CharacterVariantIndex)reader.ReadNetworkCharacterVariantIndex();
        }
    }
}