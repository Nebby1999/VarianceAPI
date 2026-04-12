using RoR2.ExpansionManagement;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    internal static class VAPIRuleBook
    {
        private struct CategoryWithIndex
        {
            public RuleCategoryDef category;
            public int categoryIndex;
        }
        private static RuleChoiceDef _varianceExpansionRuleChoice;
        private static CategoryWithIndex _variantPackCategory;
        private static CategoryWithIndex _variantCategory;
        internal static RuleDef _varianceArtifactRuleDef;

        private static Dictionary<CharacterVariantIndex, RuleDef> _variantIndexToRuleDef = new Dictionary<CharacterVariantIndex, RuleDef>();

        [SystemInitializer(typeof(RuleCatalog), typeof(CharacterVariantCatalog), typeof(CharacterVariantTierCatalog))]
        private static void Init()
        {
            _varianceExpansionRuleChoice = VAPIContent.VAPIExpansion.asset.enabledChoice;

            AddVariantPackCategory();
            AddVariantPackRules();
            
            AddCharacterVariantCategory();
            AddVariantRules();

            _varianceArtifactRuleDef = RuleCatalog.FindRuleDef("Artifacts.Variance");

            RoR2.Language.onCurrentLanguageChanged += RecomputeTokenValues;
        }

        private static void RecomputeTokenValues()
        {
            foreach(var (variantIndex, ruleDef) in _variantIndexToRuleDef)
            {
                CharacterVariantDef variantDef = CharacterVariantCatalog.GetCharacterVariantDef(variantIndex);
                string variantName = GetInGameVariantName(variantDef);

                ruleDef.displayToken = variantName;
                ruleDef.FindChoice("On").tooltipNameToken = variantName;
                ruleDef.FindChoice("Off").tooltipNameToken = variantName;
            }
        }

        private static void AddVariantPackCategory()
        {
            //Hide category if VAPI is not enabled.
            bool VariantPackCategoryHiddenTest()
            {
                bool preGameControllerExists = PreGameController.instance;
                if (preGameControllerExists == false)
                    return true;

                bool isVAPIExpansionActive = PreGameController.instance.readOnlyRuleBook.IsChoiceActive(_varianceExpansionRuleChoice);

                if (!isVAPIExpansionActive)
                    return false;

                return true;
            }

            var categoryDef = new RuleCategoryDef
            {
                displayToken = "VAPI_RULE_HEADER_VARIANTPACKS",
                subtitleToken = "VAPI_RULE_HEADER_VARIANTPACKS_SUBTITLE",
                editToken = "VAPI_RULE_HEADER_VARIANTPACKS_EDIT",
                emptyTipToken = "VAPI_RULE_HEADER_VARIANTPACKS_EMPTY",
                ruleCategoryType = RuleCatalog.RuleCategoryType.VoteResultGrid,
                color = Color.cyan,
                hiddenTest = VariantPackCategoryHiddenTest
            };
            _variantPackCategory = new CategoryWithIndex
            {
                category = categoryDef,
                categoryIndex = RuleCatalogExtras.AddCategory(categoryDef)
            };
        }

        private static void AddVariantPackRules()
        {
            RuleDef CreateRuleDefFromVariantPack(VariantPack variantPack)
            {
                RuleDef rule = new RuleDef($"VariantPacks.{variantPack.identifier}", variantPack.nameToken);

                VAPIRuleChoiceDef onchoice = VAPIRuleChoiceDef.CreateAndAddToRule(rule, "On");
                onchoice.sprite = variantPack.packIcon;
                onchoice.tooltipNameToken = variantPack.nameToken;
                onchoice.tooltipBodyToken = variantPack.descriptionToken;
                onchoice.tooltipNameColor = Color.cyan;
                onchoice.variantPackIndex = variantPack.variantPackIndex;
                variantPack.packEnabledChoice = onchoice;
                rule.MakeNewestChoiceDefault();

                VAPIRuleChoiceDef offChoice = VAPIRuleChoiceDef.CreateAndAddToRule(rule, "Off");
                offChoice.spritePath = "Textures/MiscIcons/texUnlockIcon";
                offChoice.tooltipNameToken = variantPack.nameToken;
                offChoice.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
                offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
                offChoice.tooltipBodyToken = variantPack.descriptionToken;

                return rule;
            }

            for(int i = 0; i < VariantPackManager.registeredVariantPacks.Length; i++)
            {
                ReadOnlyVariantPack variantPack = VariantPackManager.registeredVariantPacks[i];

                //Do not add rule to the hidden packs, which is the main API pack
                if (variantPack.isHidden)
                    continue;

                RuleDef packRule = CreateRuleDefFromVariantPack(variantPack.src);
                RuleCatalogExtras.AddRuleToCatalog(packRule, _variantPackCategory.categoryIndex);
            }
        }

        private static void AddCharacterVariantCategory()
        {
            bool VariantCategoryHiddenTest()
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
                foreach(var variantPack in VariantPackManager.registeredVariantPacks)
                {
                    if (variantPack.isHidden)
                        continue;

                    anyPackActive |= PreGameController.instance.readOnlyRuleBook.IsChoiceActive(variantPack.src.packEnabledChoice);
                }

                if (!anyPackActive)
                    return true;

                bool isVAPIExpansionActive = PreGameController.instance.readOnlyRuleBook.IsChoiceActive(_varianceExpansionRuleChoice);
                if (isVAPIExpansionActive)
                    return false;

                return true;
            }

            var categoryDef = new RuleCategoryDef
            {
                displayToken = "VAPI_RULE_HEADER_VARIANTS",
                subtitleToken = "VAPI_RULE_HEADER_VARIANTS_SUBTITLE",
                editToken = "VAPI_RULE_HEADER_VARIANTS_EDIT",
                emptyTipToken = "VAPI_RULE_HEADER_VARIANTS_EMPTY",
                ruleCategoryType = RuleCatalog.RuleCategoryType.VoteResultGrid,
                color = Color.cyan,
                hiddenTest = VariantCategoryHiddenTest,
            };

            _variantCategory = new CategoryWithIndex
            {
                category = categoryDef,
                categoryIndex = RuleCatalogExtras.AddCategory(categoryDef)
            };
        }

        private static void AddVariantRules()
        {
            RuleDef CreateRuleDefFromVariant(CharacterVariantDef characterVariantDef)
            {
                Sprite icon = GetTargetCharacterSprite(characterVariantDef.targetCharacter);
                string inGameVariantName = GetInGameVariantName(characterVariantDef);

                RuleDef rule = new RuleDef($"Variants.{characterVariantDef.cachedName}", inGameVariantName);
                rule.displayToken = inGameVariantName;

                VAPIRuleChoiceDef onChoice = VAPIRuleChoiceDef.CreateAndAddToRule(rule, "On");
                onChoice.sprite = icon;
                onChoice.tooltipNameToken = inGameVariantName;
                onChoice.tooltipBodyToken = "VAPI_RULE_VARIANT_ON_DESCRIPTION";
                onChoice.variantIndex = characterVariantDef.characterVariantIndex;
                onChoice.tooltipNameColor = Color.cyan;
                onChoice.tiedPackEnabledChoice = VariantPackManager.GetVariantPack(VariantPackManager.FindVariantPackIndex(characterVariantDef.characterVariantIndex)).packEnabledChoice;
                onChoice.requiredChoiceDefs = new List<RuleChoiceDef>();
                SetRequiredAssets(onChoice, characterVariantDef);

                rule.MakeNewestChoiceDefault();

                VAPIRuleChoiceDef offChoice = VAPIRuleChoiceDef.CreateAndAddToRule(rule, "Off");
                offChoice.spritePath = "Textures/MiscIcons/texUnlockIcon";
                offChoice.tooltipNameToken = inGameVariantName;
                offChoice.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
                offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
                offChoice.tooltipBodyToken = "VAPI_RULE_VARIANT_OFF_DESCRIPTION";
                offChoice.tiedPackEnabledChoice = VariantPackManager.GetVariantPack(VariantPackManager.FindVariantPackIndex(characterVariantDef.characterVariantIndex)).packEnabledChoice;
                offChoice.requiredChoiceDefs = new List<RuleChoiceDef>();
                SetRequiredAssets(offChoice, characterVariantDef);


                _variantIndexToRuleDef.Add(characterVariantDef.characterVariantIndex, rule);
                OnVariantDefSpawnRateChanged(characterVariantDef);
                return rule;
            }

            for (int i = 0; i < CharacterVariantCatalog.registeredCharacterVariantDefs.Length; i++)
            {
                CharacterVariantDef def = CharacterVariantCatalog.registeredCharacterVariantDefs[i];

                if (!def.hasProvider)
                    continue;

                RuleDef variantRule = CreateRuleDefFromVariant(def);
                RuleCatalogExtras.AddRuleToCatalog(variantRule, _variantCategory.categoryIndex);
            }
        }

        public static void OnVariantDefSpawnRateChanged(CharacterVariantDef variantDef)
        {
            if(!_variantIndexToRuleDef.TryGetValue(variantDef.characterVariantIndex, out RuleDef ruleDef))
            {
                return;
            }

            var onChoice = ruleDef.FindChoice("On");
            var offChoice = ruleDef.FindChoice("Off");

            var shouldDisplay = variantDef.spawnRate > 0;
            onChoice.excludeByDefault = !shouldDisplay;
            offChoice.excludeByDefault = !shouldDisplay;
        }

        public static bool IsVariantRuleEnabled(CharacterVariantDef variant, RuleBook runRuleBook)
        {
            if(!_variantIndexToRuleDef.TryGetValue(variant.characterVariantIndex, out RuleDef ruleDef))
            {
                return false;
            }

            var onChoice = (VAPIRuleChoiceDef)ruleDef.FindChoice("On");
            return runRuleBook.IsChoiceActive(onChoice) && runRuleBook.IsChoiceActive(onChoice.tiedPackEnabledChoice);
        }

        private static Sprite GetTargetCharacterSprite(VariantCharacterTarget target)
        {
            Sprite CreateFromIcon(Texture2D icon) => Sprite.Create(icon, new Rect(0f, 0f, icon.width, icon.height), new Vector2(0.5f, 0.5f), 100);
            Component c = target.LoadCharacterComponent();
            if(c is CharacterBody body && body.portraitIcon)
            {
                return CreateFromIcon((Texture2D)body.portraitIcon);
            }
            if(c is CharacterMaster master)
            {
                GameObject bodyPrefab = master.bodyPrefab;
                if(bodyPrefab.TryGetComponent<CharacterBody>(out var masterBody) && masterBody.portraitIcon)
                {
                    return CreateFromIcon((Texture2D)masterBody.portraitIcon);
                }
            }

            return UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>("2205ae9eb97f98e42ac56ee4a9e32474[texMysteryIcon]").WaitForCompletion();
        }

        private static string GetInGameVariantName(CharacterVariantDef characterVariantDef)
        {
            Component c = characterVariantDef.targetCharacter.LoadCharacterComponent();
            string baseNameToken = "???";
            if(c is CharacterBody body)
            {
                baseNameToken = body.baseNameToken;
            }
            else if(c is CharacterMaster master && master.bodyPrefab && master.bodyPrefab.TryGetComponent<CharacterBody>(out var masterBody))
            {
                baseNameToken = masterBody.baseNameToken;
            }

            if(characterVariantDef.variantNameProvider == null)
            {
                return Language.GetString(baseNameToken);
            }
            else
            {
                return characterVariantDef.variantNameProvider!.GetVariantName(Language.GetString(baseNameToken));
            }
        }

        private static void SetRequiredAssets(VAPIRuleChoiceDef choiceDef, CharacterVariantDef variantDef)
        {
            List<UnlockableDef> GetRequiredUnlockableDefs()
            {
                List<UnlockableDef> unlockables = new List<UnlockableDef>();
                var spawnCondition = variantDef.spawnCondition;
                if (spawnCondition == null)
                    return unlockables;

                UnlockableDef requiredUnlock = spawnCondition.requiredUnlock;
                if (requiredUnlock)
                    unlockables.Add(requiredUnlock);
                return unlockables;
            }

            List<ExpansionDef> GetRequiredExpansionDefs()
            {
                List<ExpansionDef> expansions = new List<ExpansionDef>();

                //Add the expansionDef of the body, if it exists
                var component = variantDef.targetCharacter.LoadCharacterComponent();
                if(component is CharacterBody b && b.TryGetComponent<ExpansionRequirementComponent>(out var expansionRequirementComponent))
                {
                    expansions.Add(expansionRequirementComponent.requiredExpansion);
                }
                else if(component is CharacterMaster m && m.bodyPrefab && m.bodyPrefab.TryGetComponent<ExpansionRequirementComponent>(out var masterBodyExpansionRequirementComponent))
                {
                    expansions.Add(masterBodyExpansionRequirementComponent.requiredExpansion);
                }


                var spawnCondition = variantDef.spawnCondition;
                if(spawnCondition != null)
                {
                    foreach(var expansion in spawnCondition.requiredExpansionDefs)
                    {
                        HG.ListUtils.AddIfUnique(expansions, expansion);
                    }
                }
                return expansions;


            }
            choiceDef.requiredUnlockables = GetRequiredUnlockableDefs();
            choiceDef.requiredExpansionDefs = GetRequiredExpansionDefs();
            choiceDef.requiredChoiceDefs.Add(choiceDef.tiedPackEnabledChoice);
            foreach(var expansion in choiceDef.requiredExpansionDefs)
            {
                choiceDef.requiredChoiceDefs.Add(expansion.enabledChoice);
            }
        }
    }

    internal sealed class VAPIRuleChoiceDef : ExtendedRuleChoiceDef
    {
        public CharacterVariantIndex variantIndex = CharacterVariantIndex.none;

        public VariantPackIndex variantPackIndex = VariantPackIndex.None;

        public RuleChoiceDef tiedPackEnabledChoice;

        public static VAPIRuleChoiceDef CreateAndAddToRule(RuleDef ruleDef, string choiceName, object extraData = null, bool excludeByDefault = false)
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
    }

}