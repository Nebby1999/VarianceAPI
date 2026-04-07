#nullable enable
using HG;
using KinematicCharacterController;
using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    public sealed class VariantSizeModifierApplicator : IDisposable
    {
        private struct MotorCapsuleModifier : IDisposable
        {
            public readonly KinematicCharacterMotor motor;
            public readonly float originalRadius;
            public readonly float originalHeight;
            public readonly float originalYOffset;

            public MotorCapsuleModifier(KinematicCharacterMotor motor)
            {
                this.motor = motor;
                var capsule = motor.Capsule;
                originalRadius = capsule.radius;
                originalHeight = capsule.height;
                originalYOffset = motor.CapsuleYOffset;
            }

            public void Dispose()
            {
                if (!motor)
                    return;

                motor.SetCapsuleDimensions(originalRadius, originalHeight, originalYOffset);
            }
        }
        private float _computedSizeModifier;
        private CharacterBody? _characterBody;
        private MotorCapsuleModifier[]? _motorCapsuleModifiers;

        public void Dispose()
        {
            if(!_characterBody)
            {
                return;
            }

            if(TryGetModelTransform(out var modelTransform))
            {
                modelTransform!.localScale /= _computedSizeModifier;
            }

            if (_motorCapsuleModifiers == null)
                return;

            for(int i = 0; i < _motorCapsuleModifiers.Length; i++)
            {
                _motorCapsuleModifiers[i].Dispose();
            }
        }

        public void Apply()
        {
            if (!_characterBody)
            {
                return;
            }

            if(TryGetModelTransform(out var modelTransform))
            {
                modelTransform!.localScale *= _computedSizeModifier;
            }

            KinematicCharacterMotor[] motors = _characterBody!.GetComponents<KinematicCharacterMotor>();
            _motorCapsuleModifiers = new MotorCapsuleModifier[motors.Length];
            for(int i = 0; i < motors.Length; i++)
            {
                var motor = motors[i];
                _motorCapsuleModifiers[i] = new MotorCapsuleModifier(motor);

                var capsule = motor.Capsule;
                motor.SetCapsuleDimensions(capsule.radius * _computedSizeModifier, capsule.height * _computedSizeModifier, _computedSizeModifier);
            }
        }

        private bool TryGetModelTransform(out Transform? modelTransform)
        {
            modelTransform = null;
            if (!_characterBody)
                return false;

            if (!_characterBody!.modelLocator)
                return false;

            modelTransform = _characterBody!.modelLocator!.modelTransform;
            return modelTransform;
        }

        public VariantSizeModifierApplicator(CharacterBody targetBody, ReadOnlyArray<CharacterVariantDef> variantDefs)
        {
            _characterBody = targetBody;
            float highestSizeModifier = float.NegativeInfinity;
            float summedSizeModifier = 0f;
            foreach(var variantDef in variantDefs)
            {
                if(variantDef.scaleMultiplier > highestSizeModifier)
                {
                    highestSizeModifier = variantDef.scaleMultiplier;
                }
                summedSizeModifier += variantDef.scaleMultiplier;
            }

            if(variantDefs.Length <= 1)
            {
                _computedSizeModifier = highestSizeModifier;
            }
            else
            {
                summedSizeModifier -= highestSizeModifier;
                summedSizeModifier /= (variantDefs.Length - 1);

                _computedSizeModifier = highestSizeModifier + summedSizeModifier;
            }
        }
    }
}