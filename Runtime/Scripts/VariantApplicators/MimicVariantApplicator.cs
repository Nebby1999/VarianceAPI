#nullable enable

using HG;
using RoR2;
using System;
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
            //Use the disguise states to drive the applicator's "isDisguised" property
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

    /// <summary>
    /// An implementation of <see cref="IVariantBodyApplicator"/> thats assigned to DLC4's Mimic enemy (<see cref="DLC4Content.BodyPrefabs.MimicBody"/>).
    /// <para></para>
    /// This implementation opts to apply and unapply modifiers <b>depending</b> on wether the Mimic is currently Disguised as a Chest or not.
    /// <br></br>
    /// The modifiers that are applied when the mimic is <b>not</b> disguised are:
    /// <list type="bullet">
    /// <item><see cref="CharacterVariantDef.scaleMultiplier"/></item>
    /// <item><see cref="CharacterVariantDef.variantNameProvider"/>s</item>
    /// <item><see cref="CharacterVariantDef.visualModifier"/></item>
    /// </list>
    /// Once the Mimic goes back into hiding, the modifiers above are <b>unapplied</b>
    /// </summary>
    public sealed class MimicVariantBodyApplicator : MonoBehaviour, IVariantBodyApplicator
    {
        private CharacterBodyVariantController? _characterBodyVariantController;
        //Instance of method impls to handle the application of modifiers
        private DefaultVariantBodyApplicator.MethodImpls _defaultMethodImpls = new DefaultVariantBodyApplicator.MethodImpls();
        private ReadOnlyArray<CharacterVariantDef>? _variants;

        /// <summary>
        /// Event that's fired when <see cref="isDisguised"/> changes value.
        /// </summary>
        public event Action? onDisguisedChanged;

        /// <summary>
        /// Wether the mimic is currently disguised or not
        /// </summary>
        public bool isDisguised
        {
            get => _isDisguised;
            internal set
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
            //Store the variants and the controller
            _variants = defs;
            _characterBodyVariantController = src;

            //If we're disguised, apply the disguise safe modifiers
            if(isDisguised)
            {
                ApplyDisguiseSafeModifiers();
            }
            else //We're not disguised, so apply all of them
            {
                ApplyDisguiseSafeModifiers();
                ApplyDisguiseUnsafeModifiers();
            }
        }

        public void UnapplyVariants(CharacterBodyVariantController src)
        {
            //Unapply all modifiers 
            UnapplyDisguiseSafeModifiers();
            UnapplyDisguiseUnsafeModifiers();
            _characterBodyVariantController = null;
            _variants = null;
        }

        //"DisguiseSafe" means that the modifier _should not_ give away the fact that the chest is a mimic.
        //It has it's own DisposableCollectionHelper instance.
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

        //"DisguiseUnsage" means that the modifier will _very likely_ give away the fact that the chest is a mimic.
        //It has it's own DisposableCollectionHelper, which is disposed of when the mimic becomes disguised
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
            _defaultMethodImpls.ActivateNameModifiers(_characterBodyVariantController, _disguiseUnsafeDisposables);

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
            onDisguisedChanged?.Invoke();
        }
    }
}
