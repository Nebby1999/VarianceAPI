#nullable enable
using HG;
using KinematicCharacterController;
using RoR2;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// Class that handles applying size modifiers to variants
    /// </summary>
    public sealed class VariantSizeModifierApplicator : IDisposable
    {
        private float _computedSizeCoefficient;

        private CharacterBody? _characterBody;
        private BuffDef? _usedBuffDef;
        private int _buffStacks;

        private ModelLocator? _modelLocator;
        private ModelLocatorSizeUpdater? _modelLocatorSizeUpdater;

        /// <summary>
        /// Disposes the size modifier, returning it to normal
        /// </summary>
        public void Dispose()
        {
            if(!NetworkServer.active || _characterBody == null || _usedBuffDef == null)
            {
                return;
            }

            _characterBody.SetBuffCount(_usedBuffDef.buffIndex, 0);
            UnityEngine.Object.Destroy(_modelLocatorSizeUpdater);
            _modelLocatorSizeUpdater = null;
        }

        /// <summary>
        /// Applies the size modifier
        /// </summary>
        public void Apply()
        {
            if (_characterBody == null)
            {
                LogWarning($"Unable to apply a size modifier of {_computedSizeCoefficient} as there's no characterBody");
                return;
            }

            //Don't do anything if size coefficient is just 1
            if (Mathf.Approximately(_computedSizeCoefficient, 1))
            {
                LogMessage($"Not applying size coefficient of {_computedSizeCoefficient} as it's approximately 1");
                return;
            }

            //If the model locator is not null, and its not enabled or doesnt auto update model transform, then we'll just drive the size ourselves.
            //This method gets called on all machines, and we want this to be added.
            if (_modelLocator != null && (_modelLocator.enabled == false || _modelLocator.autoUpdateModelTransform == false))
            {
                _modelLocatorSizeUpdater = _modelLocator.gameObject.AddComponent<ModelLocatorSizeUpdater>();
                LogMessage($"Added the ModelLocatorSizeUpdater to {_characterBody.gameObject}");
            }

            float realCoefficient = _computedSizeCoefficient;

            bool decreaseInsteadOfIncrease = false;
            //If the computed size coefficient is greater than 1, then we need to increase the scale. With a value of 1.1 we dont wanna increase size by 110%, but rather just 10%.
            //If it's less, we dont need to reduce it by 1.
            if (_computedSizeCoefficient >= 1)
            {
                realCoefficient -= 1;
                decreaseInsteadOfIncrease = false;
            }
            else
            {
                realCoefficient = _computedSizeCoefficient;
                decreaseInsteadOfIncrease = true;
            }

            //A coefficient of 1.1 would turn into a scale increase percentage of 10. One of 0.5 would turn into a scale reduction percentage of 50;
            int scalePercentage = decreaseInsteadOfIncrease ? 100 - Mathf.FloorToInt(realCoefficient * 100) : Mathf.FloorToInt(realCoefficient * 100);

            _usedBuffDef = decreaseInsteadOfIncrease ? VAPIContent.Buffs.VAPIDecreaseScale.asset! : VAPIContent.Buffs.VAPIIncreaseScale.asset!;
            _buffStacks = scalePercentage;

            if (NetworkServer.active)
                _characterBody.SetBuffCount(_usedBuffDef.buffIndex, _buffStacks);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetBody">The body that will have it's size modifier</param>
        /// <param name="variantDefs">The variant defs used to calculate the final size modifier</param>
        public VariantSizeModifierApplicator(CharacterBody targetBody, ReadOnlyArray<CharacterVariantDef> variantDefs)
        {
            _characterBody = targetBody;

            if (variantDefs.Length == 0)
            {
                _computedSizeCoefficient = 1;
                return;
            }

            float sizeMultiplier = 1;
            foreach(var variantDef in variantDefs)
            {
                if (variantDef.scaleMultiplier <= 0)
                {
                    LogWarning($"VariantDef {variantDef.cachedName} has a scaleMultiplier that's less or equal to 0. This will be ignored.");
                    continue;
                }

                sizeMultiplier *= variantDef.scaleMultiplier;
            }

            if(float.IsInfinity(sizeMultiplier))
            {
                LogWarning($"SizeModifier for {targetBody} with {variantDefs.Length} variants computed to an infinity size multiplier! setting size coefficient to 1");
                _computedSizeCoefficient = 1;
                return;
            }

            _computedSizeCoefficient = Mathf.Max(float.Epsilon, sizeMultiplier);

            if(targetBody.TryGetComponent<ModelLocator>(out var mdlLocator))
            {
                _modelLocator = mdlLocator;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogMessage(object data)
        {
#if DEBUG
            VAPILog.Message(data);
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogWarning(object data)
        {
#if DEBUG
            VAPILog.Warning(data);
#endif
        }
    }
}