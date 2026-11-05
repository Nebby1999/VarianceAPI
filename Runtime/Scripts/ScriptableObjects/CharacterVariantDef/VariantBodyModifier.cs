#nullable enable
using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A <see cref="IVariantBodyModifier"/> is a <see cref="IValidatable"/> and <see cref="ICloneable"/> interface used to define new Body modifiers. These implementations can be serialized in the editor via the <see cref="SerializeReference"/> attribute.
    /// <br></br>
    /// Modifications are done via the <see cref="ModifyBody(CharacterMaster)"/> method.
    /// <br></br>
    /// Must support cloning via the <see cref="ICloneable"/> implementation.
    /// <para></para>
    /// <b>Built In Body Modifiers</b>
    /// <list type="bullet">
    /// <item><see cref="LoadCharacterSoundbank"/>: Used to load an existing character's soundbank.</item>
    /// </list>
    /// </summary>
    public interface IVariantBodyModifier : IValidatable, ICloneable
    {
        /// <summary>
        /// Modify the <paramref name="body"/> here. This method should return a disposable object used to revert any changes applied to the body.
        /// </summary>
        public IDisposable? ModifyBody(CharacterBody body);
    }

    [Serializable]
    public struct LoadCharacterSoundbank : IVariantBodyModifier
    {
        private struct DisposableCharacterBankLoad : IDisposable
        {
            public AkBank loadedBankComponent;
            public void Dispose()
            {
                if (loadedBankComponent)
                {
                    UnityEngine.Object.Destroy(loadedBankComponent);
                }
            }
        }

        public CharacterVariantTarget loadFromBody;
        public object Clone()
        {
            return new LoadCharacterSoundbank { loadFromBody = (CharacterVariantTarget)loadFromBody.Clone() };
        }

        public IDisposable? ModifyBody(CharacterBody body)
        {
            if (loadFromBody == null || !loadFromBody.TryLoadCharacterComponent(out var characterBody))
            {
                return null;
            }

            if (!characterBody!.TryGetComponent<AkBank>(out var origBank))
            {
                return null;
            }

            var masterAkBank = body.gameObject.AddComponent<AkBank>();
            masterAkBank.data.WwiseObjectReference = origBank.data.WwiseObjectReference;

            return new DisposableCharacterBankLoad
            {
                loadedBankComponent = masterAkBank
            };
        }

        public void Validate()
        {
        }
    }
}