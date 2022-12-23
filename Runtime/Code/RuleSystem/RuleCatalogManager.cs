using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI.RuleSystem
{
    public static class RuleCatalogManager
    {
        private static RuleCategoryDef variantPackCategory;
        private static RuleCategoryDef variantCategory;
        [SystemInitializer]
        private static void SystemInitializer()
        {
            On.RoR2.RuleCatalog.Init += AddCustomRules;
        }

        private static void AddCustomRules(On.RoR2.RuleCatalog.orig_Init orig)
        {
            orig();
            variantPackCategory = RuleCatalog.AddCategory("VAPI_RULE_HEADER_VARIANTPACKS", "VAPI_RULE_HEADER_VARIANTS_SUBTITLE", Color.cyan, null, "VAPI_RULE_HEADER_VARIANTS_EDIT", VAPIConfig.HiddenTestVariantRules, RuleCatalog.RuleCategoryType.VoteResultGrid);

            variantCategory = RuleCatalog.AddCategory("VAPI_RULE_HEADER_VARIANTS", "VAPI_RULE_HEADER_VARIANTS_SUBTITLE", Color.cyan, null, "VAPI_RULE_HEADER_VARIANTS_EDIT", RuleCatalog.HiddenTestTrue, RuleCatalog.RuleCategoryType.VoteResultGrid);
        }

        private static RuleDef CreateRuleDefFromVariant(VariantDef variantDef)
        {
            return null;
        }
    }
}