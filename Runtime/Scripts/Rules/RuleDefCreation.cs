#nullable enable
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VAPI.Rules
{
    internal static class RuleDefCreation
    {
        private static Dictionary<BodyIndex, Sprite> _bodyIndexToGeneratedSprite = new Dictionary<BodyIndex, Sprite>();

        public static RuleDef CreateRuleDefFromVariantPack(VAPIRuleCategoryDef variantPackCategory, ReadOnlyVariantPack variantPack, bool hideLobbyDisplay)
        {
            RuleDef ruleDef = CreateRuleDefForCategory(variantPackCategory, variantPack.identifier.Replace('.', '_'), variantPack.nameToken, hideLobbyDisplay);

            VAPIRuleChoiceDef onChoice = VAPIRuleChoiceDef.CreateAndAddToRule(ruleDef, choiceKind: true, variantPack.packIcon, null);
            onChoice.tooltipNameToken = variantPack.nameToken;
            onChoice.tooltipBodyToken = variantPack.descriptionToken;
            onChoice.variantPackIndex = variantPack.variantPackIndex;
            variantPack.src.packEnabledChoice = onChoice;
            ruleDef.MakeNewestChoiceDefault();

            VAPIRuleChoiceDef offChoice = VAPIRuleChoiceDef.CreateAndAddToRule(ruleDef, choiceKind: false, null, null);
            offChoice.tooltipNameToken = variantPack.nameToken;
            offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
            offChoice.tooltipBodyToken = variantPack.descriptionToken;
            offChoice.variantPackIndex = variantPack.variantPackIndex;

            return ruleDef;
        }

        internal static RuleDef CreateRuleDefForVariant(VAPIRuleCategoryDef categoryForBodyComponentVariants, CharacterVariantDef variant, bool hideLobbyDisplay, VAPIRuleChoiceDef requiredChoice)
        {
            CharacterBody targetBody = variant.targetBody.LoadCharacterComponent()!;
            VariantPackIndex associatedPackIndex = VariantPackManager.FindVariantPackIndex(variant);

            Sprite icon = GetCharacterSprite(targetBody);
            string ingameVariantName = GetInGameName(targetBody, variant);

            RuleDef ruleDef = CreateRuleDefForCategory(categoryForBodyComponentVariants, variant.cachedName, ingameVariantName, variant.isHidden);

            VAPIRuleChoiceDef onChoice = VAPIRuleChoiceDef.CreateAndAddToRule(ruleDef, true, icon, new BodyVariantPackContentRule { associatedBodyIndex = targetBody.bodyIndex, associatedPackIndex = associatedPackIndex, enabled = true });
            onChoice.tooltipNameToken = ingameVariantName;
            onChoice.tooltipBodyToken = "VAPI_RULE_VARIANT_ON_DESCRIPTION";
            onChoice.variantIndex = variant.characterVariantIndex;
            onChoice.tiedPackEnabledChoice = VariantPackManager.GetVariantPack(associatedPackIndex).packEnabledChoice;
            AssignRequirementsToVariantChoice(onChoice, variant, requiredChoice);

            ruleDef.MakeNewestChoiceDefault();

            VAPIRuleChoiceDef offChoice = VAPIRuleChoiceDef.CreateAndAddToRule(ruleDef, false, null, new BodyVariantPackContentRule { associatedBodyIndex = targetBody.bodyIndex, associatedPackIndex = associatedPackIndex, enabled = false });
            offChoice.tooltipNameToken = ingameVariantName;
            offChoice.tooltipBodyToken = "VAPI_RULE_VARIANT_OFF_DESCRIPTION";
            offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
            offChoice.variantIndex = variant.characterVariantIndex;
            offChoice.tiedPackEnabledChoice = VariantPackManager.GetVariantPack(associatedPackIndex).packEnabledChoice;
            AssignRequirementsToVariantChoice(offChoice, variant, requiredChoice);

            return ruleDef;
        }

        internal static RuleDef CreateRuleDefForBody(VAPIRuleCategoryDef bodyVariantsCategory, CharacterBody bodyComponent, bool hideLobbyDisplay)
        {
            Sprite icon = GetCharacterSprite(bodyComponent);
            string ingameName = GetInGameName(bodyComponent, null);
            ExpansionDef? requiredExpansion = GetRequiredExpansion(bodyComponent);

            RuleDef ruleDef = CreateRuleDefForCategory(bodyVariantsCategory, bodyComponent.name, ingameName, hideLobbyDisplay);

            VAPIRuleChoiceDef onChoice = VAPIRuleChoiceDef.CreateAndAddToRule(ruleDef, true, icon, null);
            onChoice.tooltipNameToken = ingameName;
            onChoice.tooltipBodyToken = "Variants for this body will naturally spawn on the run";
            if(requiredExpansion != null)
            {
                onChoice.requiredExpansionDefs.Add(requiredExpansion);
                onChoice.requiredChoiceDefs.Add(requiredExpansion.enabledChoice);
            }

            ruleDef.MakeNewestChoiceDefault();

            VAPIRuleChoiceDef offChoice = VAPIRuleChoiceDef.CreateAndAddToRule(ruleDef, false, null, null);
            offChoice.tooltipNameToken = ingameName;
            offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
            offChoice.tooltipBodyToken = "Variants for this body will not naturally spawn on the run";
            if(requiredExpansion != null)
            {
                offChoice.requiredExpansionDefs.Add(requiredExpansion);
                offChoice.requiredChoiceDefs.Add(requiredExpansion.enabledChoice);
            }

            return ruleDef;
        }

        private static RuleDef CreateRuleDefForCategory(VAPIRuleCategoryDef targetCategory, string ruleName, string displayToken, bool hideLobbyDisplay)
        {
            RuleDef ruleDef = new RuleDef(targetCategory.baseNameForRules + ruleName, displayToken)
            {
                hideLobbyDisplay = hideLobbyDisplay,
            };
            RuleCatalogExtras.AddRuleToCatalog(ruleDef, targetCategory.ruleCategoryIndex);
            return ruleDef;
        }

        private static void AssignRequirementsToVariantChoice(VAPIRuleChoiceDef variantChoice, CharacterVariantDef variantDef, VAPIRuleChoiceDef bodyVariantsEnabledChoice)
        {
            List<UnlockableDef> GetRequiredUnlockableDefs()
            {
                List<UnlockableDef> unlockables = new List<UnlockableDef>();
                var spawnCondition = variantDef.spawnCondition;
                if (spawnCondition == null)
                    return unlockables;

                UnlockableDef? requiredUnlock = spawnCondition.requiredUnlock;
                if (requiredUnlock != null)
                    unlockables.Add(requiredUnlock);

                return unlockables;
            }

            List<ExpansionDef> GetRequiredExpansionDefs()
            {
                List<ExpansionDef> expansions = new List<ExpansionDef>();

                //Add the expansionDef of the body, if it exists
                if (!variantDef.targetBody.TryLoadCharacterComponent(out CharacterBody? characterBody))
                {
                    return expansions;
                }

                if (characterBody!.TryGetComponent<ExpansionRequirementComponent>(out var expansionRequirementComponent))
                {
                    expansions.Add(expansionRequirementComponent.requiredExpansion);
                }


                var spawnCondition = variantDef.spawnCondition;
                if (spawnCondition != null && spawnCondition.requiredExpansionDefs != null)
                {
                    foreach (var expansion in spawnCondition.requiredExpansionDefs)
                    {
                        if (expansion != null)
                            HG.ListUtils.AddIfUnique(expansions, expansion);
                    }
                }
                return expansions;
            }

            variantChoice.requiredUnlockables = GetRequiredUnlockableDefs();
            variantChoice.requiredExpansionDefs = GetRequiredExpansionDefs();
            variantChoice.requiredChoiceDefs.Add(variantChoice.tiedPackEnabledChoice!);
            variantChoice.requiredChoiceDefs.Add(bodyVariantsEnabledChoice);
            foreach(var expansion in variantChoice.requiredExpansionDefs)
            {
                variantChoice.requiredChoiceDefs.Add(expansion.enabledChoice);
            }
        }

        private static ExpansionDef? GetRequiredExpansion(CharacterBody body)
        {
            if(body.TryGetComponent<ExpansionRequirementComponent>(out var expansionRequirementComponent))
            {
                return expansionRequirementComponent.requiredExpansion;
            }
            return null;
        }

        private static Sprite GetCharacterSprite(CharacterBody? bodyComponent)
        {
            if(bodyComponent == null)
            {
                return GetMysteryIconSprite();
            }

            if(_bodyIndexToGeneratedSprite.TryGetValue(bodyComponent.bodyIndex, out Sprite generatedSprite))
            {
                return generatedSprite;
            }

            if(bodyComponent.portraitIcon != null && bodyComponent.portraitIcon is Texture2D icon)
            {
                generatedSprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), new Vector2(0.5f, 0.5f), 100);

                _bodyIndexToGeneratedSprite.Add(bodyComponent.bodyIndex, generatedSprite);
                return generatedSprite;
            }

            return GetMysteryIconSprite();
        }

        private static Sprite GetMysteryIconSprite() => Addressables.LoadAssetAsync<Sprite>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Common_MiscIcons.texUnlockIcon_png_texUnlockIcon_).WaitForCompletion();

        private static string GetInGameName(CharacterBody? body, CharacterVariantDef? variantDef)
        {
            if (body == null)
                return "???";

            if(variantDef == null || variantDef.variantNameProvider == null)
            {
                return Language.GetString(body.baseNameToken);
            }
            else
            {
                return variantDef.variantNameProvider.GetVariantName(Language.GetString(body.baseNameToken));
            }
        }
    }
}