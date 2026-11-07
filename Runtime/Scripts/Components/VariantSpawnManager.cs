#nullable enable
using MSU;
using MSU.Config;
using RiskOfOptions.OptionConfigs;
using RoR2;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Networking;
using VAPI.Rules;
using static MSU.Config.ConfiguredVariable;

namespace VAPI
{
    /// <summary>
    /// The <see cref="VariantSpawnManager"/> is a Run Behaviour that handles the spawning of Variants.
    /// <para></para>
    /// This behaviour only exists if <see cref="VAPIContent.VarianceAPI"/> is enabled in the run.
    /// <para></para>
    /// This component subscribes to the body's start event to turn it into a variant. It has mainly <see cref="Xoroshiro128Plus"/> instances on the server to manage specific server related RNG.
    /// <br></br>
    /// If you do not wish for the VariantSpawnManager to handle the variants for a scripted summon, consider using <see cref="VariantMasterSummon"/> or <see cref="VariantDirectorSpawnRequest"/>.
    /// </summary>
    [RequireComponent(typeof(NetworkParent))]
    public sealed class VariantSpawnManager : MonoBehaviour
    {
        private static readonly ConVarFlags _flags =
#if DEBUG
            ConVarFlags.None
#else
            ConVarFlags.Cheat
#endif
            ;

        private static BoolConVar _logSpawnManager = new BoolConVar("vapi_cv_logspawnmanager", _flags, "0", "Enables logging for the VariantSpawnManager");
        /// <summary>
        /// The config for the Artifact of Variance's multiplier
        /// </summary>
        [FormatToken("VAPI_ARTIFACT_VARIANCE_DESC")]
        [AutoConfig]
        public static ConfiguredFloat artifactSpawnRateMultiplier = new ConfiguredFloat(2)
        {
            section = "General",
            description = "Multiplier that's applied to the spawn chance of variants when the Artifact of Variance is Enabled.",
            configFileIdentifier = VAPIConfig.GENERAL,
            sliderType = ConfiguredFloat.SliderTypeEnum.Normal,
            sliderConfig = new SliderConfig
            {
                FormatString = "{0:0.0}",
                min = 0,
                max = 100,
                checkIfDisabled = () => !VAPIConfig._enableArtifactOfVariance
            }
        };

        /// <summary>
        /// Returns the singleton instance of the manager
        /// </summary>
        public static VariantSpawnManager? instance => _instance;
        private static VariantSpawnManager? _instance;

        private CharacterVariantMask availableVariants;

        /// <summary>
        /// The base RNG for the other RNG classes in this manager, available only on the server
        /// </summary>
        public Xoroshiro128Plus? variantRng { get; private set; }

        /// <summary>
        /// The RNG handling variant spawns, available only on the server
        /// </summary>
        public Xoroshiro128Plus? variantSpawnRng { get; private set; }

        /// <summary>
        /// The RNG handling variant rewards, available only on the server
        /// </summary>
        public Xoroshiro128Plus? variantRewardRng { get; private set; }

        /// <summary>
        /// Returns true if <paramref name="variantIndex"/> is enabled in the run.
        /// </summary>
        public bool IsVariantAvailable(CharacterVariantIndex variantIndex) => availableVariants.Contains(variantIndex);

        private void Awake()
        {
            Run.instance.networkRuleBookComponent.onRuleBookUpdated += OnRulebookUpdated;
            availableVariants = CharacterVariantMask.Rent();
            Log("Awake");
        }

        private void OnRulebookUpdated(NetworkRuleBook book)
        {
            if (book != null)
            {
                RuleBook ruleBook = book.ruleBook;
                ruleBook.GenerateCharacterVariantMask(availableVariants);
                Log($"Rulebook Updated");
            }
        }

        private void OnDestroy()
        {
            if (Run.instance && Run.instance.networkRuleBookComponent)
            {
                Run.instance.networkRuleBookComponent.onRuleBookUpdated -= OnRulebookUpdated;
            }
            CharacterVariantMask.Return(availableVariants);
            Log("Destroyed");
        }

        private void Start()
        {
            if (NetworkServer.active)
            {
                ulong seed = Run.instance.seed;
                variantRng = new Xoroshiro128Plus(seed);
                Log($"VariantRNG Seed: {seed}");
                variantSpawnRng = new Xoroshiro128Plus(variantRng.nextUlong);
                variantRewardRng = new Xoroshiro128Plus(variantRng.nextUlong);
            }

            OnRulebookUpdated(Run.instance.networkRuleBookComponent);
        }

        private void OnEnable()
        {
            CharacterBody.onBodyStartGlobal += OnBodyStart;

            SingletonHelper.Assign(ref _instance, this);
            Log("Disabled");
        }

        private void OnDisable()
        {
            CharacterBody.onBodyStartGlobal -= OnBodyStart;

            SingletonHelper.Unassign(ref _instance, this);
            Log("Enabled");
        }

        private float GetSpawnRateMultiplier()
        {
            return RunArtifactManager.instance.IsArtifactEnabled(VAPIContent.Artifacts.Variance.asset) ? artifactSpawnRateMultiplier : 1f;
        }

