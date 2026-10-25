using EntityStates;
using RoR2;
using System;

namespace VAPI
{
    [Serializable]
    public struct VariantDeathStateOverride : ICloneable
    {
        private struct DisposableVariantDeathStateOverride : IDisposable
        {
            private CharacterDeathBehavior _characterDeathBehaviour;
            private SerializableEntityStateType _originalDeathState;

            public DisposableVariantDeathStateOverride(CharacterDeathBehavior deathBehaviour, SerializableEntityStateType originalDeathState)
            {
                _characterDeathBehaviour = deathBehaviour;
                _originalDeathState = originalDeathState;
            }

            public void Dispose()
            {
                if(_characterDeathBehaviour)
                {
                    _characterDeathBehaviour.deathState = _originalDeathState;
                }
            }
        }

        public SerializableEntityStateType deathStateOverride;

        public IDisposable ApplyDeathStateOverride(CharacterDeathBehavior characterDeathBehaviour)
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

        public object Clone()
        {
            return new VariantDeathStateOverride
            {
                deathStateOverride = new SerializableEntityStateType(deathStateOverride.typeName)
            };
        }

        public VariantDeathStateOverride(SerializableEntityStateType stateType)
        {
            deathStateOverride = stateType;
        }
        public VariantDeathStateOverride(string typeName)
        {
            deathStateOverride = new SerializableEntityStateType(typeName);
        }
        public VariantDeathStateOverride(Type stateType)
        {
            deathStateOverride = new SerializableEntityStateType(stateType);
        }
    }
}