using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

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
        private static Sprite texUIHeaderSharp;
        internal static RuleDef _varianceArtifactRuleDef;

        //Dictionary that stores the variant index with it's rule def
        private static Dictionary<CharacterVariantIndex, RuleDef> _variantIndexToRuleDef = new Dictionary<CharacterVariantIndex, RuleDef>();

        [SystemInitializer(typeof(RuleCatalog), typeof(CharacterVariantCatalog), typeof(CharacterVariantTierCatalog))]
        private static void Init()
        {
            _varianceExpansionRuleChoice = VAPIContent.VarianceAPI.asset.enabledChoice;

            //Add the VariantPack Category, so that users can enable and disable packs at their whim
            _variantPackCategory = AddRuleCategory("VARIANTPACKS", () =>
            {
                if (!PreGameController.instance || PreGameController.instance.readOnlyRuleBook == null)
                    return true;

                bool isVAPIExpansionActive = PreGameController.instance.readOnlyRuleBook.IsChoiceActive(_varianceExpansionRuleChoice);

                //Hide category is expansion not active
                return isVAPIExpansionActive == false;
            });
            LogMessage($"Added VariantPackCategory");
            //Add the VariantPack Rules themselves.
            AddVariantPackRules();

            //Add the Charactervariant category, this category has icons for each body. The rules enables and disables the entire character's variants.
            //An individual rule may be clicked and that'll allow the end user to toggle on or off each variant specifically.
            _variantCategory = AddRuleCategory("VARIANTS", () =>
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

                bool isVAPIExpansionActive = PreGameController.instance.readOnlyRuleBook.IsChoiceActive(_varianceExpansionRuleChoice);
                if (isVAPIExpansionActive)
                    return false;

                return true;
            });
            LogMessage($"Added VariantCategory");
            AddBodyVariantRules();

            texUIHeaderSharp = VAPIAssets.LoadAsset<Sprite>("texUIHeaderSharp");

            On.RoR2.UI.RuleChoiceController.SetChoice += RuleChoiceController_SetChoice;
            On.RoR2.UI.RuleChoiceController.UpdateChoiceDisplay += RuleChoiceController_UpdateChoiceDisplay;
            On.RoR2.UI.RuleCategoryController.SetData += RuleCategoryController_SetData;
            /*
            AddCharacterVariantCategory();
            AddVariantRulesToBodyVariantRules();

            _varianceArtifactRuleDef = RuleCatalog.FindRuleDef("Artifacts.Variance");

            RoR2.Language.onCurrentLanguageChanged += RecomputeTokenValues;
            VAPILog.Info($"VAPI Rulebook initialized");*/
        }

        private static void RuleCategoryController_SetData(On.RoR2.UI.RuleCategoryController.orig_SetData orig, RoR2.UI.RuleCategoryController self, RuleCategoryDef categoryDef, RuleChoiceMask availability, RuleBook ruleBook)
        {
        }

        private static void RuleChoiceController_UpdateChoiceDisplay(On.RoR2.UI.RuleChoiceController.orig_UpdateChoiceDisplay orig, RoR2.UI.RuleChoiceController self, RuleChoiceDef displayChoiceDef)
        {
            orig(self, displayChoiceDef);
            if (displayChoiceDef.extraData is VAPIRuleChoiceDef packContentRule)
            {
                ExpansionDef expansionDef = packContentRule.expansionDef;
                if (self.image)
                {
                    Transform subIconTransform = self.image.transform.parent.Find("SubIcon");
                    AsyncImage subIcon = subIconTransform ? subIconTransform.GetComponent<AsyncImage>() : null;
                    if (!subIcon)
                    {
                        subIcon = new GameObject("SubIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(AsyncImage)).GetComponent<AsyncImage>();
                        subIcon.transform.SetParent(self.image.transform.parent, false);
                        Transform hoverOutline = subIcon.transform.parent.Find("HoverOutline");
                        if (hoverOutline)
                        {
                            subIcon.transform.SetSiblingIndex(hoverOutline.transform.GetSiblingIndex());
                        }
                        subIcon.gameObject.layer = LayerIndex.ui.intVal;
                        RectTransform rectTransform = subIcon.GetComponent<RectTransform>();
                        rectTransform.sizeDelta = new Vector2(32f, 32f);
                        rectTransform.localPosition = new Vector3(-20f, 20f);
                    }
                    AssetReferenceT<Sprite> asset = expansionDef.disabledIconSpriteReference;
#pragma warning disable CS0618 // Type or member is obsolete
                    Sprite sprite = expansionDef.disabledIconSprite;
                    if (packContentRule.enabled)
                    {
                        sprite = expansionDef.iconSprite;
#pragma warning restore CS0618 // Type or member is obsolete
                        asset = expansionDef.iconSpriteReference;
                    }
                    subIcon.SetAssetOrDirectReference(asset, sprite);
                }
                foreach (RuleChoiceController ruleChoiceController in RuleChoiceController.instancesList)
                {
                    if (ruleChoiceController.choiceDef != null && ruleChoiceController.choiceDef.extraData as ExpansionDef == expansionDef)
                    {
                        ruleChoiceController.UpdateChoiceDisplay(ruleChoiceController.choiceDef);
                    }
                }
            }
            else if (displayChoiceDef.extraData is ExpansionDef expansionDef)
            {
                if (!ExpansionRulesCatalog.expansionByRuleCategory.TryGetValue(expansionDef.expansionIndex, out RuleCategoryDef expansionCategoryDef))
                {
                    return;
                }

                if (PreGameController.instance && self.tooltipProvider && expansionCategoryDef.children.Count > 0)
                {
                    self.tooltipProvider.overrideBodyText = null;
                    string bodyText = self.tooltipProvider.bodyText;
                    bodyText += "\n";
                    foreach (RuleDef ruleDef in expansionCategoryDef.children.OrderByDescending(x => PreGameController.instance.readOnlyRuleBook.GetRuleChoiceIndex(x) == x.defaultChoiceIndex))
                    {
                        RuleChoiceDef choice = PreGameController.instance.readOnlyRuleBook.GetRuleChoice(ruleDef);
                        if (choice != null)
                        {
                            bodyText += Language.GetStringFormatted("EXPANSION_CONTENT_" + choice.localName.ToUpperInvariant(), choice.getTooltipName(choice));
                        }
                    }
                    self.tooltipProvider.overrideBodyText = bodyText;
                }

                if (!self.GetComponentInParent<RuleCategoryController>())
                {
                    return;
                }

                Transform baseOutlineTransform = self.transform.Find("BaseOutline");
                Image baseOutline = baseOutlineTransform ? baseOutlineTransform.GetComponent<Image>() : null;
                Transform hoverOutlineTransform = self.transform.Find("HoverOutline");
                Image hoverOutline = hoverOutlineTransform ? hoverOutlineTransform.GetComponent<Image>() : null;

                if (!baseOutline)
                {
                    baseOutline = new GameObject("BaseOutline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                    baseOutline.transform.SetParent(self.transform, false);
                    baseOutline.gameObject.layer = LayerIndex.ui.intVal;
                    baseOutline.sprite = texUIHeaderSharp;
                    baseOutline.color = new Color32(255, 255, 255, 30);
                    RectTransform rectTransform = baseOutline.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(48f, 8f);
                    rectTransform.localPosition = new Vector3(0f, -30f, 0f);
                }
                if (!hoverOutline)
                {
                    hoverOutline = new GameObject("HoverOutline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                    hoverOutline.transform.SetParent(self.transform, false);
                    hoverOutline.gameObject.layer = LayerIndex.ui.intVal;
                    hoverOutline.sprite = texUIHeaderSharp;
                    hoverOutline.color = Color.white;
                    RectTransform rectTransform = hoverOutline.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(48f, 8f);
                    rectTransform.localPosition = new Vector3(0f, -32f, 0f);
                }
                if (self.hgButton)
                {
                    self.hgButton.showImageOnHover = true;
                    self.hgButton.imageOnInteractable = baseOutline;
                    self.hgButton.imageOnHover = hoverOutline;
                }

                Transform notificationIconTransform = self.transform.Find("NotificationIcon");
                Image notificationIcon = notificationIconTransform ? notificationIconTransform.GetComponent<Image>() : null;

                bool rulesNonDefault = false;
                if (PreGameController.instance && expansionCategoryDef.children.Count > 0)
                {
                    rulesNonDefault = expansionCategoryDef.children.Any(x => PreGameController.instance.readOnlyRuleBook.GetRuleChoiceIndex(x) != x.defaultChoiceIndex);
                }

                if (rulesNonDefault && !notificationIcon)
                {
                    notificationIcon = new GameObject("NotificationIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                    notificationIcon.transform.SetParent(self.transform, false);
                    notificationIcon.gameObject.layer = LayerIndex.ui.intVal;
                    notificationIcon.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texPlayerVoteStatus.png").WaitForCompletion();
                    notificationIcon.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/UI/matUIOverbrighten2x.mat").WaitForCompletion();
                    notificationIcon.color = new Color32(235, 235, 235, 200);
                    RectTransform rectTransform = notificationIcon.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(20f, 30f);
                    rectTransform.localPosition = new Vector3(16f, 16f);
                }

                if (notificationIcon)
                {
                    notificationIcon.gameObject.SetActive(rulesNonDefault);
                }

                if (self.image)
                {
                    FadedOutMeshEffect disabledMeshEffect = self.image.gameObject.GetComponent<FadedOutMeshEffect>();
                    if (rulesNonDefault && !disabledMeshEffect)
                    {
                        disabledMeshEffect = self.image.gameObject.AddComponent<FadedOutMeshEffect>();
                    }
                    if (disabledMeshEffect)
                    {
                        disabledMeshEffect.enabled = rulesNonDefault;
                    }
                }
            }
        }

        private static void RuleChoiceController_SetChoice(On.RoR2.UI.RuleChoiceController.orig_SetChoice orig, RoR2.UI.RuleChoiceController self, RuleChoiceDef newChoiceDef)
        {
            if(self.choiceDef?.extraData is VAPIRuleChoiceDef && newChoiceDef?.extraData is not VAPIRuleChoiceDef)
            {
                self.tooltipProvider.overrideBodyText = null;
                Transform notificationIconTransform = self.transform.Find("NotificationIcon");
                if (notificationIconTransform)
                {
                    notificationIconTransform.gameObject.SetActive(false);
                }
                if (self.image)
                {
                    FadedOutMeshEffect disabledMeshEffect = self.image.gameObject.GetComponent<FadedOutMeshEffect>();
                    if (disabledMeshEffect)
                    {
                        disabledMeshEffect.enabled = false;
                    }
                }
            }
            orig(self, newChoiceDef);
        }

        private static CategoryWithIndex AddRuleCategory(string baseToken, Func<bool> hiddenTest)
        {
            string displayToken = $"VAPI_RULE_HEADER_{baseToken}";

            var categoryDef = new RuleCategoryDef
            {
                displayToken = displayToken,
                subtitleToken = $"{displayToken}_SUBTITLE",
                editToken = $"{displayToken}_EDIT",
                emptyTipToken = $"{displayToken}_EMPTY",
                ruleCategoryType = RuleCatalog.RuleCategoryType.VoteResultGrid,
                color = VAPIConfig._variantHealthBarColor,
                hiddenTest = hiddenTest
            };

            return new CategoryWithIndex
            {
                category = categoryDef,
                categoryIndex = RuleCatalogExtras.AddCategory(categoryDef)
            };
        }

        private static void AddVariantPackRules()
        {
            RuleDef CreateRuleDefFromVariantPack(VariantPack variantPack)
            {
                RuleDef rule = new RuleDef($"VAPI.VariantPacks.{variantPack.identifier}", variantPack.nameToken);

                VAPIRuleChoiceDef onChoice = CreateAndAddRuleChoiceDef<VAPIRuleChoiceDef>(rule, "On");
                onChoice.sprite = variantPack.packIcon;
                onChoice.tooltipNameToken = variantPack.nameToken;
                onChoice.tooltipBodyToken = variantPack.descriptionToken;
                onChoice.tooltipNameColor = VAPIConfig._variantHealthBarColor;
                variantPack.packEnabledChoice = onChoice;
                rule.MakeNewestChoiceDefault();

                VAPIRuleChoiceDef offChoice = CreateAndAddRuleChoiceDef<VAPIRuleChoiceDef>(rule, "Off");
                offChoice.spritePath = "Textures/MiscIcons/texUnlockIcon";
                offChoice.tooltipNameToken = variantPack.nameToken;
                offChoice.tooltipBodyToken = variantPack.descriptionToken;
                offChoice.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
                offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;

                return rule;
            }

            for (int i = 0; i < VariantPackManager.registeredVariantPacks.Length; i++)
            {
                ReadOnlyVariantPack variantPack = VariantPackManager.registeredVariantPacks[i];

                //Do not add rule to the hidden packs, which is the main API pack
                if (variantPack.isHidden)
                {
                    LogMessage($"Skipping creating a RuleDef for {variantPack.identifier} as it's hidden");
                    continue;
                }

                RuleDef packRule = CreateRuleDefFromVariantPack(variantPack.src);
                RuleCatalogExtras.AddRuleToCatalog(packRule, _variantPackCategory.categoryIndex);
            }
        }

        private static void AddBodyVariantRules()
        {
            RuleDef CreateRuleDefFromBody(CharacterBody body)
            {
                Sprite icon = GetCharacterSprite(body);

                RuleDef rule = new RuleDef($"VAPI.CharacterVariants.{body.name}", body.baseNameToken);
                
                VAPIRuleChoiceDef onChoice = CreateAndAddRuleChoiceDef<VAPIRuleChoiceDef>(rule, "On");
                onChoice.sprite = icon;
                onChoice.tooltipNameToken = body.baseNameToken;
                onChoice.tooltipBodyToken = $"Variants for this character can spawn";
                onChoice.tooltipNameColor = VAPIConfig._variantHealthBarColor;
                SetRequiredValuesForChoice(onChoice, body);

                rule.MakeNewestChoiceDefault();

                VAPIRuleChoiceDef offChoice = CreateAndAddRuleChoiceDef<VAPIRuleChoiceDef>(rule, "Off");
                offChoice.spritePath = "Textures/MiscIcons/texUnlockIcon";
                offChoice.tooltipNameToken = body.baseNameToken;
                offChoice.tooltipBodyToken = "Variants for this character cannot spawn";
                offChoice.tooltipNameColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable);
                offChoice.getTooltipName = RuleChoiceDef.GetOffTooltipNameFromToken;
                SetRequiredValuesForChoice(offChoice, body);

                return rule;
            }
            List<CharacterBody> bodyWithVariants = new List<CharacterBody>();

            bool HasAtLeastOneNonHiddenVariant(CharacterVariantProvider provider)
            {
                foreach(var variant in provider.allVariants)
                {
                    if (variant.isHidden == false)
                        return true;
                }
                return false;
            }

            //Add the body components that have providers
            foreach(var bodyComponent in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                var variantDefProvider = CharacterVariantCatalog.GetCharacterVariantProvider(bodyComponent.bodyIndex);
                if (variantDefProvider == null || HasAtLeastOneNonHiddenVariant(variantDefProvider) == false)
                    continue;

                RuleDef characterRule = CreateRuleDefFromBody(bodyComponent);
                RuleCatalogExtras.AddRuleToCatalog(characterRule, _variantCategory.categoryIndex);
            }
        }

        private static void SetRequiredValuesForChoice(ExtendedRuleChoiceDef extendedRuleChoiceDef, CharacterBody body)
        {
            if(body.TryGetComponent<ExpansionRequirementComponent>(out var expansionRequirementComponent) && expansionRequirementComponent.requiredExpansion)
            {
                extendedRuleChoiceDef.requiredExpansionDefs.Add(expansionRequirementComponent.requiredExpansion);
                extendedRuleChoiceDef.requiredChoiceDefs.Add(expansionRequirementComponent.requiredExpansion.enabledChoice);
            }
        }

        private static TRuleChoiceDef CreateAndAddRuleChoiceDef<TRuleChoiceDef>(RuleDef parent, string choiceName) where TRuleChoiceDef : RuleChoiceDef, new()
        {
            TRuleChoiceDef result = new TRuleChoiceDef()
            {
                ruleDef = parent,
                localName = choiceName,
                globalName = $"{parent.globalName}.{choiceName}",
            };
            parent.choices.Add(result);
            return result;
        }

        private static Sprite GetCharacterSprite(CharacterVariantTarget target)
        {
            if (!target.TryLoadCharacterComponent(out var body))
            {
                return GetMysteryIcon();
            }

            return GetCharacterSprite(body);
        }

        private static Sprite GetCharacterSprite(CharacterBody bodyComponent)
        {
            if (bodyComponent.portraitIcon)
            {
                return CreateSpriteFromIcon((Texture2D)bodyComponent.portraitIcon);
            }

            return GetMysteryIcon();
        }

        private static Sprite CreateSpriteFromIcon(Texture2D icon) => Sprite.Create(icon, new Rect(0f, 0f, icon.width, icon.height), new Vector2(0.5f, 0.5f), 100);

        private static Sprite GetMysteryIcon() => UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>("2205ae9eb97f98e42ac56ee4a9e32474[texMysteryIcon]").WaitForCompletion();

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

        internal sealed class VAPIRuleChoiceDef : ExtendedRuleChoiceDef
        {

        }

        public class FadedOutMeshEffect : BaseMeshEffect
        {
            public Color color = new Color(1f, 1f, 1f, 0f);

            public override void ModifyMesh(VertexHelper vh)
            {
                if (!IsActive())
                {
                    return;
                }

                UIVertex vertex = default;
                for (int i = 0; i < vh.currentVertCount; i++)
                {
                    vh.PopulateUIVertex(ref vertex, i);
                    if (vertex.position.y > 0f && vertex.position.x > 0f)
                    {
                        vertex.color = color;
                        vh.SetUIVertex(vertex, i);
                    }
                }
            }
        }
        /*private static void RecomputeTokenValues()
        {
            foreach(var (variantIndex, ruleDef) in _variantIndexToRuleDef)
            {
                CharacterVariantDef variantDef = CharacterVariantCatalog.GetCharacterVariantDef(variantIndex);
                string variantName = GetIngameName(variantDef);

                ruleDef.displayToken = variantName;
                ruleDef.FindChoice("On").tooltipNameToken = variantName;
                ruleDef.FindChoice("Off").tooltipNameToken = variantName;
            }
            LogMessage($"Finished recomputing token values");
        }

        private static void AddVariantRulesToBodyVariantRules()
        {
            RuleDef CreateRuleDefFromVariant(CharacterVariantDef characterVariantDef)
            {
                Sprite icon = GetCharacterSprite(characterVariantDef.targetBody);
                string inGameVariantName = GetIngameName(characterVariantDef);

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
                return rule;
            }

            //Order them by body name, that way all the "beetle body" variants will appear next to each other
            using var _ = ListPool<CharacterVariantDef>.RentCollection(out var variantDefsOrderedByBodyName);
            for(int i = 0; i < CharacterVariantCatalog.characterVariantCount; i++)
            {
                var def = CharacterVariantCatalog.registeredCharacterVariantDefs[i];
                if(!def.hasProvider)
                {
                    LogMessage($"Skipping creating a RuleDef for {def} as it has no provider");
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

        private static Sprite GetCharacterSprite(CharacterVariantTarget target)
        {
            if(!target.TryLoadCharacterComponent(out var body))
            {
                return GetMysteryIcon();
            }

            return GetCharacterSprite(body);
        }

        private static Sprite GetCharacterSprite(CharacterBody bodyComponent)
        {
            if (bodyComponent.portraitIcon)
            {
                return CreateSpriteFromIcon((Texture2D)bodyComponent.portraitIcon);
            }

            return GetMysteryIcon();
        }

        private static Sprite CreateSpriteFromIcon(Texture2D icon) => Sprite.Create(icon, new Rect(0f, 0f, icon.width, icon.height), new Vector2(0.5f, 0.5f), 100);

        private static Sprite GetMysteryIcon() => UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>("2205ae9eb97f98e42ac56ee4a9e32474[texMysteryIcon]").WaitForCompletion();

        private static string GetIngameName(CharacterBody characterBody)
        {
            return Language.GetString(characterBody.baseNameToken);
        }

        private static string GetIngameName(CharacterVariantDef characterVariantDef)
        {
            string baseNameToken = "???";
            if(!characterVariantDef.targetBody.TryLoadCharacterComponent(out CharacterBody body))
            {
                return baseNameToken;
            }

            if(characterVariantDef.variantNameProvider == null)
            {
                return GetIngameName(body);
            }
            else
            {
                return characterVariantDef.variantNameProvider!.GetVariantName(GetIngameName(body));
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
    }*/
    }
    }