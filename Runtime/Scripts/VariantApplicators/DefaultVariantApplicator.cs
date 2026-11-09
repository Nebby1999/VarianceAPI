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
    public sealed class DefaultVariantBodyApplicator : IVariantBodyApplicator
    {
        public sealed class MethodImpls
        {
            public void ApplyTierModifications(CharacterBodyVariantController src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                if(variantDef.variantTier != null)
                {
                    src.Log($"Applying {variantDef.cachedName}'s Tier's Modifiers (tier:{variantDef.variantTier.cachedName})");
                    disposableCollectionHelper.AddDisposable(variantDef.variantTier.ModifyBody(src.characterBody));
                }
            }

            public void ApplyBodyModifiers(CharacterBodyVariantController src, CharacterVariantDef variantDef, int bodyModifierIndex, DisposableCollectionHelper disposableCollectionHelper)
            {
                IVariantBodyModifier bodyModifier = variantDef.bodyModifiers[bodyModifierIndex];
                src.Log($"Applying {variantDef.cachedName}'s {(bodyModifier == null ? "NULL" : bodyModifier.GetType().Name)} Body Modifier");
                disposableCollectionHelper.AddDisposable(bodyModifier?.ModifyBody(src.characterBody));
            }

            public void ApplyBuffs(CharacterBodyVariantController src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                src.Log($"Applying {variantDef.cachedName}'s VariantBuffStorage");
                disposableCollectionHelper.AddDisposable(variantDef.variantBuffs.ApplyBuffs(src.characterBody));
            }

            public void ApplySkillReplacement(CharacterBodyVariantController src, CharacterVariantDef variantDef, int skillReplacementIndex, DisposableCollectionHelper disposableCollectionHelper)
            {
                if (src.characterBody.skillLocator == null)
                    return;
                
                VariantSkillReplacement skillReplacement = variantDef.skillReplacements[skillReplacementIndex];
                src.Log($"Applying {variantDef.cachedName} skill replacement for {src.characterBody.skillLocator.GetOrFindSkill(skillReplacement.slot, skillReplacement.slotName)}");
                disposableCollectionHelper.AddDisposable(skillReplacement.ApplySkillReplacement(src.characterBody.skillLocator));
            }

            public void ApplyCharacterDeathStateOverride(CharacterBodyVariantController src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                if(src.characterDeathBehavior != null)
                {
                    src.Log($"Applying {variantDef.cachedName} character death behaviour");
                    disposableCollectionHelper.AddDisposable(variantDef.deathStateOverride.ApplyDeathStateOverride(src.characterDeathBehavior));
                }
            }

            public void ApplyBodyComponents(CharacterBodyVariantController src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                src.Log($"Applying {variantDef.cachedName}'s body components");
                disposableCollectionHelper.AddDisposable(variantDef.additionalVariantComponents.ApplyComponents(src.characterBody));
            }

            public void ApplyModelComponents(CharacterBodyVariantController src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                if(src.characterModel != null)
                {
                    src.Log($"Applying {variantDef.cachedName}'s model components");
                    disposableCollectionHelper.AddDisposable(variantDef.additionalVariantComponents.ApplyComponents(src.characterModel));
                }
            }

            public void ApplyScaleModifiers(CharacterBodyVariantController src, ReadOnlyArray<CharacterVariantDef> variantDefs, DisposableCollectionHelper disposableCollectionHelper)
            {
                src.Log($"Applying scale modifier");
                var scaleModifier = new VariantSizeModifierApplicator(src.characterBody, variantDefs);
                scaleModifier.Apply();
                disposableCollectionHelper.AddDisposable(scaleModifier);
            }

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

            private struct DisposableNameOverride : IDisposable
            {
                public CharacterBodyVariantController src;
                public string originalName;
                public void Dispose()
                {
                    if(src)
                    {
                        src.cachedVariantName = originalName;
                    }
                }
            }

            public void ApplyNameModifiers(CharacterBodyVariantController src, ReadOnlyArray<CharacterVariantDef> variantDefs, DisposableCollectionHelper disposableCollectionHelper)
            {
                string baseName = Language.GetString(src.characterBody.baseNameToken);

                if (variantDefs.Length == 0)
                {
                    src.cachedVariantName = baseName;
                    return;
                }

                src.Log($"Applying name modifiers");
                using var _0 = ListPool<CharacterVariantDef>.RentCollection(out var sortedVariants);
                sortedVariants.AddRange(variantDefs);
                sortedVariants.Sort((a, b) =>
                {
                    bool aIsOverride = a.variantNameProvider is VariantNameOverride;
                    bool bIsOverride = b.variantNameProvider is VariantNameOverride;

                    return bIsOverride.CompareTo(aIsOverride);
                });

                foreach (var variant in sortedVariants)
                {
                    if (variant.variantNameProvider != null)
                    {
                        baseName = variant.variantNameProvider.GetVariantName(baseName);
                    }
                }
                src.cachedVariantName = baseName;
                disposableCollectionHelper.AddDisposable(new DisposableNameOverride { src = src, originalName = Language.GetString(src.characterBody.baseNameToken) });
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

            _methodImpls.ApplyNameModifiers(src, variants, _disposableCollectionHelper);
        }

        public void UnapplyVariants(CharacterBodyVariantController src)
        {
            _disposableCollectionHelper.Dispose();
        }
    }

    public sealed class DefaultVariantMasterApplicator : IVariantMasterApplicator
    {
        public sealed class MethodImpls
        {
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

            public void ApplyInventoryDefinition(CharacterMasterVariantStorage src, CharacterVariantDef variantDef, DisposableCollectionHelper disposableCollectionHelper)
            {
                //Apply channeled items && equipment info
                //Inventory definition returns if not server
                src.Log($"Applying {variantDef.cachedName}'s inventory definition");
                disposableCollectionHelper.AddDisposable(variantDef.inventoryDefinition.ApplyToInventory(src.characterMaster.inventory));
            }

            public void ApplyMasterModifier(CharacterMasterVariantStorage src, CharacterVariantDef srcDef, int modifierIndex, DisposableCollectionHelper disposableCollectionHelper)
            {
                //Base API modifiers return if not server
                IVariantMasterModifier masterModifier = srcDef.masterModifiers[modifierIndex];
                src.Log($"Applying {srcDef.cachedName}'s {(masterModifier == null ? "NULL" : masterModifier.GetType().Name)} Master Modifier");
                disposableCollectionHelper.AddDisposable(masterModifier?.ModifyMaster(src.characterMaster));
            }

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
