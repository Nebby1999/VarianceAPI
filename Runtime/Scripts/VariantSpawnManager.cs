//TODO: is this needed?
//Yes, cuz you can disable variants by disabling the expansion def.
#nullable enable
using MSU.Config;
using RiskOfOptions.OptionConfigs;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    [RequireComponent(typeof(NetworkParent))]
    public sealed class VariantSpawnManager : MonoBehaviour
    {

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

        public static VariantSpawnManager? instance => _instance;
        private static VariantSpawnManager? _instance;

        public Xoroshiro128Plus? variantRng { get; private set; }
        public Xoroshiro128Plus? variantSpawnRng { get; private set; }
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
            CharacterMaster.onStartGlobal += OnMasterStart;
            CharacterBody.onBodyStartGlobal += OnBodyStart;

            SingletonHelper.Assign(ref _instance, this);
        }
        private void OnDisable()
        {
            CharacterMaster.onStartGlobal -= OnMasterStart;
            CharacterBody.onBodyStartGlobal -= OnBodyStart;

            SingletonHelper.Unassign(ref _instance, this);
        }

        private float GetSpawnRateMultiplier()
        {
            return RunArtifactManager.instance.IsArtifactEnabled(VAPIContent.Artifacts.Variance.asset) ? artifactSpawnRateMultiplier : 1f;
        }

        private void OnBodyStart(CharacterBody body)
        {
            //When the body starts, check it's master for a list of provided variants.
            //If masterless, Get the variants from the VariantProvider utilizing the CharacterBody index.
            //Basically, Master is the main source, if no master, fallback to Provider.
            if (!NetworkServer.active)
                return;

            if(!body.TryGetComponent<CharacterBodyVariantController>(out var characterBodyVariantController))
            {
                return;
            }

            //If it succesfully gets the variants from the master, return.
            if (characterBodyVariantController.TryLinkCharacterMasterVariantStorage())
            {
                return;
            }

            if(!characterBodyVariantController.doNotRollForVariants)
            {
                return;
            }

            CharacterVariantProvider? provider = CharacterVariantCatalog.FindCharacterVariantProvider(body.bodyIndex);
            if(provider == null)
            {
                return;
            }

            //Character has no master, but the variant provider exists, proceed to roll the fallback variants.
            CharacterVariantDef[]? fallbackVariantDefs = provider.RollVariantDefs(new CharacterVariantProvider.RollVariantDefsArgs { rng = variantSpawnRng!, spawnChanceMultiplier = GetSpawnRateMultiplier() });

            if(fallbackVariantDefs == null || fallbackVariantDefs.Length == 0)
            {
                return;
            }

            characterBodyVariantController.SetFallbackCharacterVariantDefs(fallbackVariantDefs);

            //TODO: Impl event for master picking variants?
        }

        private void OnMasterStart(CharacterMaster master)
        {
            //When the master starts, Select the variants to utilize. and store them.
            //The variants are then stuck forever.
            //PlayercharacterMaster works slightly different, however.
            if (!NetworkServer.active)
                return;

            if(!master.TryGetComponent<CharacterMasterVariantStorage>(out var masterVariantStorage))
            {
                return;
            }

            //Do not turn into a variant if it's forbidden for this character.
            if(masterVariantStorage.doNotRollForVariants)
            {
                return;
            }

            CharacterVariantProvider? provider = CharacterVariantCatalog.FindCharacterVariantProvider(master.masterIndex);
            if(provider == null)
            {
                return;
            }

            //TODO: Impl variance artifact effect
            CharacterVariantDef[]? variantDefsForMaster = provider.RollVariantDefs(new CharacterVariantProvider.RollVariantDefsArgs { rng = variantSpawnRng!, spawnChanceMultiplier = GetSpawnRateMultiplier() });

            if(variantDefsForMaster == null || variantDefsForMaster.Length == 0)
            {
                return;
            }

            masterVariantStorage.SetVariantDefsForCharacter(variantDefsForMaster);

            //TODO: Impl event for master picking variants?
        }

    }
}