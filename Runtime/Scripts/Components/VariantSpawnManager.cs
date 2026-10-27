#nullable enable
using MSU;
using MSU.Config;
using RiskOfOptions.OptionConfigs;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using static MSU.Config.ConfiguredVariable;

namespace VAPI
{
    /// <summary>
    /// Class that manages the spawning of variants, this class is found within a Run behaviour thats only instantiated if <see cref="VAPIContent.VarianceAPI"/> is enabled.
    /// </summary>
    [RequireComponent(typeof(NetworkParent))]
    public sealed class VariantSpawnManager : MonoBehaviour
    {
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

        private void Start()
        {
            if(NetworkServer.active)
            {
                variantRng = new Xoroshiro128Plus(Run.instance.seed);
                variantSpawnRng = new Xoroshiro128Plus(variantRng.nextUlong);
                variantRewardRng = new Xoroshiro128Plus(variantRng.nextUlong);
            }
        }

        private void OnEnable()
        {
            //CharacterMaster.onStartGlobal += OnMasterStart;
            CharacterBody.onBodyStartGlobal += OnBodyStart;

            SingletonHelper.Assign(ref _instance, this);
        }

        private void OnDisable()
        {
            //CharacterMaster.onStartGlobal -= OnMasterStart;
            CharacterBody.onBodyStartGlobal -= OnBodyStart;

            SingletonHelper.Unassign(ref _instance, this);
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
        }

        private void OnBodyStart(CharacterBody body)
        {
            //When the body starts, check it's master for a list of provided variants.
            //If masterless, Get the variants from the VariantProvider utilizing the CharacterBody index.
            //Basically, Master is the main source, if no master, fallback to Provider.

            if (!NetworkServer.active)
                return;

            BodyStartProcedure procedure = GetProcedureForBody(body, out BodyStartProcedureRetrievedValues values);

            switch(procedure)
            {
                case BodyStartProcedure.Return:
                    {
                        VAPILog.Info($"Not turning {body} into a variant, it simply cannot be one.");
                        return;
                    }
                case BodyStartProcedure.RollVariants:
                    {
                        VAPILog.Info($"Rolling variants for {body} using the provider");
                        CharacterVariantDef[] rolledVariants = values.variantProvider!.RollVariantDefs(new CharacterVariantProvider.RollVariantDefsArgs { rng = variantSpawnRng!, spawnChanceMultiplier = 1f });
                        values.bodyController!.SetCharacterVariantDefsServer(rolledVariants);
                        return;
                    }
                case BodyStartProcedure.SetFromStorage:
                    {
                        VAPILog.Info($"Trying to Set the variants from the storage.");
                        if(values.masterVariantStorage!.TryGetVariantsStoredForCharacter(body.bodyIndex, out var variants))
                        {
                            VAPILog.Info($"Succesfully set the variants from the storage.");
                            values.bodyController!.SetCharacterVariantDefsServer(variants!);
                        }
                        return;
                    }
            }

            //If we cant roll the variants for the body that just started, return.
            /*if (CanRollVariantsForBody(body, out CharacterBodyVariantController? variantController, out CharacterVariantProvider? provider) == false)
            {
                return;
            }

            //We can roll variants for the body, so proceed to roll them.
            CharacterVariantDef[] variantDefs = provider!.RollVariantDefs(new CharacterVariantProvider.RollVariantDefsArgs { rng = variantSpawnRng!, spawnChanceMultiplier = GetSpawnRateMultiplier() });


            variantController!.SetCharacterVariantDefsServer(variantDefs);*/
        }

        private BodyStartProcedure GetProcedureForBody(CharacterBody body, out BodyStartProcedureRetrievedValues retrievedValues)
        {
            retrievedValues = default;
            if (!body)
                return BodyStartProcedure.Return;

            CharacterVariantProvider? retrievedProvider = CharacterVariantCatalog.GetCharacterVariantProvider(body.bodyIndex);
            if (retrievedProvider == null)
                return BodyStartProcedure.Return;

            retrievedValues.variantProvider = retrievedProvider;

            if(!body.TryGetComponent<CharacterBodyVariantController>(out var bodyVariantController))
            {
                VAPILog.Warning($"CharacterBody {body} has a CharacterVariantProvider, but the body prefab lacks the variant controller! this shouldnt be possible!");
                return BodyStartProcedure.Return;
            }

            retrievedValues.bodyController = bodyVariantController;

            if (bodyVariantController.doNotRollForVariants)
                return BodyStartProcedure.Return;

            CharacterMasterVariantStorage? variantStorage = bodyVariantController.GetCharacterMasterVariantStorage();
            if(variantStorage != null && variantStorage.HasVariantsForBody(body.bodyIndex))
            {
                retrievedValues.masterVariantStorage = variantStorage;
                return BodyStartProcedure.SetFromStorage;
            }

            return BodyStartProcedure.RollVariants;
        }


        private bool CanRollVariantsForBody(CharacterBody body, out CharacterBodyVariantController? characterBodyVariantController, out CharacterVariantProvider? characterVariantProvider)
        {
            characterBodyVariantController = null;
            characterVariantProvider = null;

            //This shouldnt even happen, fuck my stupid chungus life
            if (!body)
                return false;

            //Retrieve the provider, but don't assign it to the out variable yet, we'll only assign the out variables once we're bound to return true.
            CharacterVariantProvider? providerForBody = CharacterVariantCatalog.GetCharacterVariantProvider(body.bodyIndex);
            //If the body doesnt have a provider, return false.
            if (providerForBody == null)
            {
                return false;
            }

            //This shouldnt happen, log it if it does happen
            if(body.TryGetComponent<CharacterBodyVariantController>(out var controllerForBody) == false)
            {
                VAPILog.Warning($"CharacterBody {body} has a CharacterVariantProvider, but the body prefab lacks the variant controller! this shouldnt be possible!");
                return false;
            }

            //If the controller says the manager can't roll for variants, then dont.
            if (controllerForBody.doNotRollForVariants == true)
                return false;

            //If the body has a master storage, and the master storage has stored variants for the body, do not roll new ones.
            CharacterMasterVariantStorage? storage = controllerForBody.GetCharacterMasterVariantStorage();
            if(storage != null && storage.HasVariantsForBody(body.bodyIndex))
            {
                return false;
            }

            //All checks passed, set out variables and return true.
            characterBodyVariantController = controllerForBody;
            characterVariantProvider = providerForBody;
            return true;
        }
    }
}