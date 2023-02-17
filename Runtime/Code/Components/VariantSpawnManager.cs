using Moonstorm;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

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
        [ConfigurableField(VAPIConfig.general, ConfigSection = "VarianceAPI :: General", ConfigDesc = "Multiplier thats applied to the spawn chance of variants when the Artifact of Variance is enabled")]
        [TokenModifier("VAPI_ARTIFACT_VARIANCE_DESC", StatTypes.Default, 0)]
        public static float artifactSpawnRateMultiplier = 2f;
        /// <summary>
        /// The current instance of the VariantSpawnManager
        /// </summary>
        public static VariantSpawnManager Instance { get; private set; }
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
            for (int i = 0; i < pool.Length; i++)
            {
                var chance = pool[i].spawnRate * (RunArtifactManager.instance.IsArtifactEnabled(varianceArtifact) ? artifactSpawnRateMultiplier : defaultSpawnRateMultiplier);
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
                var spawnRate = Mathf.Min(100, currentDef.spawnRate * (RunArtifactManager.instance.IsArtifactEnabled(varianceArtifact) ? artifactSpawnRateMultiplier : defaultSpawnRateMultiplier));
                if (Util.CheckRoll(spawnRate))
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
