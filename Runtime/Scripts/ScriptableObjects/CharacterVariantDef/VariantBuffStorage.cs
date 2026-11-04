#nullable enable
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// Interface used to apply Buffs as a timed buff.
    /// <para>Must support cloning via the <see cref="ICloneable"/> interface.</para>
    /// <para>Built in implementations:</para>
    /// <list type="bullet">
    /// <item><see cref="LegacyTimedBuffApplication"/>: The original timed buff application of VAPI 2.0</item>
    /// </list>
    /// </summary>
    public interface IVariantBuffInfoTimedApplication : ICloneable
    {
        /// <summary>
        /// Apply the buff in <paramref name="buffCountPair"/> to <paramref name="characterBody"/> as a timed buff
        /// </summary>
        public void ApplyAsTimedBuff(CharacterBody characterBody, BuffCountPair buffCountPair);
    }

    /// <summary>
    /// Represents the original buff application of VAPI 2.0.
    /// <br></br>
    /// Adds all the buff counts with linear time.
    /// </summary>
    [Serializable]
    public struct LegacyTimedBuffApplication : IVariantBuffInfoTimedApplication
    {
        [Tooltip("How long all the buff stacks last")]
        public float totalTimeForBuffs;
        public void ApplyAsTimedBuff(CharacterBody characterBody, BuffCountPair buffCountPair)
        {
            for (int i = 0; i < buffCountPair.buffCount; i++)
            {
                characterBody.AddTimedBuff(buffCountPair.buffDef, totalTimeForBuffs);
            }
        }

        /// <summary>
        /// Returns a clone of this
        /// </summary>
        public object Clone()
        {
            return this;
        }
    }

    /// <summary>
    /// Represents a pair of <see cref="BuffDef"/>, alongside a specific integer value for it's <see cref="buffCount"/>
    /// </summary>
    public struct BuffCountPair
    {
        public BuffDef buffDef;
        public int buffCount;
    }

    /// <summary>
    /// Represents a buff to apply to a variant. Supports cloneability
    /// </summary>
    [Serializable]
    public struct VariantBuffInfo : ICloneable
    {
        [Tooltip("The buff to apply to this variant")]
        public AddressReferencedBuffDef? buffDef;
        [Tooltip("The total amount of buffs to apply")]
        public int count;
        [SerializeReference, SubclassSelector]
        [Tooltip("An implementation for applying buffs as a timed buff, can be left null.")]
        public IVariantBuffInfoTimedApplication? timedApplicationImpl;

        /// <summary>
        /// Tries to compute a <see cref="BuffCountPair"/> out of the metadata in this <see cref="VariantBuffInfo"/>
        /// <br></br>
        /// Returns false if <see cref="buffDef"/> is null, or it doesnt resolve to a valid <see cref="BuffDef"/>
        /// </summary>
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

        /// <summary>
        /// Clones this buff info
        /// </summary>
        public object Clone()
        {
            var result = new VariantBuffInfo
            {
                count = count,
            };
            if(timedApplicationImpl != null)
            {
                result.timedApplicationImpl = (IVariantBuffInfoTimedApplication)timedApplicationImpl.Clone();
            }
            result.buffDef = VAPIUtils.CloneAddressReferencedAsset<AddressReferencedBuffDef, BuffDef>(buffDef);
            return result;
        }

        /// <summary>
        /// Creates an instance of <see cref="VariantBuffInfo"/> with a specific buff name and count
        /// </summary>
        public static VariantBuffInfo Create(string buffName, int count = 1) => new VariantBuffInfo { buffDef = buffName, count = count };

        /// <summary>
        /// Creates an instance of <see cref="VariantBuffInfo"/> with a specific buff and count
        /// </summary>
        public static VariantBuffInfo Create(BuffDef buffDef, int count = 1) => new VariantBuffInfo { buffDef = buffDef, count = count };
    }

    /// <summary>
    /// Class that represents a collection of Buffs to apply to a variant. Can be cloned.
    /// </summary>
    [Serializable]
    public sealed class VariantBuffStorage : ICloneable
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

        /// <summary>
        /// The buff infos to apply to the character
        /// </summary>
        public VariantBuffInfo[] buffInfos = Array.Empty<VariantBuffInfo>();

        private List<BuffCountPair> _appliedBuffCountPairs = new List<BuffCountPair>();

        /// <summary>
        /// Applies the buffs stored in this to <paramref name="characterBody"/>.
        /// <br></br>
        /// Returns null if the body is null, or this method is called outside of a Server context.
        /// <br></br>
        /// Otherwise returns a Disposable interface to revert any buffs applied by this.
        /// </summary>
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

        /// <summary>
        /// Clones this buff storage's data into a new instance
        /// </summary>
        public object Clone()
        {
            var result = new VariantBuffStorage(new VariantBuffInfo[buffInfos.Length]);
            for(int i = 0; i < result.buffInfos.Length; i++)
            {
                result.buffInfos[i] = (VariantBuffInfo)buffInfos[i].Clone();
            }
            return result;
        }

        #region Constructors
        /// <summary>
        /// Constructor, encapuslates <paramref name="buffDef"/> and <paramref name="count"/> into a <see cref="VariantBuffInfo"/> then calls <see cref="VariantBuffStorage.VariantBuffStorage(VariantBuffInfo)"/>
        /// </summary>
        public VariantBuffStorage(BuffDef buffDef, int count = 1) : this(VariantBuffInfo.Create(buffDef, count)) { }
        /// <summary>
        /// Constructor, encapuslates <paramref name="buffName"/> and <paramref name="count"/> into a <see cref="VariantBuffInfo"/> then calls <see cref="VariantBuffStorage.VariantBuffStorage(VariantBuffInfo)"/>
        /// </summary>
        public VariantBuffStorage(string buffName, int count = 1) : this(VariantBuffInfo.Create(buffName, count)) { }
        /// <summary>
        /// Constructor, assigns <paramref name="buffInfo"/> into an array of BuffInfos then calls <see cref="VariantBuffStorage.VariantBuffStorage(VariantBuffInfo[])"/>
        /// </summary>
        public VariantBuffStorage(VariantBuffInfo buffInfo) : this(new VariantBuffInfo[1] { buffInfo })
        {
            buffInfos = new VariantBuffInfo[1] { buffInfo };
        }
        /// <summary>
        /// Constructor, sets the buffs to use with the buffs stored in <paramref name="_buffInfos"/>
        /// </summary>
        public VariantBuffStorage(VariantBuffInfo[] _buffInfos)
        {
            buffInfos = _buffInfos;
        }
        /// <summary>
        /// Parameterless Constructor
        /// </summary>
        public VariantBuffStorage() { }
        #endregion
    }
}