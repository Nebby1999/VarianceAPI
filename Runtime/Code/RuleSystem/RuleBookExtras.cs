using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI.RuleSystem
{
    public static class RuleBookExtras
    {
        private static RuleCategoryDef variantPackCategory;
        private static int variantPackCategoryIndex;
        private static RuleCategoryDef variantCategory;
        private static int variantCategoryIndex;
        [SystemInitializer(typeof(RuleCatalog), typeof(VariantCatalog))]
        private static void SystemInitializer()
        {
            AddNewCategories();
            AddVariantPackRules();
            AddVariantRules();
        }

        private static void AddNewCategories()
        {
            variantPackCategory = new RuleCategoryDef
            {
                displayToken = "VAPI_RULE_HEADER_VARIANTPACKS",
                subtitleToken = "VAPI_RULE_HEADER_VARIANTPACKS_SUBTITLE",
                editToken = "VAPI_RULE_HEADER_VARIANTPACKS_EDIT",
                emptyTipToken = "VAPI_RULE_HEADER_VARIANTPACKS_EMPTY",
                ruleCategoryType = RuleCatalog.RuleCategoryType.VoteResultGrid,
                color = Color.cyan,
                hiddenTest = RuleCatalog.HiddenTestFalse
            };
            variantPackCategoryIndex = AddCategory(variantPackCategory);

            variantCategory = new RuleCategoryDef
            {
                displayToken = "VAPI_RULE_HEADER_VARIANTS",
                subtitleToken = "VAPI_RULE_HEADER_VARIANTS_SUBTITLE",
                editToken = "VAPI_RULE_HEADER_VARIANTS_EDIT",
                emptyTipToken = "VAPI_RULE_HEADER_VARIANTS_EMPTY",
                ruleCategoryType = RuleCatalog.RuleCategoryType.VoteResultGrid,
                color = Color.cyan,
                hiddenTest = VAPIConfig.HiddenTestVariantRules,
            };
            variantCategoryIndex = AddCategory(variantCategory);
        }

        private static void AddVariantPackRules()
        {
            for(int i = 0; i < VariantPackCatalog.registeredPacks.Length; i++)
            {
                VariantPackDef pack = VariantPackCatalog.registeredPacks[i];
                if (pack.variants.Length <= 0)
                    continue;

                RuleDef packRule = CreateRuleDefFromVariantPack(pack);
                AddRuleToCatalog(packRule, variantPackCategoryIndex);
            }
        }

        private static void AddVariantRules()
        {
            for(int i = 0; i < VariantCatalog.registeredVariants.Length; i++)
            {
                VariantDef def = VariantCatalog.registeredVariants[i];

                RuleDef variantRule = CreateRuleDefFromVariant(def);
                AddRuleToCatalog(variantRule, variantCategoryIndex);
            }
        }

        private static RuleDef CreateRuleDefFromVariantPack(VariantPackDef variantPack)
        {
            RuleDef rule = new RuleDef($"VariantPacks.{variantPack.name}", variantPack.nameToken);
            rule.displayToken = $"VAPI_VARIANTPACKRULE_{variantPack.nameToken}_DISPLAY";

            VAPIRuleChoiceDef onChoice = rule.AddVAPIChoice("On");
            onChoice.sprite = variantPack.packEnabledIcon;
            onChoice.tooltipNameToken = variantPack.nameToken;
            onChoice.tooltipNameColor = Color.cyan;
            onChoice.tooltipBodyToken = variantPack.descriptionToken;
            rule.MakeNewestChoiceDefault();

            VAPIRuleChoiceDef offChoice = rule.AddVAPIChoice("Off");
            offChoice.sprite = variantPack.packDisabledIcon;
            offChoice.tooltipNameToken = variantPack.nameToken;
            offChoice.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
            offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
            offChoice.tooltipBodyToken = variantPack.descriptionToken;

            return rule;
        }
        private static RuleDef CreateRuleDefFromVariant(VariantDef variantDef)
        {
            CharacterBody body = BodyCatalog.GetBodyPrefabBodyComponent(BodyCatalog.FindBodyIndex(variantDef.bodyName));
            string variantName = GetNameFromOverrides(variantDef);
            RuleDef rule = new RuleDef($"Variants.{variantDef.name}", variantName);
            rule.displayToken = $"VAPI_VARIANTRULE_{variantDef.name.ToUpperInvariant()}_DISPLAY";

            VAPIRuleChoiceDef onChoice = rule.AddVAPIChoice("On");
            onChoice.sprite = Sprite.Create((Texture2D)body.portraitIcon, new Rect(0f, 0f, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f), 100);
            onChoice.tooltipNameToken = variantName;
            onChoice.tooltipNameColor = Color.cyan;
            onChoice.tooltipBodyToken = "VAPI_RULE_VARIANT_ON_DESCRIPTION";
            onChoice.requiredVariantPack = VariantPackCatalog.FindVariantPackDef(variantDef);
            onChoice.variantIndex = variantDef.VariantIndex;
            onChoice.onlyShowInGameBrowserIfNonDefault = true;
            rule.MakeNewestChoiceDefault();

            RuleChoiceDef offChoice = rule.AddVAPIChoice("Off");
            offChoice.spritePath = "Textures/MiscIcons/texUnlockIcon";
            offChoice.tooltipNameToken = variantName;
            offChoice.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
            offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
            offChoice.tooltipBodyToken = "VAPI_RULE_VARIANT_OFF_DESCRIPTION";
            offChoice.onlyShowInGameBrowserIfNonDefault = true;
            return rule;
        }
        private static int AddCategory(RuleCategoryDef category)
        {
            RuleCatalog.allCategoryDefs.Add(category);
            return RuleCatalog.allCategoryDefs.Count - 1;
        }


        //Vanilla Categories:
        //0 - Difficultym, 1 - Expansions, 2 - Artifacts, 3 - Items, 4 - Equipments, 5 - Misc
        private static void AddRuleToCatalog(RuleDef ruleDef, int ruleCategoryDefIndex)
        {
            ruleDef.category = RuleCatalog.GetCategoryDef(ruleCategoryDefIndex);
            ruleDef.globalIndex = RuleCatalog.allRuleDefs.Count;
            RuleCatalog.allCategoryDefs[ruleCategoryDefIndex].children.Add(ruleDef);
            RuleCatalog.allRuleDefs.Add(ruleDef);

            if(RuleCatalog.highestLocalChoiceCount < ruleDef.choices.Count)
            {
                RuleCatalog.highestLocalChoiceCount = ruleDef.choices.Count;
            }

            RuleCatalog.ruleDefsByGlobalName[ruleDef.globalName] = ruleDef;
            for(int i = 0; i < ruleDef.choices.Count; i++)
            {
                RuleChoiceDef choiceDef = ruleDef.choices[i];
                choiceDef.localIndex = i;
                choiceDef.globalIndex = RuleCatalog.allChoicesDefs.Count;
                RuleCatalog.allChoicesDefs.Add(choiceDef);

                RuleCatalog.ruleChoiceDefsByGlobalName[choiceDef.globalName] = choiceDef;

                if(choiceDef.requiredUnlockable)
                {
                    HG.ArrayUtils.ArrayAppend(ref RuleCatalog._allChoiceDefsWithUnlocks, choiceDef);
                }
            }
        }

        private static VAPIRuleChoiceDef AddVAPIChoice(this RuleDef ruleDef, string choiceName, object extraData = null, bool excludeByDefault = false)
        {
            VAPIRuleChoiceDef ruleChoice = new VAPIRuleChoiceDef();
            ruleChoice.ruleDef = ruleDef;
            ruleChoice.localName = choiceName;
            ruleChoice.globalName = ruleDef.globalName + "." + choiceName;
            ruleChoice.extraData = extraData;
            ruleChoice.excludeByDefault = excludeByDefault;
            ruleDef.choices.Add(ruleChoice);
            return ruleChoice;
        }

        private static string GetNameFromOverrides(VariantDef def)
        {
            CharacterBody body = BodyCatalog.GetBodyPrefabBodyComponent(BodyCatalog.FindBodyIndexCaseInsensitive(def.bodyName));
            string name = string.Empty;
            foreach (VariantDef.VariantOverrideName overrides in def.nameOverrides)
            {
                switch (overrides.overrideType)
                {
                    case OverrideNameType.Prefix:
                        name = Language.GetStringFormatted(overrides.token) + " " + body.GetDisplayName();
                        break;
                    case OverrideNameType.Suffix:
                        name = body.GetDisplayName() + " " + Language.GetStringFormatted(overrides.token);
                        break;
                    case OverrideNameType.Complete:
                        name = Language.GetStringFormatted(overrides.token);
                        break;
                }
            }
            return name;
        }

        public static bool TryCastVAPIRuleChoiceDef(RuleChoiceDef choice, out VAPIRuleChoiceDef vapiRuleChoiceDef)
        {
            if(choice is VAPIRuleChoiceDef vrcd)
            {
                vapiRuleChoiceDef = vrcd;
                return true;
            }
            vapiRuleChoiceDef = null;
            return false;
        }
    }
}