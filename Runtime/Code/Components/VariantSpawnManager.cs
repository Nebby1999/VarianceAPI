using Moonstorm;
using Moonstorm.Config;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using RiskOfOptions.OptionConfigs;

namespace VAPI.Components
{
    /// <summary>
    /// A Singleton monobehaviour that takes care of spawning Variants for a run, this behaviour is instanciated with the Run gameObject when the VarianceAPI expansion is enabled
    /// </summary>
    public class VariantSpawnManager : MonoBehaviour
    {
        /// <summary>
        /// The Artifact of Variance's SpawnRate multiplier
        /// </summary>
        [TokenModifier("VAPI_ARTIFACT_VARIANCE_DESC", StatTypes.Default, 0)]
#if !UNITY_EDITOR
        public static ConfigurableFloat artifactSpawnRateMultiplier = VAPIConfig.MakeConfigurableFloat(2f, (f) =>
        {
            f.Section = "General";
            f.Description = "Multiplier thats applied to the spawn chance of variants when the Artifact of Variance is enabled";
            f.ConfigFile = VAPIConfig.generalConfig;
            f.UseStepSlider = false;
            f.SliderConfig = new SliderConfig
            {
                formatString = "{0:0.0}",
                min = 0,
                max = 100,
                checkIfDisabled = () => !VAPIConfig.enableArtifactOfVariance
            };
        });
#else
        public static ConfigurableFloat artifactSpawnRateMultiplier;
#endif

        /// <summary>
        /// The current instance of the VariantSpawnManager
        /// </summary>
        public static VariantSpawnManager Instance { get; private set; }
        /// <summary>
        /// A Spawn Rate Multiplier applied to all variantDefs, this will never be a negative number.
        /// </summary>
        public float DefaultSpawnRateMultiplier { get => defaultSpawnRateMultiplier; set => defaultSpawnRateMultiplier = Mathf.Max(0, value); }
        [SerializeField] private float defaultSpawnRateMultiplier = 1;
        [SerializeField] private ArtifactDef varianceArtifact;
        /// <summary>
        /// Event raised when the VariantSpawnManager awakens
        /// </summary>
        public static event Action<VariantSpawnManager> OnAwake;
        /// <summary>
        /// Event raised when a Variant spawns
        /// </summary>
        public static event Action<ReadOnlyCollection<VariantDef>, GameObject> OnVariantSpawnedServer;
        /// <summary>
        /// Event raisedd when a Variant gets killed
        /// </summary>
        public static event Action<ReadOnlyCollection<VariantDef>, DamageReport> OnVariantKilledServer;
        private Xoroshiro128Plus variantRNG;

        private void Awake()
        {
            Run.onRunStartGlobal += CreateRNG;
            CharacterBody.onBodyStartGlobal += TryCreateVariant;
            OnAwake?.Invoke(this);
        }

        private void OnDestroy()
        {
            Run.onRunStartGlobal -= CreateRNG;
            CharacterBody.onBodyStartGlobal -= TryCreateVariant;
        }

        private void CreateRNG(Run run) => variantRNG = new Xoroshiro128Plus(run.seed);

        private void TryCreateVariant(CharacterBody obj)
        {
            if (!NetworkServer.active)
                return;

            if (obj.GetComponent<DoNotTurnIntoVariant>())
                return;

            BodyVariantDefProvider provider = VariantCatalog.GetBodyVariantDefProvider(obj.bodyIndex);

            if (provider == null)
                return;

            VariantDef[] variantsForBody = Roll(provider);

            if (variantsForBody == null)
                return;

            BodyVariantManager managerForBody = obj.GetComponent<BodyVariantManager>();

            if (!managerForBody)
                return;

            managerForBody.AddVariants(variantsForBody);

            BodyVariantReward rewards = obj.GetComponent<BodyVariantReward>();
            if (rewards)
            {
                rewards.AddVariants(variantsForBody);
            }
            OnVariantSpawnedServer?.Invoke(new ReadOnlyCollection<VariantDef>(variantsForBody), obj.gameObject);
        }

        private VariantDef[] Roll(BodyVariantDefProvider provider)
        {
            VariantDef[] uniques = provider.GetUniqueVariants(true);
            if (uniques != null && RollUniques(uniques, out VariantDef uniqueResult))
            {
                return new VariantDef[] { uniqueResult };
            }

            VariantDef[] notUniques = provider.GetVariants(true);
            if (notUniques != null && RollNotUniques(notUniques, out VariantDef[] result))
            {
                return result;
            }

            return null;
        }

        private bool RollUniques(VariantDef[] pool, out VariantDef result)
        {
            var uniqueRng = new WeightedSelection<int>();
            float notUniqueChance = 0f;
            var spawnRateMultiplier = RunArtifactManager.instance.IsArtifactEnabled(varianceArtifact) ? artifactSpawnRateMultiplier + defaultSpawnRateMultiplier : defaultSpawnRateMultiplier;
            for (int i = 0; i < pool.Length; i++)
            {
                var chance = pool[i].spawnRate * spawnRateMultiplier;
                uniqueRng.AddChoice(i, Mathf.Min(100, chance));
                notUniqueChance += Mathf.Max(0, 100 - chance);
            }
            uniqueRng.AddChoice(-1, notUniqueChance);

            var index = uniqueRng.Evaluate(variantRNG.nextNormalizedFloat);
            bool success = index != -1;

            result = success ? pool[index] : null;
#if DEBUG
            VAPILog.Info($"Unique Roll Success: {success}, Chosen Unique Index: {index}");
#endif
            return success;
        }

        private bool RollNotUniques(VariantDef[] pool, out VariantDef[] result)
        {
            List<VariantDef> defs = new List<VariantDef>();
            for (int i = 0; i < pool.Length; i++)
            {
                var currentDef = pool[i];
                var spawnRateMultiplier = RunArtifactManager.instance.IsArtifactEnabled(varianceArtifact) ? artifactSpawnRateMultiplier + defaultSpawnRateMultiplier : defaultSpawnRateMultiplier;
                var spawnRate = Mathf.Min(100, currentDef.spawnRate * spawnRateMultiplier);
                if (spawnRate <= 0)
                    continue;

                if(variantRNG.RangeFloat(0, 100) <= spawnRate)
                {
                    defs.Add(currentDef);
                }
            }
            var success = defs.Count != 0;

            result = success ? defs.ToArray() : null;
            return success;
        }


        private void OnEnable()
        {
            if (Instance)
                VAPILog.Error($"Only one VariantSpawnManager can exist at a time.");
            else
                Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        internal void OnVariantKilled(ReadOnlyCollection<VariantDef> variants, DamageReport damageReport)
        {
            OnVariantKilledServer?.Invoke(variants, damageReport);
        }
    }
}
