#nullable enable
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    public interface IVariantBuffInfoTimedApplication
    {
        public void ApplyAsTimedBuff(CharacterBody characterBody, BuffCountPair buffCountPair);
    }

    [Serializable]
    public struct LegacyTimedBuffApplication : IVariantBuffInfoTimedApplication
    {
        public float totalTimeForBuffs;
        public void ApplyAsTimedBuff(CharacterBody characterBody, BuffCountPair buffCountPair)
        {
            for (int i = 0; i < buffCountPair.buffCount; i++)
            {
                characterBody.AddTimedBuff(buffCountPair.buffDef, totalTimeForBuffs);
            }
        }
    }

    public struct BuffCountPair
    {
        public BuffDef buffDef;
        public int buffCount;
    }

    [Serializable]
    public struct VariantBuffInfo
    {
        public AddressReferencedBuffDef? buffDef;
        public int count;
        [SerializeReference, SubclassSelector]
        public IVariantBuffInfoTimedApplication? timedApplicationImpl;

        public bool TryGetBuffCountPair(out BuffCountPair asBuffCountPair)
        {
            asBuffCountPair = default;
            if (buffDef == null)
                return false;

            BuffDef buff = buffDef.LoadAssetNow();
            if (!buff)
                return false;

            asBuffCountPair = new BuffCountPair { buffDef = buff, buffCount = count };
            return true;
        }
    }

    public sealed class VariantBuffStorage
    {
        private readonly struct DisposableVariantBuffModifier : IDisposable
        {
            private readonly CharacterBody _characterBody;
            private readonly BuffCountPair[] _buffCountPairs;

            public DisposableVariantBuffModifier(CharacterBody characterBody, BuffCountPair[] buffCountPairs)
            {
                _characterBody = characterBody;
                _buffCountPairs = buffCountPairs;
            }

            public void Dispose()
            {
                if(!_characterBody)
                {
                    return;
                }

                for(int i = 0; i < _buffCountPairs.Length; i++)
                {
                    for(int j = 0; j < _buffCountPairs[i].buffCount; j++)
                    {
                        _characterBody.RemoveBuff(_buffCountPairs[j].buffDef);
                    }
                }
            }
        }

        public VariantBuffInfo[] buffInfos = Array.Empty<VariantBuffInfo>();

        private List<BuffCountPair> _appliedBuffCountPairs = new List<BuffCountPair>();

        public IDisposable? ApplyBuffs(CharacterBody characterBody)
        {
            if (!characterBody && !NetworkServer.active)
                return null;

            _appliedBuffCountPairs.Clear();
            for(int i = 0; i< buffInfos.Length; i++)
            {
                if (!buffInfos[i].TryGetBuffCountPair(out var buffCountPair))
                {
                    continue;
                }

                if (buffInfos[i].timedApplicationImpl != null)
                {
                    buffInfos[i].timedApplicationImpl?.ApplyAsTimedBuff(characterBody, buffCountPair);
                    continue;
                }

                for(int j = 0; j < buffCountPair.buffCount; j++)
                {
                    characterBody.AddBuff(buffCountPair.buffDef);
                }
                _appliedBuffCountPairs.Add(buffCountPair);
            }

            return new DisposableVariantBuffModifier(characterBody, _appliedBuffCountPairs.ToArray());
        }

        public VariantBuffStorage(VariantBuffInfo[] _buffInfos)
        {
            buffInfos = _buffInfos;
        }
        public VariantBuffStorage() { }
    }
}