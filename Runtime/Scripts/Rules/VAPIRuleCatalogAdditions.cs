using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI.Rules
{
    internal struct BodyVariantPackContentRule
    {
        public BodyIndex associatedBodyIndex;
        public VariantPackIndex associatedPackIndex;
        public bool enabled;
    }

    //Handles adding the new rulebook categories
    internal static class RuleCatalogAdditions
    {
        //Selection of specific variant packs
        public static VAPIRuleCategoryDef variantPackCategory;

        //Allows for the disabling of entire variants for a body, clicking an icon as is opens a sub-category with the variants
        public static VAPIRuleCategoryDef bodyVariantsCategory;

        //Map of the Body to the actual variant categories for that body's variants
        public static Dictionary<BodyIndex, VAPIRuleCategoryDef> bodyToVariantCategories = new Dictionary<BodyIndex, VAPIRuleCategoryDef>();

        public static Dictionary<ReadOnlyVariantPack, RuleDef> variantPackToRule = new Dictionary<ReadOnlyVariantPack, RuleDef>(new ReadOnlyVariantPack.IdentifierComparer());
        public static Dictionary<CharacterVariantIndex, RuleDef> variantToRule = new Dictionary<CharacterVariantIndex, RuleDef>();

        public static RuleChoiceDef varianceAPIExpansionEnabledRuleChoiceDef;
        public static RuleDef varianceArtifactRuleDef;

        [SystemInitializer(typeof(RuleCatalog), typeof(CharacterVariantCatalog), typeof(CharacterVariantTierCatalog))]
        private static IEnumerator Init()
        {
            varianceAPIExpansionEnabledRuleChoiceDef = VAPIContent.VarianceAPI.asset.enabledChoice;

            yield return null;

            //We add the VariantPack category alongside it's rules.
            var coroutine = AddVariantPackCategoryAndRules();
            while (coroutine.MoveNext())
            {
                yield return null;
            }

            coroutine = AddPerBodyVariantCategoryAndRules();
            while(coroutine.MoveNext())
            {
                yield return null;
            }

            varianceArtifactRuleDef = RuleCatalog.FindRuleDef("Artifacts.Variance");

            RoR2.Language.onCurrentLanguageChanged += RecomputeTokenValues;

            VAPILog.Info($"VAPI Rulebook initialized");

            coroutine = RuleCatalogAdditionsUI.Init();
            while(coroutine.MoveNext())
            {
                yield return null;
            }

        }

        private static void RecomputeTokenValues()
        {

        }

        private static IEnumerator AddVariantPackCategoryAndRules()
        {
            yield return null;

            variantPackCategory = VAPIRuleCategoryDef.CreateRuleCategory("VARIANTPACKS", "VariantPacks", VariantPackCategoryHiddenTest);


            yield return null;

            for(int i = 0; i < VariantPackManager.registeredVariantPacks.Length; i++)
            {
                ReadOnlyVariantPack variantPack = VariantPackManager.registeredVariantPacks[i];
                
                yield return null;

                RuleDef packRule = RuleDefCreation.CreateRuleDefFromVariantPack(variantPackCategory, variantPack, variantPack.isHidden);
                variantPackToRule.Add(variantPack, packRule);
            }
        }

        private static IEnumerator AddPerBodyVariantCategoryAndRules()
        {
            yield return null;

            bodyVariantsCategory = VAPIRuleCategoryDef.CreateRuleCategory("VARIANTS", "Variants", BodyVariantsCategoryHiddenTest);

            //We will iterate thru the body components
            foreach(var bodyComponent in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                yield return null;

                //If the body has no variants, no need to make a rule for it.
                CharacterVariantProvider variantProviderForBody = CharacterVariantCatalog.GetCharacterVariantProvider(bodyComponent.bodyIndex);
                if (variantProviderForBody == null)
                    continue;

                //Since this body has variants, add it to the bodyVariantsCategory
                //We will mark it as not hidden if it has at least one non hidden variant.
                RuleDef bodyVariantsEnabledRuleDef = RuleDefCreation.CreateRuleDefForBody(bodyVariantsCategory, bodyComponent, HasAllVariantsHidden(variantProviderForBody));

                //We dont want the categories to be hidden, doing so makes it impossible to submit rules as is.
                var categoryForBodyComponentVariants = VAPIRuleCategoryDef.CreateRuleCategory("BODY_VARIANTS", $"BodyVariants.{bodyComponent.name}", RuleCatalog.HiddenTestFalse);
                categoryForBodyComponentVariants.displayToken = bodyComponent.baseNameToken;

                bodyToVariantCategories.Add(bodyComponent.bodyIndex, categoryForBodyComponentVariants);

                //Iterate thru each variant from the body
                foreach(var variant in variantProviderForBody.allVariants)
                {
                    yield return null;

                    //We want to include the Body's "global toggle for variants" as one of the required choices that need to be on
                    RuleDef variantRule = RuleDefCreation.CreateRuleDefForVariant(categoryForBodyComponentVariants, variant, variant.isHidden, (VAPIRuleChoiceDef)bodyVariantsEnabledRuleDef.FindChoice("On"));
                    variantToRule.Add(variant.characterVariantIndex, variantRule);
                }
            }
        }

        private static bool HasAllVariantsHidden(CharacterVariantProvider provider)
        {
            foreach(var variant in provider.allVariants)
            {
                if(variant.isHidden == false)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool VariantPackCategoryHiddenTest()
        {
            if (!PreGameController.instance || PreGameController.instance.readOnlyRuleBook == null)
                return true;

            bool isVAPIExpansionActive = PreGameController.instance.readOnlyRuleBook.IsChoiceActive(varianceAPIExpansionEnabledRuleChoiceDef);

            //Hide category is expansion not active
            return isVAPIExpansionActive == false;
        }

        private static bool BodyVariantsCategoryHiddenTest()
        {
            bool preGameControllerExists = PreGameController.instance;
            if (!preGameControllerExists)
                return true;

            //Outside of debug builds, show category if the user wants to see it
#if !DEBUG
                if (!VAPIConfig._showVariantRuleCategory)
                {
                    return true;
                }
#endif

            bool anyPackActive = false;
            foreach (var variantPack in VariantPackManager.registeredVariantPacks)
            {
                if (variantPack.isHidden)
                    continue;

                anyPackActive |= PreGameController.instance.readOnlyRuleBook.IsChoiceActive(variantPack.src.packEnabledChoice);
            }

            if (!anyPackActive)
                return true;

            bool isVAPIExpansionActive = PreGameController.instance.readOnlyRuleBook.IsChoiceActive(varianceAPIExpansionEnabledRuleChoiceDef);
            if (isVAPIExpansionActive)
                return false;

            return true;
        }
    }
}