        private enum BodyStartProcedure
        {
            SetFromStorage,
            RollVariants,
            Return
        }

        private struct BodyStartProcedureRetrievedValues
        {
            public CharacterVariantProvider? variantProvider;
            public CharacterBodyVariantController? bodyController;
            public CharacterMasterVariantStorage? masterVariantStorage;
            public CharacterVariantDef[]? additiveVariantDefs;
            public float? spawnRateMultiplier;
        }

        private void OnBodyStart(CharacterBody body)
        {
            //When the body starts, check it's master for a list of provided variants.
            //If masterless, Get the variants from the VariantProvider utilizing the CharacterBody index.
            //Basically, Master is the main source, if no master, fallback to Provider.

            if (!NetworkServer.active)
                return;

            BodyStartProcedure procedure = GetProcedureForBody(body, out BodyStartProcedureRetrievedValues values);

            switch (procedure)
            {
                case BodyStartProcedure.Return:
                    {
                        return;
                    }
                case BodyStartProcedure.RollVariants:
                    {
                        var args = new CharacterVariantProvider.RollVariantDefsArgs
                        {
                            rng = variantSpawnRng!,
                            spawnChanceMultiplier = 1 * (GetSpawnRateMultiplier() * (values.spawnRateMultiplier ?? 1)),
                            additiveVariantDefs = values.additiveVariantDefs,
                            rollType = values.additiveVariantDefs == null ? CharacterVariantProvider.RollType.Default : CharacterVariantProvider.RollType.NonUnique,
                        };
                        Log($"Rolling Variants for {body} with the following arguments:\r\n{args.ToString()}");
                        CharacterVariantDef[] rolledVariants = values.variantProvider!.RollVariantDefs(new CharacterVariantProvider.RollVariantDefsArgs { rng = variantSpawnRng!, spawnChanceMultiplier = GetSpawnRateMultiplier() });
                        Log($"Assigning {rolledVariants.Length} variants to {body}");
                        values.bodyController!.SetCharacterVariantDefsServer(rolledVariants);
                        return;
                    }
                case BodyStartProcedure.SetFromStorage:
                    {
                        if (values.masterVariantStorage!.TryGetVariantsStoredForCharacter(body.bodyIndex, out var variants))
                        {
                            values.bodyController!.SetCharacterVariantDefsServer(variants!);
                        }
                        return;
                    }
            }
        }

        //This method retrieves the procedure for variant spawning, basically what the spawn manager needs to do depending on the metadata that the body and it's master contains
        private BodyStartProcedure GetProcedureForBody(CharacterBody body, out BodyStartProcedureRetrievedValues retrievedValues)
        {
            retrievedValues = default;
            if (!body)
            {
                return BodyStartProcedure.Return;
            }

            CharacterVariantProvider? retrievedProvider = CharacterVariantCatalog.GetCharacterVariantProvider(body.bodyIndex);
            if (retrievedProvider == null)
            {
                Log($"Returning BodyStartProcedure.Return as {body.name} has no CharacterVariantProvider");
                return BodyStartProcedure.Return;
            }

            retrievedValues.variantProvider = retrievedProvider;

            if (!body.TryGetComponent<CharacterBodyVariantController>(out var bodyVariantController))
            {
                VAPILog.Warning($"CharacterBody {body} has a CharacterVariantProvider, but the body prefab lacks the variant controller! this shouldnt be possible!");
                Log($"Returning BodyStartProcedure.Return as {body.name} has no CharacterBodyVariantController");
                return BodyStartProcedure.Return;
            }

            retrievedValues.bodyController = bodyVariantController;

            if (bodyVariantController.doNotRollForVariants)
            {
                Log($"Returning BodyStartProcedure.Return as {bodyVariantController}'s doNotRollForVariants is set to true");
                return BodyStartProcedure.Return;
            }

            CharacterMasterVariantStorage? variantStorage = bodyVariantController.GetCharacterMasterVariantStorage();
            if (variantStorage != null && variantStorage.HasVariantsForBody(body.bodyIndex))
            {
                retrievedValues.masterVariantStorage = variantStorage;
                Log($"Returning BodyStartProcedure.SetFromStorage as {body.name}'s master has a CharacterMasterVariantStorage with variants for {BodyCatalog.GetBodyName(body.bodyIndex)}");
                return BodyStartProcedure.SetFromStorage;
            }

            if (bodyVariantController.HasPreAssignedSpawnData())
            {
                var preSpawnData = bodyVariantController.ConsumePreAssignedSpawnData();
                retrievedValues.additiveVariantDefs = preSpawnData.additiveVariantDefs;
                retrievedValues.spawnRateMultiplier = preSpawnData.spawnRateMultiplier;
            }

            Log($"Returning BodyStartProcedure.RollVariants for {body.name}");
            return BodyStartProcedure.RollVariants;
        }

        private void Log(object data)
        {
            if (_logSpawnManager.value)
            {
                VAPILog.Message($"[{gameObject.name}.VariantSpawnManager]: {data}");
            }
        }
    }
}