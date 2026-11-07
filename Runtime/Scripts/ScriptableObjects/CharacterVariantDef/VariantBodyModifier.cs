#nullable enable
using RoR2;
using System;
using UnityEngine;
using VAPI.AddressReferencedAssets;

namespace VAPI
{
    /// <summary>
    /// A <see cref="IVariantBodyModifier"/> is a <see cref="IValidatable"/> and <see cref="ICloneable"/> interface used to define new Body modifiers. These implementations can be serialized in the editor via the <see cref="SerializeReference"/> attribute.
    /// <br></br>
    /// Modifications are done via the <see cref="ModifyBody(CharacterBody)"/> method.
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

    /// <summary>
    /// Loads an existing Character's Soundbank
    /// </summary>
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

        [Tooltip("The Character's soundbank to load")]
        public CharacterVariantTarget loadFromBody;


        /// <summary>
        /// Clones the metadata in this into a new instance of <see cref="LoadCharacterSoundbank"/>
        /// </summary>
        public object Clone()
        {
            return new LoadCharacterSoundbank { loadFromBody = (CharacterVariantTarget)loadFromBody.Clone() };
        }

        public IDisposable? ModifyBody(CharacterBody body)
        {
            if (loadFromBody == null || !loadFromBody.TryLoadCharacterComponent(out var characterBody))
            {
#if DEBUG
                VAPILog.Warning($"Not loading character sound bank as {loadFromBody} failed to resolve into a CharacterBody");
#endif
                return null;
            }

            if (!characterBody!.TryGetComponent<AkBank>(out var origBank))
            {
#if DEBUG
                VAPILog.Warning($"Not applying LoadCharacterSoundBank as {characterBody} has no AkBank component");
#endif
                return null;
            }

            var bodyAkBank = body.gameObject.AddComponent<AkBank>();
            bodyAkBank.data.WwiseObjectReference = origBank.data.WwiseObjectReference;

            return new DisposableCharacterBankLoad
            {
                loadedBankComponent = bodyAkBank
            };
        }

        public void Validate()
        {
        }

        /// <summary>
        /// Constructor. Sets the loadFromBody for this instance via <paramref name="addressOrCatalogName"/>
        /// </summary>
        /// <param name="addressOrCatalogName">The body address or catalog name to use</param>
        public LoadCharacterSoundbank(string addressOrCatalogName) : this(new AddressReferencedBody(addressOrCatalogName)) { }

        /// <summary>
        /// Constructor. Sets the loadFromBody for this instance via <paramref name="bodyPrefab"/>
        /// </summary>
        /// <param name="bodyPrefab">The body prefab name to use</param>
        public LoadCharacterSoundbank(GameObject bodyPrefab) : this(new AddressReferencedBody(bodyPrefab)) { }


        /// <summary>
        /// Constructor. Sets the loadFromBody for this instance via <paramref name="bodyReference"/>
        /// </summary>
        /// <param name="bodyReference">The body reference to use</param>
        public LoadCharacterSoundbank(AddressReferencedBody bodyReference) : this(new CharacterVariantTarget(bodyReference)) { }


        /// <summary>
        /// Constructor. Sets the loadFromBody for this instance via <paramref name="characterTarget"/>
        /// </summary>
        /// <param name="characterTarget">The target character to use</param>
        public LoadCharacterSoundbank(CharacterVariantTarget characterTarget)
        {
            loadFromBody = characterTarget;
        }
    }
}