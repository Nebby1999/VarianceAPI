using RoR2;
using System;
using System.Collections;
using UnityEngine;

namespace VAPI.Rules
{
    internal static class RuleCatalogAdditionsUI
    {
        private static Sprite _texUIHeaderSharp;

        internal static IEnumerator Init()
        {
            using var spriteRequest = VAPIAssets.LoadAssetAsync<Sprite>("texUIHeaderSharp");

            while(!spriteRequest.isComplete)
            {
                yield return null;
            }

            _texUIHeaderSharp = spriteRequest.asset;

            On.RoR2.UI.RuleChoiceController.SetChoice += RuleChoiceController_SetChoice;
            On.RoR2.UI.RuleChoiceController.UpdateChoiceDisplay += RuleChoiceController_UpdateChoiceDisplay;
            On.RoR2.UI.RuleCategoryController.SetData += RuleCategoryController_SetData;
        }

        private static void RuleCategoryController_SetData(On.RoR2.UI.RuleCategoryController.orig_SetData orig, RoR2.UI.RuleCategoryController self, RuleCategoryDef categoryDef, RuleChoiceMask availability, RuleBook ruleBook)
        {
            orig(self, categoryDef, availability, ruleBook);
        }

        private static void RuleChoiceController_UpdateChoiceDisplay(On.RoR2.UI.RuleChoiceController.orig_UpdateChoiceDisplay orig, RoR2.UI.RuleChoiceController self, RuleChoiceDef displayChoiceDef)
        {
            orig(self, displayChoiceDef);
        }

        private static void RuleChoiceController_SetChoice(On.RoR2.UI.RuleChoiceController.orig_SetChoice orig, RoR2.UI.RuleChoiceController self, RuleChoiceDef newChoiceDef)
        {
            orig(self, newChoiceDef);
        }
    }
}