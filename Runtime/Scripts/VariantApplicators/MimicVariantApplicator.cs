#nullable enable

using HG;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    internal sealed class MimicVariantBodyApplicatorHooks
    {
        [SystemInitializer]
        private static void Init()
        {
            On.EntityStates.Mimic.Monster.IsDisguisedState.OnEnter += IsDisguisedState_OnEnter;
            On.EntityStates.Mimic.Monster.RemovedDisguiseState.OnEnter += RemovedDisguiseState_OnEnter;
            BodyCatalog.availability.CallWhenAvailable(() =>
            {
                var bodyPrefab = BodyCatalog.GetBodyPrefab(BodyCatalog.FindBodyIndex(nameof(DLC4Content.BodyPrefabs.MimicBody)));
                if(bodyPrefab)
                {
#if DEBUG
                    VAPILog.Info($"Applying MimicVariantBodyApplicator to {bodyPrefab}");
#endif
                    bodyPrefab.AddComponent<MimicVariantBodyApplicator>();
                }
            });
        }

        private static void RemovedDisguiseState_OnEnter(On.EntityStates.Mimic.Monster.RemovedDisguiseState.orig_OnEnter orig, EntityStates.Mimic.Monster.RemovedDisguiseState self)
        {
            orig(self);
            if(self.TryGetComponent<MimicVariantBodyApplicator>(out var applicator))
            {
                applicator.isDisguised = false;
            }
        }

        private static void IsDisguisedState_OnEnter(On.EntityStates.Mimic.Monster.IsDisguisedState.orig_OnEnter orig, EntityStates.Mimic.Monster.IsDisguisedState self)
        {
            orig(self);
            if(self.TryGetComponent<MimicVariantBodyApplicator>(out var applicator))
            {
                applicator.isDisguised = true;
            }
        }
    }

    public sealed class MimicVariantBodyApplicator : MonoBehaviour, IVariantBodyApplicator
    {
        private CharacterBodyVariantController? _characterBodyVariantController;
        private DefaultVariantBodyApplicator.MethodImpls _defaultMethodImpls = new DefaultVariantBodyApplicator.MethodImpls();
        private ReadOnlyArray<CharacterVariantDef>? _variants;

        internal bool isDisguised
        {
            get => _isDisguised;
            set
            {
                if(_isDisguised != value)
                {
                    _isDisguised = value;
                    OnIsDisguisedChanged();
                }
            }
        }
        private bool _isDisguised;

        public void ApplyVariants(CharacterBodyVariantController src, ReadOnlyArray<CharacterVariantDef> defs)
        {
            _variants = defs;
            _characterBodyVariantController = src;

            if(isDisguised)
            {
                ApplyDisguiseSafeModifiers();
            }
            else
            {
                ApplyDisguiseSafeModifiers();
                ApplyDisguiseUnsafeModifiers();
            }
        }

        public void UnapplyVariants(CharacterBodyVariantController src)
        {
            UnapplyDisguiseSafeModifiers();
            UnapplyDisguiseUnsafeModifiers();
            _characterBodyVariantController = null;
            _variants = null;
        }

        #region DisguiseSafe Modifiers
        private bool _hasAppliedDisguiseSafeModifiers;
        private DisposableCollectionHelper _disguiseSafeDisposables = new DisposableCollectionHelper(disposeInReverseOrder: true);
        private void ApplyDisguiseSafeModifiers()
        {
            if (_variants.HasValue == false || _characterBodyVariantController == null)
                return;

            for(int i = 0; i < _variants.Value.Length; i++)
            {
                CharacterVariantDef variant = _variants.Value[i];

                _defaultMethodImpls.ApplyTierModifications(_characterBodyVariantController, variant, _disguiseSafeDisposables);

                _defaultMethodImpls.ApplyTierModifications(_characterBodyVariantController, variant, _disguiseSafeDisposables);

                for (int bodyModifierIndex = 0; bodyModifierIndex < variant.bodyModifiers.Length; bodyModifierIndex++)
                {
                    _defaultMethodImpls.ApplyBodyModifiers(_characterBodyVariantController, variant, bodyModifierIndex, _disguiseSafeDisposables);
                }

                _defaultMethodImpls.ApplyBuffs(_characterBodyVariantController, variant, _disguiseSafeDisposables);

                for (int skillReplacementIndex = 0; skillReplacementIndex < variant.skillReplacements.Length; skillReplacementIndex++)
                {
                    _defaultMethodImpls.ApplySkillReplacement(_characterBodyVariantController, variant, skillReplacementIndex, _disguiseSafeDisposables);
                }

                _defaultMethodImpls.ApplyCharacterDeathStateOverride(_characterBodyVariantController, variant, _disguiseSafeDisposables);

                _defaultMethodImpls.ApplyBodyComponents(_characterBodyVariantController, variant, _disguiseSafeDisposables);

                _defaultMethodImpls.ApplyModelComponents(_characterBodyVariantController, variant, _disguiseSafeDisposables);
            }
            _hasAppliedDisguiseSafeModifiers = true;
        }

        private void UnapplyDisguiseSafeModifiers()
        {
            _disguiseSafeDisposables.Dispose();
            _hasAppliedDisguiseSafeModifiers = false;
        }
        #endregion

        #region DisguiseUnsafe Modifiers
        private bool _hasAppliedDisguiseUnsafeModifiers;
        private DisposableCollectionHelper _disguiseUnsafeDisposables = new DisposableCollectionHelper(disposeInReverseOrder: true);
        private void ApplyDisguiseUnsafeModifiers()
        {
            if (_variants.HasValue == false || _characterBodyVariantController == null)
                return;

            List<CharacterVariantVisualModifier> visualModifiers = new List<CharacterVariantVisualModifier>();
            for (int i = 0; i < _variants.Value.Length; i++)
            {
                CharacterVariantDef variant = _variants.Value[i];

                if(variant.visualModifier != null)
                {
                    visualModifiers.Add(variant.visualModifier);
                }
            }

            _defaultMethodImpls.ApplyScaleModifiers(_characterBodyVariantController, _variants.Value, _disguiseUnsafeDisposables);
            _defaultMethodImpls.ApplyVisualModifiersAfterSkin(_characterBodyVariantController, visualModifiers, _disguiseUnsafeDisposables);
            _defaultMethodImpls.ApplyNameModifiers(_characterBodyVariantController, _variants.Value, _disguiseUnsafeDisposables);

            _hasAppliedDisguiseUnsafeModifiers = true;
        }

        private void UnapplyDisguiseUnsafeModifiers()
        {
            _disguiseUnsafeDisposables.Dispose();
            _hasAppliedDisguiseUnsafeModifiers = false;
        }
        #endregion

        private void OnIsDisguisedChanged()
        {
            //If we're now disguised
            if(isDisguised == true)
            {
                //And we've applied unsafe modifiers, unapply them.
                if(_hasAppliedDisguiseUnsafeModifiers == true)
                {
                    UnapplyDisguiseUnsafeModifiers();
                }
            }
            else //If we're no longer disguised
            {
                //And we've not applied unsafe modifiers, apply them
                if(_hasAppliedDisguiseUnsafeModifiers == false)
                {
                    ApplyDisguiseUnsafeModifiers();
                }
            }
        }
    }
}
