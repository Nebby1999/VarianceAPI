using Moonstorm.AddressableAssets;
using R2API;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.ExpansionManagement;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Moonstorm;

namespace VAPI.RuleSystem
{
    internal static class RuleBookExtras
    {
        private static RuleChoiceDef varianceExpansionRuleChoice;
        private static RuleCategoryDef variantPackCategory;
        private static int variantPackCategoryIndex;
        private static RuleCategoryDef variantCategory;
        private static int variantCategoryIndex;
        private static Dictionary<VariantIndex, VAPIRuleChoiceDef> variantIndexToRuleChoice = new Dictionary<VariantIndex, VAPIRuleChoiceDef>();
        internal static RuleDef varianceArtifactRuleDef;

        [SystemInitializer(typeof(RuleCatalog), typeof(VariantCatalog))]
        private static void SystemInitializer()
        {
            varianceExpansionRuleChoice = VAPIAssets.LoadAsset<ExpansionDef>("VarianceExpansion").enabledChoice;
            AddNewCategories();
            AddVariantPackRules();
            AddVariantRules();
            varianceArtifactRuleDef = RuleCatalog.FindRuleDef("Artifacts.Variance");

            AddressReferencedAsset.OnAddressReferencedAssetsLoaded += FinishRuleChoices;
        }

        private static void FinishRuleChoices()
        {
            foreach(var (variantIndex, ruleChoice) in variantIndexToRuleChoice)
            {
                VariantDef def = VariantCatalog.GetVariantDef(variantIndex);

                ruleChoice.requiredUnlockables = GetRequiredUnlockableDefs(def);
                ruleChoice.requiredExpansionDefs = GetRequiredExpansionDefs(def);
            }
        }

        internal static bool TryCastVAPIRuleChoiceDef(RuleChoiceDef choice, out VAPIRuleChoiceDef vapiRuleChoiceDef)
        {
            if (choice is VAPIRuleChoiceDef vrcd)
            {
                vapiRuleChoiceDef = vrcd;
                return true;
            }
            vapiRuleChoiceDef = null;
            return false;
        }

        internal static bool CanVariantSpawn(RuleBook runRulebook, VariantIndex variantToCheck)
        {
            if (variantToCheck == VariantIndex.None)
                return false;

            var variantChoice = variantIndexToRuleChoice[variantToCheck];
            var packEnabledChoice = variantChoice.tiedPackEnabledChoice;
            return runRulebook.IsChoiceActive(packEnabledChoice) && runRulebook.IsChoiceActive(variantChoice);
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
                hiddenTest = VariantPackCategoryHiddenTest
            };
            variantPackCategoryIndex = RuleCatalogExtras.AddCategory(variantPackCategory);

            variantCategory = new RuleCategoryDef
            {
                displayToken = "VAPI_RULE_HEADER_VARIANTS",
                subtitleToken = "VAPI_RULE_HEADER_VARIANTS_SUBTITLE",
                editToken = "VAPI_RULE_HEADER_VARIANTS_EDIT",
                emptyTipToken = "VAPI_RULE_HEADER_VARIANTS_EMPTY",
                ruleCategoryType = RuleCatalog.RuleCategoryType.VoteResultGrid,
                color = Color.cyan,
                hiddenTest = VariantCategoryHiddenTest,
            };
            variantCategoryIndex = RuleCatalogExtras.AddCategory(variantCategory);
        }

        private static void AddVariantPackRules()
        {
            for (int i = 0; i < VariantPackCatalog.registeredPacks.Length; i++)
            {
                VariantPackDef pack = VariantPackCatalog.registeredPacks[i];
                //Adding a rule for packs that dont have variants is pointless
                if (pack.variants.Length <= 0)
                    continue;

                RuleDef packRule = CreateRuleDefFromVariantPack(pack);
                RuleCatalogExtras.AddRuleToCatalog(packRule, variantPackCategoryIndex);
            }
        }

        private static RuleDef CreateRuleDefFromVariantPack(VariantPackDef variantPack)
        {
            RuleDef rule = new RuleDef($"VariantPacks.{variantPack.name}", variantPack.nameToken);

            VAPIRuleChoiceDef onChoice = AddVAPIChoice(rule, "On");
            onChoice.sprite = variantPack.packEnabledIcon;
            onChoice.tooltipNameToken = variantPack.nameToken;
            onChoice.tooltipNameColor = Color.cyan;
            onChoice.tooltipBodyToken = variantPack.descriptionToken;
            onChoice.variantPackIndex = variantPack.VariantPackIndex;
            rule.MakeNewestChoiceDefault();
            variantPack.EnabledChoice = onChoice;

            VAPIRuleChoiceDef offChoice = AddVAPIChoice(rule, "Off");
            offChoice.spritePath = "Textures/MiscIcons/texUnlockIcon";
            offChoice.tooltipNameToken = variantPack.nameToken;
            offChoice.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
            offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
            offChoice.tooltipBodyToken = variantPack.descriptionToken;

            return rule;
        }
        private static void AddVariantRules()
        {
            VAPILog.Info("Adding Variant Rules");
#if DEBUG
            VAPILog.Debug($"Registered Variants: {VariantCatalog.VariantCount}");
#endif
            for (int i = 0; i < VariantCatalog.registeredVariants.Length; i++)
            {
                VariantDef def = VariantCatalog.registeredVariants[i];

                RuleDef variantRule = CreateRuleDefFromVariant(def);
                RuleCatalogExtras.AddRuleToCatalog(variantRule, variantCategoryIndex);
            }
        }

