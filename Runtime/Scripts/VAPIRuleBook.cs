using RoR2.ExpansionManagement;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using VAPI.AddressReferencedAssets;
using HG;
using System.Linq;
using UnityEngine.Networking;

namespace VAPI
{
    //Does this need to be injected into the rulebook?
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

        //Dictionary that stores the variant index with it's rule def
        private static Dictionary<CharacterVariantIndex, RuleDef> _variantIndexToRuleDef = new Dictionary<CharacterVariantIndex, RuleDef>();

        [SystemInitializer(typeof(RuleCatalog), typeof(CharacterVariantCatalog), typeof(CharacterVariantTierCatalog))]
        private static void Init()
        {
            _varianceExpansionRuleChoice = VAPIContent.VarianceAPI.asset.enabledChoice;

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
            bool VariantPackCategoryHiddenTest()
            {
                if (!PreGameController.instance || PreGameController.instance.readOnlyRuleBook == null)
                    return true;

                bool isVAPIExpansionActive = PreGameController.instance.readOnlyRuleBook.IsChoiceActive(_varianceExpansionRuleChoice);

                //Hide category is expansion not active
                return isVAPIExpansionActive == false;
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
                Sprite icon = GetTargetCharacterSprite(characterVariantDef.targetBody);
                string inGameVariantName = GetInGameVariantName(characterVariantDef);

                RuleDef rule = new RuleDef($"Variants.{characterVariantDef.cachedName}", inGameVariantName);
                rule.displayToken = inGameVariantName;
                rule.hideLobbyDisplay = characterVariantDef.isHidden;

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

            //Order them by body name, that way all the "beetle body" variants will appear next to each other
            using var _ = ListPool<CharacterVariantDef>.RentCollection(out var variantDefsOrderedByBodyName);
            for(int i = 0; i < CharacterVariantCatalog.characterVariantCount; i++)
            {
                var def = CharacterVariantCatalog.registeredCharacterVariantDefs[i];
                if(!def.hasProvider)
                {
                    continue;
                }

                variantDefsOrderedByBodyName.Add(def);
            }

            //At this point since it has a provider it means the target body is not null and has a component.
            variantDefsOrderedByBodyName.Sort((a, b) =>
            {
                CharacterBody aBody = a.targetBody.LoadCharacterComponent()!;
                CharacterBody bBody = b.targetBody.LoadCharacterComponent()!;

                return aBody.name.CompareTo(bBody.name);
            });


            foreach (var def in variantDefsOrderedByBodyName)
            {
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

            var canChangeInRulebook = variantDef.spawnRate > 0;
            if(canChangeInRulebook == false)
            {
                //Make it so the rule def's default choice is the off choice
                ruleDef.defaultChoiceIndex = offChoice.localIndex;
                //hide the on choice
                onChoice.excludeByDefault = true;
            }
            else
            {
                //Make it so the rule def's default choice is the on choice
                ruleDef.defaultChoiceIndex = onChoice.localIndex;
                //show the on choice
                onChoice.excludeByDefault = false;
            }

            //If we're the server, and the pregame controller exists, apply rulebook choice when the config changes.
            if(NetworkServer.active && PreGameController.instance)
            {
                PreGameController.instance.ApplyChoice(canChangeInRulebook ? onChoice.globalIndex : offChoice.globalIndex);
            }
        }

        public static void GenerateCharacterVariantMask(this RuleBook book, CharacterVariantMask dest)
        {
            dest.Clear();
            for(int i = 0; i < book.ruleValues.Length; i++)
            {
                RuleChoiceDef ruleChoiceDef = RuleCatalog.GetRuleDef(i).choices[book.ruleValues[i]];
                if(ruleChoiceDef is VAPIRuleChoiceDef vapiRuleChoiceDef && vapiRuleChoiceDef.variantIndex != CharacterVariantIndex.none)
                {
                    dest.Add(vapiRuleChoiceDef.variantIndex);
                }
            }
        }

        private static Sprite GetTargetCharacterSprite(CharacterVariantTarget target)
        {
            Sprite CreateFromIcon(Texture2D icon) => Sprite.Create(icon, new Rect(0f, 0f, icon.width, icon.height), new Vector2(0.5f, 0.5f), 100);

            if(!target.TryLoadCharacterComponent(out var body))
            {
                return UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>("2205ae9eb97f98e42ac56ee4a9e32474[texMysteryIcon]").WaitForCompletion();
            }

            if(body.portraitIcon)
            {
                return CreateFromIcon((Texture2D)body.portraitIcon);
            }

            return UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>("2205ae9eb97f98e42ac56ee4a9e32474[texMysteryIcon]").WaitForCompletion();
        }

        private static string GetInGameVariantName(CharacterVariantDef characterVariantDef)
        {
            string baseNameToken = "???";
            if(!characterVariantDef.targetBody.TryLoadCharacterComponent(out CharacterBody body))
            {
                return baseNameToken;
            }

            baseNameToken = body.baseNameToken;

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
                if(!variantDef.targetBody.TryLoadCharacterComponent(out CharacterBody characterBody))
                {
                    return expansions;
                }

                if(characterBody.TryGetComponent<ExpansionRequirementComponent>(out var expansionRequirementComponent))
                {
                    expansions.Add(expansionRequirementComponent.requiredExpansion);
                }


                var spawnCondition = variantDef.spawnCondition;
                if(spawnCondition != null && spawnCondition.requiredExpansionDefs != null)
                {
                    foreach(var expansion in spawnCondition.requiredExpansionDefs)
                    {
                        if(expansion != null)
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