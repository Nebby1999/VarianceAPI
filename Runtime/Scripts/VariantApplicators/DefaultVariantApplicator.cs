#nullable enable
using HG;
using R2API.MiscHelpers;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// Default implementation for applying <see cref="CharacterVariantDef"/> to <see cref="CharacterBody"/>.
    /// <br></br>
    /// While this class cannot be inherited, the nested class <see cref="MethodImpls"/> expose a way to reuse the same logic as the default variant body applicator. When writing custom applicators it is recommended to reuse those methods unless you need a custom implementation.
    /// <para></para>
    /// <br></br>
    /// By default it implements the following modifiers when <see cref="IVariantBodyApplicator.ApplyVariants(CharacterBodyVariantController, ReadOnlyArray{CharacterVariantDef})"/> is called:
    /// <list type="number">
    /// <item><see cref="CharacterVariantDef.variantTier"/> modifiers</item>
    /// <item><see cref="CharacterVariantDef.bodyModifiers"/></item>
    /// <item><see cref="CharacterVariantDef.variantBuffs"/></item>
    /// <item><see cref="CharacterVariantDef.skillReplacements"/></item>
    /// <item><see cref="CharacterVariantDef.deathStateOverride"/></item>
    /// <item><see cref="CharacterVariantDef.additionalVariantComponents"/> that target the <see cref="CharacterBody"/></item>
    /// <item><see cref="CharacterVariantDef.additionalVariantComponents"/> that target the <see cref="CharacterModel"/></item>
    /// <item><see cref="CharacterVariantDef.scaleMultiplier"/></item>
    /// <item><see cref="CharacterVariantDef.visualModifier"/></item>
    /// <item><see cref="CharacterVariantDef.variantNameProvider"/></item>
    /// <item><see cref="CharacterVariantDef.arrivalToken"/></item>
    /// </list>
    /// <para></para>
    /// All modifiers are disposed when <see cref="IVariantBodyApplicator.UnapplyVariants(CharacterBodyVariantController)"/>
    /// </summary>
    public sealed class DefaultVariantBodyApplicator : IVariantBodyApplicator
    {
        /// <summary>
        /// Class that contains the default method implementations for applying modifiers.
        /// <br></br>
        /// When writing custom <see cref="IVariantBodyApplicator"/>, it is recommended to create an instance of this class and use it to apply modifiers.
        /// </summary>
        public sealed class MethodImpls
        {
            /// <summary>
            /// Applies the Tier Modifiers from <paramref name="variantDef"/>.
            /// </summary>
            /// <param name="src">The controller that needs the Tier Modifiers</param>
            /// <param name="variantDef">The variant to apply it's Tier Modifiers</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyTierModifications(CharacterBodyVariantController src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                if(variantDef.variantTier != null)
                {
                    src.Log($"Applying {variantDef.cachedName}'s Tier's Modifiers (tier:{variantDef.variantTier.cachedName})");
                    disposableCollectionHelper.AddDisposable(variantDef.variantTier.ModifyBody(src.characterBody));
                }
            }

            /// <summary>
            /// Applies the <see cref="IVariantBodyModifier"/>s from <paramref name="variantDef"/>
            /// </summary>
            /// <param name="src">The controller that needs the body modification</param>
            /// <param name="variantDef">The variant to apply one of it's body modifiers</param>
            /// <param name="bodyModifierIndex">The index of the body modifier to apply</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyBodyModifiers(CharacterBodyVariantController src, CharacterVariantDef variantDef, int bodyModifierIndex, DisposableCollectionHelper disposableCollectionHelper)
            {
                IVariantBodyModifier bodyModifier = variantDef.bodyModifiers[bodyModifierIndex];
                src.Log($"Applying {variantDef.cachedName}'s {(bodyModifier == null ? "NULL" : bodyModifier.GetType().Name)} Body Modifier");
                disposableCollectionHelper.AddDisposable(bodyModifier?.ModifyBody(src.characterBody));
            }

            /// <summary>
            /// Applies the <see cref="VariantBuffStorage"/> from <paramref name="variantDef"/>.
            /// </summary>
            /// <param name="src">The controller that needs the Buffs applied</param>
            /// <param name="variantDef">The variant to apply it's <see cref="VariantBuffStorage"/></param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyBuffs(CharacterBodyVariantController src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                src.Log($"Applying {variantDef.cachedName}'s VariantBuffStorage");
                disposableCollectionHelper.AddDisposable(variantDef.variantBuffs.ApplyBuffs(src.characterBody));
            }

            /// <summary>
            /// Applies the <see cref="VariantSkillReplacement"/>s from <paramref name="variantDef"/>
            /// </summary>
            /// <param name="src">The controller that needs the Skill Replacements</param>
            /// <param name="variantDef">The variant to apply one of it's Skill Replacements</param>
            /// <param name="skillReplacementIndex">The index of the SkillReplacement to apply</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplySkillReplacement(CharacterBodyVariantController src, CharacterVariantDef variantDef, int skillReplacementIndex, DisposableCollectionHelper disposableCollectionHelper)
            {
                if (src.characterBody.skillLocator == null)
                    return;
                
                VariantSkillReplacement skillReplacement = variantDef.skillReplacements[skillReplacementIndex];
                src.Log($"Applying {variantDef.cachedName} skill replacement for {src.characterBody.skillLocator.GetOrFindSkill(skillReplacement.slot, skillReplacement.slotName)}");
                disposableCollectionHelper.AddDisposable(skillReplacement.ApplySkillReplacement(src.characterBody.skillLocator));
            }

            /// <summary>
            /// Applies the <see cref="VariantDeathStateOverride"/> from <paramref name="variantDef"/>.
            /// </summary>
            /// <param name="src">The controller that needs the Death State Override</param>
            /// <param name="variantDef">The variant to apply it's Death State Override</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyCharacterDeathStateOverride(CharacterBodyVariantController src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                if(src.characterDeathBehavior != null)
                {
                    src.Log($"Applying {variantDef.cachedName} character death behaviour");
                    disposableCollectionHelper.AddDisposable(variantDef.deathStateOverride.ApplyDeathStateOverride(src.characterDeathBehavior));
                }
            }

            /// <summary>
            /// Applies the <see cref="VariantComponent"/>s from <paramref name="variantDef"/> that target the <see cref="CharacterBody"/>
            /// </summary>
            /// <param name="src">The controller that needs the Variant Components</param>
            /// <param name="variantDef">The variant to apply it's Components that target the <see cref="CharacterBody"/></param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyBodyComponents(CharacterBodyVariantController src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                src.Log($"Applying {variantDef.cachedName}'s body components");
                disposableCollectionHelper.AddDisposable(variantDef.additionalVariantComponents.ApplyComponents(src.characterBody));
            }

            /// <summary>
            /// Applies the <see cref="VariantComponent"/>s from <paramref name="variantDef"/> that target the <see cref="CharacterModel"/>
            /// </summary>
            /// <param name="src">The controller that needs the Variant Components</param>
            /// <param name="variantDef">The variant to apply it's Components that target the <see cref="CharacterBody"/></param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyModelComponents(CharacterBodyVariantController src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                if(src.characterModel != null)
                {
                    src.Log($"Applying {variantDef.cachedName}'s model components");
                    disposableCollectionHelper.AddDisposable(variantDef.additionalVariantComponents.ApplyComponents(src.characterModel));
                }
            }

            /// <summary>
            /// Applies the Scale Modifiers from <paramref name="variantDefs"/> to the <see cref="CharacterBody"/> from <paramref name="src"/>
            /// </summary>
            /// <param name="src">The controller that needs the Scale Modifiers</param>
            /// <param name="variantDefs">The variants to apply their scale modifiers</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyScaleModifiers(CharacterBodyVariantController src, ReadOnlyArray<CharacterVariantDef> variantDefs, DisposableCollectionHelper disposableCollectionHelper)
            {
                src.Log($"Applying scale modifier");
                var scaleModifier = new VariantSizeModifierApplicator(src.characterBody, variantDefs);
                scaleModifier.Apply();
                disposableCollectionHelper.AddDisposable(scaleModifier);
            }

            /// <summary>
            /// Applies the Visual Modifiers from <paramref name="visualModifiers"/> to the <see cref="CharacterModel"/> from <paramref name="src"/>
            /// <br></br>
            /// This is done via a coroutine that gets assigned to <paramref name="src"/>, it will apply the modifiers after the <see cref="SkinDef"/> has been applied
            /// </summary>
            /// <param name="src">The controller that needs the Visual Modifiers</param>
            /// <param name="visualModifiers">The visual modifiers to apply</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyVisualModifiersAfterSkin(CharacterBodyVariantController src, List<CharacterVariantVisualModifier> visualModifiers, DisposableCollectionHelper disposableCollectionHelper)
            {
                IEnumerator Coroutine(CharacterBodyVariantController owner, List<CharacterVariantVisualModifier> visualModifiers, DisposableCollectionHelper disposableCollectionHelper)
                {
                    var waitForEndOfFrame = new WaitForEndOfFrame();
                    while((owner.characterModel ? owner.characterModel!._hasSkinBeenApplied == false : false))
                    {
                        yield return waitForEndOfFrame;
                    }

                    bool shouldUpdateIDRS = false;
                    foreach(var visualModifier in visualModifiers)
                    {
                        disposableCollectionHelper.AddDisposable(visualModifier.ApplyVisualModifiers(src.characterModel!, src, out bool appliedMeshReplacements));
                        shouldUpdateIDRS |= appliedMeshReplacements;
                    }

                    if(shouldUpdateIDRS && src.characterModel != null && src.characterBody != null && src.characterBody.inventory != null)
                    {
                        src.characterModel.DisableAllItemDisplays();
                        src.characterModel.UpdateItemDisplay(src.characterBody.inventory);
                    }

                    yield return true;
                }

                if(visualModifiers.Count > 0 && src.characterModel != null)
                {
                    src.Log($"Applying Visual Modifiers after skin");
                    src.StartVisualModifierApplicationCoroutine(Coroutine(src, visualModifiers, disposableCollectionHelper));
                }
            }

            private struct DisableNameModifiersDisposable : IDisposable
            {
                public CharacterBodyVariantController src;
                public void Dispose()
                {
                    if(src != null)
                    {
                        src.nameModifiersActive = false;
                    }
                }
            }

            /// <summary>
            /// Activates the Name Modifiers for <paramref name="src"/>
            /// </summary>
            /// <param name="src">The controller to activate the name modifiers</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to</param>
            public void ActivateNameModifiers(CharacterBodyVariantController src, DisposableCollectionHelper disposableCollectionHelper)
            {
                src.nameModifiersActive = true;
                disposableCollectionHelper.AddDisposable(new DisableNameModifiersDisposable() { src = src });
            }

            /// <summary>
            /// Announces the arrival of the monster. Only gets announced if <paramref name="src"/>'s <see cref="CharacterBodyVariantController.supressArrivalMessageServer"/> is set to false and <see cref="VAPIConfig._sendArrivalMessages"/> is set to true
            /// </summary>
            /// <param name="src"></param>
            public void AnnounceArrival(CharacterBodyVariantController src)
            {
                if(src.supressArrivalMessageServer == false && VAPIConfig._sendArrivalMessages)
                {
                    src.AnnounceArrivalServer();
                }
            }
        }

        private MethodImpls _methodImpls = new MethodImpls();
        private DisposableCollectionHelper _disposableCollectionHelper = new DisposableCollectionHelper(disposeInReverseOrder: true);
        private List<CharacterVariantVisualModifier> _visualModifiers = new List<CharacterVariantVisualModifier>();

        public void ApplyVariants(CharacterBodyVariantController src, ReadOnlyArray<CharacterVariantDef> variants)
        {
            _visualModifiers.Clear();
            for (int i = 0; i < variants.Length; i++)
            {
                CharacterVariantDef variantDef = variants[i];

                _methodImpls.ApplyTierModifications(src, variantDef, _disposableCollectionHelper);

                for(int bodyModifierIndex = 0; bodyModifierIndex < variantDef.bodyModifiers.Length; bodyModifierIndex++)
                {
                    _methodImpls.ApplyBodyModifiers(src, variantDef, bodyModifierIndex, _disposableCollectionHelper);
                }

                _methodImpls.ApplyBuffs(src, variantDef, _disposableCollectionHelper);

                for (int skillReplacementIndex = 0; skillReplacementIndex < variantDef.skillReplacements.Length; skillReplacementIndex++)
                {
                    _methodImpls.ApplySkillReplacement(src, variantDef, skillReplacementIndex, _disposableCollectionHelper);
                }

                _methodImpls.ApplyCharacterDeathStateOverride(src, variantDef, _disposableCollectionHelper);

                _methodImpls.ApplyBodyComponents(src, variantDef, _disposableCollectionHelper);

                _methodImpls.ApplyModelComponents(src, variantDef, _disposableCollectionHelper);

                if(variantDef.visualModifier != null)
                {
                    _visualModifiers.Add(variantDef.visualModifier);
                }
            }

            _methodImpls.ApplyScaleModifiers(src, variants, _disposableCollectionHelper);

            _methodImpls.ApplyVisualModifiersAfterSkin(src, _visualModifiers, _disposableCollectionHelper);

            _methodImpls.ActivateNameModifiers(src, _disposableCollectionHelper);

            _methodImpls.AnnounceArrival(src);
        }

        public void UnapplyVariants(CharacterBodyVariantController src)
        {
            _disposableCollectionHelper.Dispose();
        }
    }

    /// <summary>
    /// Default implementation for applying <see cref="CharacterVariantDef"/> to <see cref="CharacterMaster"/>.
    /// <br></br>
    /// While this class cannot be inherited, the nested class <see cref="MethodImpls"/> expose a way to reuse the same logic as the default variant master applicator. When writing custom applicators it is recommended to reuse those methods unless you need a custom implementation.
    /// <para></para>
    /// <br></br>
    /// By default it implements the following modifiers when <see cref="IVariantMasterApplicator.ApplyVariants(CharacterMasterVariantStorage, ReadOnlyArray{CharacterVariantDef})"/> is called:
    /// <list type="number">
    /// <item><see cref="CharacterVariantDef.variantTier"/>'s master modifiers</item>
    /// <item><see cref="CharacterVariantDef.inventoryDefinition"/></item>
    /// <item><see cref="CharacterVariantDef.masterModifiers"/></item>
    /// <item><see cref="CharacterVariantDef.additionalVariantComponents"/> that target the <see cref="CharacterMaster"/></item>
    /// </list>
    /// <para></para>
    /// All modifiers are disposed when <see cref="IVariantMasterApplicator.UnapplyVariants(CharacterMasterVariantStorage, bool)"/>
    /// </summary>
    public sealed class DefaultVariantMasterApplicator : IVariantMasterApplicator
    {
        /// <summary>
        /// Class that contains the default method implementations for applying modifiers.
        /// <br></br>
        /// When writing custom <see cref="IVariantMasterApplicator"/>, it is recommended to create an instance of this class and use it to apply modifiers.
        /// </summary>
        public sealed class MethodImpls
        {
            /// <summary>
            /// Applies the tier modifications from <paramref name="variantDef"/>.
            /// </summary>
            /// <param name="src">The storage that needs the tier modifications</param>
            /// <param name="variantDef">The variant to apply it's tier modifications</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyTierModifications(CharacterMasterVariantStorage src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                //Apply Tier data.
                if (variantDef.variantTier != null)
                {
                    src.Log($"Applying {variantDef.cachedName}'s Tier's Modifiers (tier:{(!variantDef.variantTier ? "None" : variantDef.variantTier!.cachedName)})");
                    //Modify master returns if not server
                    disposableCollectionHelper.AddDisposable(variantDef.variantTier.ModifyMaster(src.characterMaster));
                }
            }

            /// <summary>
            /// Applies the <see cref="VariantInventoryDefinition"/> from <paramref name="variantDef"/>
            /// </summary>
            /// <param name="src">The storage that needs the inventory</param>
            /// <param name="variantDef">The variant to apply it's inventory definition</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyInventoryDefinition(CharacterMasterVariantStorage src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                //Apply channeled items && equipment info
                //Inventory definition returns if not server
                src.Log($"Applying {variantDef.cachedName}'s inventory definition");
                disposableCollectionHelper.AddDisposable(variantDef.inventoryDefinition.ApplyToInventory(src.characterMaster.inventory));
            }

            /// <summary>
            /// Applies the <see cref="IVariantMasterModifier"/>s from <paramref name="srcDef"/>
            /// </summary>
            /// <param name="src">The storage that needs the master modification</param>
            /// <param name="srcDef">The variant to apply one of it's master modifiers</param>
            /// <param name="modifierIndex">The index of the master modifier to apply</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyMasterModifier(CharacterMasterVariantStorage src, CharacterVariantDef srcDef, int modifierIndex, DisposableCollectionHelper disposableCollectionHelper)
            {
                //Base API modifiers return if not server
                IVariantMasterModifier masterModifier = srcDef.masterModifiers[modifierIndex];
                src.Log($"Applying {srcDef.cachedName}'s {(masterModifier == null ? "NULL" : masterModifier.GetType().Name)} Master Modifier");
                disposableCollectionHelper.AddDisposable(masterModifier?.ModifyMaster(src.characterMaster));
            }

            /// <summary>
            /// Applies the <see cref="VariantComponent"/>s stored in <paramref name="variantDef"/> that target the <see cref="CharacterMaster"/> of <paramref name="src"/>
            /// </summary>
            /// <param name="src">The storage that needs the components</param>
            /// <param name="variantDef">The variant to apply it's components to the master</param>
            /// <param name="disposableCollectionHelper">The DisposableCollectionHelper where the disposable modification will be added to.</param>
            public void ApplyComponents(CharacterMasterVariantStorage src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                //Apply components, the collection only adds components depending on the network context.
                src.Log($"Applying {variantDef.cachedName}'s master components");
                disposableCollectionHelper.AddDisposable(variantDef.additionalVariantComponents.ApplyComponents(src.characterMaster));
            }
        }

        private MethodImpls _methodImpls = new MethodImpls();
        private DisposableCollectionHelper _disposableCollectionHelper = new DisposableCollectionHelper(disposeInReverseOrder: true);

        public void ApplyVariants(CharacterMasterVariantStorage src, ReadOnlyArray<CharacterVariantDef> variants)
        {
            for(int i = 0; i < variants.Length; i++)
            {
                CharacterVariantDef variantDef = variants[i];

                _methodImpls.ApplyTierModifications(src, variantDef, _disposableCollectionHelper);
                _methodImpls.ApplyInventoryDefinition(src, variantDef, _disposableCollectionHelper);

                for(int modifierIndex = 0; modifierIndex < variantDef.masterModifiers.Length; modifierIndex++)
                {
                    _methodImpls.ApplyMasterModifier(src, variantDef, modifierIndex, _disposableCollectionHelper);
                }

                _methodImpls.ApplyComponents(src, variantDef, _disposableCollectionHelper);
            }
        }

        public void UnapplyVariants(CharacterMasterVariantStorage src, bool bodyIndexChanged)
        {
            _disposableCollectionHelper.Dispose();
        }
    }
}
