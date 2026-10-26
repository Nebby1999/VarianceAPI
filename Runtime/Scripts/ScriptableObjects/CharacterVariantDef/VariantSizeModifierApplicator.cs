#nullable enable
using HG;
using KinematicCharacterController;
using RoR2;
using System;
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
        }

        /// <summary>
        /// Applies the size modifier
        /// </summary>
        public void Apply()
        {
            if (_characterBody == null || !NetworkServer.active)
            {
                return;
            }

            //Don't do anything if size coefficient is just 1
            if(Mathf.Approximately(_computedSizeCoefficient, 1))
            {
                return;
            }

            float realCoefficient = _computedSizeCoefficient;

            //If the computed size coefficient is greater than 1, then we need to increase the scale. With a value of 1.1 we dont wanna increase size by 110%, but rather just 10%.
            //If it's less, we dont need to reduce it by 1.
            if(_computedSizeCoefficient >= 1)
            {
                realCoefficient -= 1;
            }

            //A coefficient of 1.1 would turn into a scale increase percentage of 10. One of 0.5 would turn into a scale reduction percentage of 50;
            int scalePercentage = Mathf.FloorToInt(realCoefficient * 100);

            _usedBuffDef = _computedSizeCoefficient > 1 ? VAPIContent.Buffs.VAPIIncreaseScale.asset! : VAPIContent.Buffs.VAPIDecreaseScale.asset!;
            _buffStacks = scalePercentage;
            _characterBody.SetBuffCount(_usedBuffDef.buffIndex, _buffStacks);

            if(_modelLocator != null)
            {
                //Certainly this won't be bad
                _modelLocator.enabled = true;
                _modelLocator.autoUpdateModelTransform = true;
            }
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

            float baseMultiplier = 1;
            foreach(var variantDef in variantDefs)
            {
                if (variantDef.scaleMultiplier <= 0)
                    continue;

                baseMultiplier *= variantDef.scaleMultiplier;
            }

            if(float.IsInfinity(baseMultiplier))
            {
                _computedSizeCoefficient = 1;
                return;
            }

            _computedSizeCoefficient = Mathf.Max(float.Epsilon, _computedSizeCoefficient);

            if(targetBody.TryGetComponent<ModelLocator>(out var mdlLocator))
            {
                _modelLocator = mdlLocator;
            }
        }
    }
}