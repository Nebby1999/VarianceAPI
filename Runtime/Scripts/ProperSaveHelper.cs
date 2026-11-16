#nullable enable
using MSU;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace VAPI
{
    //Internal class to have proper save support
    internal static class ProperSaveHelper
    {
        //Serializable representation of VariantSpawnManager's rng classes
        [Serializable]
        private sealed class SaveData
        {
            public Xoroshiro128PlusStates variantRNGStates;
            public Xoroshiro128PlusStates variantSpawnRNGStates;
            public Xoroshiro128PlusStates variantRewardRNGStates;

            public override string ToString()
            {
                using var _ = HG.StringBuilderPool.RentStringBuilder(out var sb);

                sb.AppendLine($"SaveData:");
                sb.AppendLine($"Variant RNG: {variantRNGStates.ToString()}");
                sb.AppendLine($"Variant Spawn RNG: {variantSpawnRNGStates.ToString()}");
                sb.AppendLine($"Variant Reward RGN: {variantRewardRNGStates.ToString()}");
                return sb.ToString();
            }
        }

        //A Xoroshiro128Plus state0 and state1 struct, can be created from existing xoroshiro instances or create new xoroshiro instances from the metadata
        [Serializable]
        private struct Xoroshiro128PlusStates
        {
            public static readonly Xoroshiro128PlusStates invalid = new Xoroshiro128PlusStates();
            public ulong state0;
            public ulong state1;

            public bool isValid { get; set; }

            private static FieldInfo? _state0FieldInfo;
            private static FieldInfo? _state1FieldInfo;

            public static Xoroshiro128PlusStates FromXoroshiroInstance(Xoroshiro128Plus? xoroshiroInstance)
            {
                if (xoroshiroInstance == null)
                    return invalid;

                _state0FieldInfo ??= R2API.Utils.Reflection.GetFieldCached<Xoroshiro128Plus>(nameof(state0));
                _state1FieldInfo ??= R2API.Utils.Reflection.GetFieldCached<Xoroshiro128Plus>(nameof(state1));

                var result = new Xoroshiro128PlusStates()
                {
                    state0 = (ulong)_state0FieldInfo.GetValue(xoroshiroInstance),
                    state1 = (ulong)_state1FieldInfo.GetValue(xoroshiroInstance),
                    isValid = true
                };

                return result;
            }

            public Xoroshiro128Plus? CreateXoroshiroInstance()
            {
                if (!isValid)
                    return null;

                _state0FieldInfo ??= R2API.Utils.Reflection.GetFieldCached<Xoroshiro128Plus>(nameof(state0));
                _state1FieldInfo ??= R2API.Utils.Reflection.GetFieldCached<Xoroshiro128Plus>(nameof(state1));

                Xoroshiro128Plus result = new Xoroshiro128Plus(0);
                _state0FieldInfo.SetValue(result, state0);
                _state1FieldInfo.SetValue(result, state1);

                return result;
            }

            public override string ToString()
            {
                return $"state0:{state0}|state1:{state1}";
            }
        }

        private const string KEY = "VAPI.VariantSpawnManagerData";

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SubscribeToProperSaveEvent()
        {
            if(VAPIMain.properSaveInstalled)
            {
                SubscribeInternal(subscribe: true);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void UnsubscribeToProperSaveEvent()
        {
            if(VAPIMain.properSaveInstalled)
            {
                SubscribeInternal(subscribe: false);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void SubscribeInternal(bool subscribe)
        {
            if(subscribe)
            {
                ProperSave.SaveFile.OnGatherSaveData -= GatherSaveData;
                ProperSave.SaveFile.OnGatherSaveData += GatherSaveData;
            }
            else
            {
                ProperSave.SaveFile.OnGatherSaveData -= GatherSaveData;
            }
        }

        public static bool TryInitRNG(ref Xoroshiro128Plus? variantRNG, ref Xoroshiro128Plus? variantSpawnRNG, ref Xoroshiro128Plus? variantRewardRNG)
        {
            if (!VAPIMain.properSaveInstalled)
                return false;

            SaveData? saveData = GetSaveData();

            if (saveData == null)
                return false;

            if (!saveData.variantRewardRNGStates.isValid ||
                !saveData.variantRNGStates.isValid ||
                !saveData.variantSpawnRNGStates.isValid)
                return false;

            VAPILog.Info($"Initializing RNG with {saveData}");

            variantRNG = saveData.variantRNGStates.CreateXoroshiroInstance();
            variantSpawnRNG = saveData.variantSpawnRNGStates.CreateXoroshiroInstance();
            variantRewardRNG = saveData.variantRewardRNGStates.CreateXoroshiroInstance();
            return true;
        }

        private static SaveData? GetSaveData() => ProperSave.Loading.CurrentSave?.TryGetModdedData<SaveData>(KEY);

        private static void GatherSaveData(Dictionary<string, object> dictionary)
        {
            if(!NetworkServer.active)
            {
                return;
            }

            var spawnManager = VariantSpawnManager.instance;
            if(spawnManager == null)
            {
                return;
            }

            SaveData saveData = new SaveData()
            {
                variantRNGStates = Xoroshiro128PlusStates.FromXoroshiroInstance(spawnManager.variantRng),
                variantSpawnRNGStates = Xoroshiro128PlusStates.FromXoroshiroInstance(spawnManager.variantSpawnRng),
                variantRewardRNGStates = Xoroshiro128PlusStates.FromXoroshiroInstance(spawnManager.variantRewardRng)
            };

            VAPILog.Info($"Saving data: {saveData}");
            dictionary[KEY] = saveData;
        }
    }
}