        private static RuleDef CreateRuleDefFromVariant(VariantDef variantDef)
        {
            CharacterBody body = BodyCatalog.GetBodyPrefabBodyComponent(BodyCatalog.FindBodyIndex(variantDef.bodyName));
            string variantName = GetNameFromOverrides(variantDef);
            RuleDef rule = new RuleDef($"Variants.{variantDef.name}", variantName);
            rule.displayToken = $"VAPI_VARIANTRULE_{variantDef.name.ToUpperInvariant()}_DISPLAY";

            VAPIRuleChoiceDef onChoice = AddVAPIChoice(rule, "On");
            onChoice.sprite = Sprite.Create((Texture2D)body.portraitIcon, new Rect(0f, 0f, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f), 100);
            onChoice.tooltipNameToken = variantName;
            onChoice.tooltipNameColor = Color.cyan;
            onChoice.tooltipBodyToken = "VAPI_RULE_VARIANT_ON_DESCRIPTION";
            onChoice.variantIndex = variantDef.VariantIndex;

            onChoice.tiedPackEnabledChoice = GetVariantPackEnabledChoice(variantDef);
            onChoice.requiredChoiceDefs = GetRequiredChoiceDefs(variantDef);
            /*onChoice.requiredExpansionDefs = GetRequiredExpansionDefs(variantDef);
            onChoice.requiredUnlockables = GetRequiredUnlockableDefs(variantDef);*/
            rule.MakeNewestChoiceDefault();

            variantIndexToRuleChoice.Add(variantDef.VariantIndex, onChoice);


            VAPIRuleChoiceDef offChoice = AddVAPIChoice(rule, "Off");
            offChoice.spritePath = "Textures/MiscIcons/texUnlockIcon";
            offChoice.tooltipNameToken = variantName;
            offChoice.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
            offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
            offChoice.tooltipBodyToken = "VAPI_RULE_VARIANT_OFF_DESCRIPTION";
            offChoice.requiredChoiceDefs = GetRequiredChoiceDefs(variantDef);

            var display = variantDef.spawnRate > 0;
            rule.forceLobbyDisplay = display;
            onChoice.excludeByDefault = !display;
            offChoice.excludeByDefault = !display;
            variantDef.spawnRateConfig.OnConfigChanged += f =>
            {
                bool b = f > 0;
                rule.forceLobbyDisplay = b;
                onChoice.excludeByDefault = !b;
                offChoice.excludeByDefault = !b;
            };

            return rule;
        }

        private static VAPIRuleChoiceDef AddVAPIChoice(RuleDef ruleDef, string choiceName, object extraData = null, bool excludeByDefault = false)
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

        private static List<RuleChoiceDef> GetRequiredChoiceDefs(VariantDef variantDef)
        {
            return new List<RuleChoiceDef> { VariantPackCatalog.FindVariantPackDef(variantDef).EnabledChoice };
        }

        private static RuleChoiceDef GetVariantPackEnabledChoice(VariantDef variantDef)
        {
            return VariantPackCatalog.FindVariantPackDef(variantDef).EnabledChoice;
        }

        private static List<ExpansionDef> GetRequiredExpansionDefs(VariantDef variantDef)
        {
            List<ExpansionDef> expansions = new List<ExpansionDef>();
            var spawnCondition = variantDef.variantSpawnCondition;
            if (!spawnCondition)
                return expansions;

            expansions.AddRange(spawnCondition.requiredExpansionDefs.Where(x => x.AssetExists).Select(x => x.Asset));
            return expansions;
        }

        private static List<UnlockableDef> GetRequiredUnlockableDefs(VariantDef variantDef)
        {
            List<UnlockableDef> unlockables = new List<UnlockableDef>();
            var spawnCondition = variantDef.variantSpawnCondition;
            if (spawnCondition && spawnCondition.requiredUnlock.AssetExists)
                unlockables.Add(spawnCondition.requiredUnlock);

            return unlockables;
        }

        //Sometimes being verbose is helpful ngl
        private static bool VariantPackCategoryHiddenTest()
        {
            bool preGameControllerExists = PreGameController.instance;
            if (!preGameControllerExists)
                return true;

            bool isVAPIExpansionActive = PreGameController.instance.readOnlyRuleBook.IsChoiceActive(varianceExpansionRuleChoice);

            if (isVAPIExpansionActive)
                return false;

            return true;
        }

        private static bool VariantCategoryHiddenTest()
        {
            bool preGameControllerExists = PreGameController.instance;
            if (!preGameControllerExists)
                return true;

#if !DEBUG
            if (!VAPIConfig.showVariantRuleCategory)
            {
                return true;
            }
#endif

            bool anyPackActive = VariantPackCatalog.registeredPacks.Where(x => x.EnabledChoice != null).Any(x => PreGameController.instance.readOnlyRuleBook.IsChoiceActive(x.EnabledChoice));
            if (!anyPackActive)
            {
                return true;
            }

            bool isVAPIExpansionActive = PreGameController.instance.readOnlyRuleBook.IsChoiceActive(varianceExpansionRuleChoice);
            if (isVAPIExpansionActive)
                return false;

            return true;
        }
    }
}