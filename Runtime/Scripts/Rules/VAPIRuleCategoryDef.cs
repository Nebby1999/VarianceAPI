#nullable enable
using R2API;
using RoR2;
using System;

namespace VAPI.Rules
{
    internal sealed class VAPIRuleCategoryDef : RuleCategoryDef
    {
        public int ruleCategoryIndex;
        public string baseNameForRules = "";

        public BodyIndex associatedBodyIndex = BodyIndex.None;

        public static VAPIRuleCategoryDef CreateRuleCategory(string token, string baseNameForRules, Func<bool> hiddenTest)
        {
            string baseToken = $"VAPI_RULE_HEADER_{token}";

            var categoryDef = new VAPIRuleCategoryDef
            {
                displayToken = baseToken,
                subtitleToken = baseToken + "_SUBTITLE",
                editToken = baseToken + "_EDIT",
                emptyTipToken = baseToken + "_EMPTY",
                ruleCategoryType = RuleCatalog.RuleCategoryType.VoteResultGrid,
                color = VAPIConfig._variantHealthBarColor,
                hiddenTest = hiddenTest,
                baseNameForRules = $"VAPI.{baseNameForRules}."
            };
            categoryDef.ruleCategoryIndex = RuleCatalogExtras.AddCategory(categoryDef);
            return categoryDef;
        }
    }
}