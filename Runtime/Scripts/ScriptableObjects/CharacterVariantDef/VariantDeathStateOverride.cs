#nullable enable
using EntityStates;
using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// Struct that represents an override for a character's death state. Can be cloned
    /// </summary>
    [Serializable]
    public struct VariantDeathStateOverride : ICloneable
    {
        private struct DisposableVariantDeathStateOverride : IDisposable
        {
            private CharacterDeathBehavior? _characterDeathBehaviour;
            private SerializableEntityStateType _originalDeathState;

            public DisposableVariantDeathStateOverride(CharacterDeathBehavior deathBehaviour, SerializableEntityStateType originalDeathState)
            {
                _characterDeathBehaviour = deathBehaviour;
                _originalDeathState = originalDeathState;
            }

            public void Dispose()
            {
                if(_characterDeathBehaviour != null)
                {
                    _characterDeathBehaviour.deathState = _originalDeathState;
                }
            }
        }

        [Tooltip("A death state to use when this variant dies.")]
        public SerializableEntityStateType deathStateOverride;

        /// <summary>
        /// Applies the <see cref="deathStateOverride"/> to <paramref name="characterDeathBehaviour"/>.
        /// <br></br>
        /// Returns null if <paramref name="characterDeathBehaviour"/> is null or <see cref="deathStateOverride"/> resulves to null. Otherwise returns a disposable object used to revert the death state change.
        public IDisposable? ApplyDeathStateOverride(CharacterDeathBehavior characterDeathBehaviour)
        {
            if (characterDeathBehaviour == null)
                return null;

            Type type = deathStateOverride.stateType;
            if (type == null)
                return null;

            var result = new DisposableVariantDeathStateOverride(characterDeathBehaviour, characterDeathBehaviour.deathState);
            characterDeathBehaviour.deathState = deathStateOverride;
            return result;
        }

        /// <summary>
        /// Clones the metadata in this into a new instance of <see cref="VariantComponentCollection"/>
        /// </summary>
        public object Clone()
        {
            return new VariantDeathStateOverride
            {
                deathStateOverride = new SerializableEntityStateType(deathStateOverride.typeName)
            };
        }

        /// <summary>
        /// Constructor. Sets the death state override to <paramref name="stateType"/>
        /// </summary>
        public VariantDeathStateOverride(SerializableEntityStateType stateType)
        {
            deathStateOverride = stateType;
        }

        /// <summary>
        /// Constructor. Sets the death state override to <paramref name="typeName"/>
        /// </summary>
        public VariantDeathStateOverride(string typeName)
        {
            deathStateOverride = new SerializableEntityStateType(typeName);
        }

        /// <summary>
        /// Constructor. Sets the death state override to <paramref name="stateType"/>
        /// </summary>
        public VariantDeathStateOverride(Type stateType)
        {
            deathStateOverride = new SerializableEntityStateType(stateType);
        }
    }
}