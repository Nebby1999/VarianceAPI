using R2API;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        public static Dictionary<BodyIndex, RuleDef> bodyIndexToRule = new Dictionary<BodyIndex, RuleDef>();
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
            while (coroutine.MoveNext())
            {
                yield return null;
            }

            varianceArtifactRuleDef = RuleCatalog.FindRuleDef("Artifacts.Variance");

            RoR2.Language.onCurrentLanguageChanged += RecomputeTokenValues;

            VAPILog.Info($"VAPI Rulebook initialized");

            coroutine = RuleCatalogAdditionsUI.Init();
            while (coroutine.MoveNext())
            {
                yield return null;
            }
        }

        private static void RecomputeTokenValues()
        {
            foreach (var (variantIndex, ruleDef) in variantToRule)
            {
                CharacterVariantDef variantDef = CharacterVariantCatalog.GetCharacterVariantDef(variantIndex);
                string variantName = RuleDefCreation.GetInGameName(variantDef.targetBody.LoadCharacterComponent(), variantDef);

                ruleDef.displayToken = variantName;
                ruleDef.FindChoice("On").tooltipNameToken = variantName;
                ruleDef.FindChoice("Off").tooltipNameToken = variantName;
            }

            foreach (var (bodyIndex, ruleDef) in bodyIndexToRule)
            {
                CharacterBody bodyComponent = BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
                string bodyName = RuleDefCreation.GetInGameName(bodyComponent, null);

                ruleDef.displayToken = bodyName;
                ruleDef.FindChoice("On").tooltipNameToken = bodyName;
                ruleDef.FindChoice("Off").tooltipNameToken = bodyName;
            }

            LogMessage($"Finished recomputing token values");
        }

        public static void GenerateCharacterVariantMask(this RuleBook ruleBook, CharacterVariantMask variantMask)
        {
            variantMask.Clear();
            foreach (var (variantIndex, rule) in variantToRule)
            {
                ExtendedRuleChoiceDef onChoice = (ExtendedRuleChoiceDef)rule.FindChoice("On");

                //Don't add variants that are explicitly disabled
                if (ruleBook.IsChoiceActive(onChoice) == false)
                {
                    continue;
                }

                //Variant is enabled, and the dependencies are active, add
                variantMask.Add(variantIndex);
            }
        }

        private static IEnumerator AddVariantPackCategoryAndRules()
        {
            yield return null;

            variantPackCategory = VAPIRuleCategoryDef.CreateRuleCategory("VARIANTPACKS", "VariantPacks", VariantPackCategoryHiddenTest);
            LogMessage("Added VariantPack Category");

            yield return null;

            for (int i = 0; i < VariantPackManager.registeredVariantPacks.Length; i++)
            {
                ReadOnlyVariantPack variantPack = VariantPackManager.registeredVariantPacks[i];

                yield return null;

                RuleDef packRule = RuleDefCreation.CreateRuleDefFromVariantPack(variantPackCategory, variantPack, variantPack.isHidden);
                RuleCatalogExtras.AddRuleToCatalog(packRule, variantPackCategory.ruleCategoryIndex);
                LogMessage($"Added RuleDef for {variantPack.identifier} (isHidden={variantPack.isHidden})");
            }
        }

        private static IEnumerator AddPerBodyVariantCategoryAndRules()
        {
            yield return null;

            bodyVariantsCategory = VAPIRuleCategoryDef.CreateRuleCategory("VARIANTS", "Variants", BodyVariantsCategoryHiddenTest);

            LogMessage($"Added Character Variant Group Category");
            //We will iterate thru the body components
            foreach (var bodyComponent in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                yield return null;

                //If the body has no variants, no need to make a rule for it.
                CharacterVariantProvider variantProviderForBody = CharacterVariantCatalog.GetCharacterVariantProvider(bodyComponent.bodyIndex);
                if (variantProviderForBody == null)
                    continue;

                //Since this body has variants, add it to the bodyVariantsCategory
                //We will mark it as not hidden if it has at least one non hidden variant.
                bool isHidden = HasAllVariantsHidden(variantProviderForBody);
                RuleDef bodyVariantsEnabledRuleDef = RuleDefCreation.CreateRuleDefForBody(bodyVariantsCategory, bodyComponent, isHidden);
                RuleCatalogExtras.AddRuleToCatalog(bodyVariantsEnabledRuleDef, bodyVariantsCategory.ruleCategoryIndex);
                LogMessage($"Added Rule for {bodyComponent.name} Variant Groups (isHidden={isHidden})");
                bodyIndexToRule.Add(bodyComponent.bodyIndex, bodyVariantsEnabledRuleDef);

                //We dont want the categories to be hidden, doing so makes it impossible to submit rules as is.
                var categoryForBodyComponentVariants = VAPIRuleCategoryDef.CreateRuleCategory("BODY_VARIANTS", $"BodyVariants.{bodyComponent.name}", RuleCatalog.HiddenTestFalse);
                categoryForBodyComponentVariants.displayToken = bodyComponent.baseNameToken;
                categoryForBodyComponentVariants.associatedBodyIndex = bodyComponent.bodyIndex;
                LogMessage($"Created Per Body Variant Category for {bodyComponent.name}");

                bodyToVariantCategories.Add(bodyComponent.bodyIndex, categoryForBodyComponentVariants);

                //Iterate thru each variant from the body
                foreach (var variant in variantProviderForBody.allVariants)
                {
                    yield return null;

                    //We want to include the Body's "global toggle for variants" as one of the required choices that need to be on
                    RuleDef variantRule = RuleDefCreation.CreateRuleDefForVariant(categoryForBodyComponentVariants, variant, variant.isHidden, (VAPIRuleChoiceDef)bodyVariantsEnabledRuleDef.FindChoice("On"));
                    RuleCatalogExtras.AddRuleToCatalog(variantRule, categoryForBodyComponentVariants.ruleCategoryIndex);
                    variantToRule.Add(variant.characterVariantIndex, variantRule);
                    LogMessage($"Added Rule for {variant.cachedName} (isHidden={isHidden})");
                }
            }
        }

        private static bool HasAllVariantsHidden(CharacterVariantProvider provider)
        {
            foreach (var variant in provider.allVariants)
            {
                if (variant.isHidden == false)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogMessage(object data)
        {
#if DEBUG
            VAPILog.Message(data);
#endif
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogError(object data)
        {
#if DEBUG
            VAPILog.Error(data);
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