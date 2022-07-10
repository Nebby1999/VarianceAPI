using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using R2API;
using UnityEngine.Networking;
using RoR2.ExpansionManagement;

namespace VAPI.Components
{
    public class VariantSpawnManager : MonoBehaviour
    {
        public static VariantSpawnManager Instance { get; private set; }

        public Xoroshiro128Plus variantRNG;
        public float spawnRateMultiplier;
        public static event Action<VariantSpawnManager> OnAwake;

        private void Awake()
        {
            variantRNG = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);

            CharacterBody.onBodyAwakeGlobal += TryCreateVariant;
            OnAwake?.Invoke(this);
        }

        private void TryCreateVariant(CharacterBody obj)
        {
            if (!NetworkServer.active)
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
            managerForBody.Apply();

            BodyVariantReward rewards = obj.GetComponent<BodyVariantReward>();
            if(rewards)
            {
                rewards.AddVariants(variantsForBody);
                rewards.Apply();
            }
        }

        private VariantDef[] Roll(BodyVariantDefProvider provider)
        {
            VariantDef[] uniques = provider.GetUniqueVariants(true);

            if(uniques != null && RollUniques(uniques, out VariantDef uniqueResult))
            {
                return new VariantDef[] { uniqueResult };
            }

            VariantDef[] notUniques = provider.GetVariants(true);
            if (notUniques != null && RollNotUniques(uniques, out VariantDef[] result))
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
                var chance = pool[i].spawnRate * spawnRateMultiplier;
                uniqueRng.AddChoice(i, Mathf.Min(100, chance));
                notUniqueChance += Mathf.Max(0, 100 - chance);
            }
            uniqueRng.AddChoice(-1, notUniqueChance);

            var index = uniqueRng.Evaluate(variantRNG.nextNormalizedFloat);
            bool success = index != -1;

            result = success ? pool[index] : null;
            return success;
        }

        private bool RollNotUniques(VariantDef[] pool, out VariantDef[] result)
        {
            List<VariantDef> defs = new List<VariantDef>();
            for(int i = 0; i < pool.Length; i++)
            {
                var currentDef = pool[i];
                var spawnRate = Mathf.Min(100, currentDef.spawnRate * spawnRateMultiplier);
                if (Util.CheckRoll(spawnRate))
                    defs.Add(currentDef);
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
            if(Instance == this)
            {
                Instance = null;
            }
        }
    }
}
