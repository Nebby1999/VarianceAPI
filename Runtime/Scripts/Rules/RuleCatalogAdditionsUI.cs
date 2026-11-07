using MSU;
using RoR2;
using RoR2.UI;
using RoR2BepInExPack;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace VAPI.Rules
{
    internal static class RuleCatalogAdditionsUI
    {
        private static Sprite _lockSprite;
        private static Sprite _texUIHeaderSharp;

        internal static IEnumerator Init()
        {
            using var spriteRequest = VAPIAssets.LoadAssetAsync<Sprite>("texUIHeaderSharp");
            var lockSpriteRequest = Addressables.LoadAssetAsync<Sprite>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Common_MiscIcons.texUnlockIcon_png_texUnlockIcon_);

            ParallelCoroutine parallelCoroutine = ParallelCoroutine.RequestFromPool();

            parallelCoroutine.Add(spriteRequest);
            parallelCoroutine.Add(lockSpriteRequest);

            while(!parallelCoroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelCoroutine);
            _lockSprite = lockSpriteRequest.Result;
            _texUIHeaderSharp = spriteRequest.asset;

            On.RoR2.UI.RuleChoiceController.SetChoice += RuleChoiceController_SetChoice;
            On.RoR2.UI.RuleChoiceController.UpdateChoiceDisplay += RuleChoiceController_UpdateChoiceDisplay;
            On.RoR2.UI.RuleCategoryController.SetData += RuleCategoryController_SetData;
        }

        private static void RuleCategoryController_SetData(On.RoR2.UI.RuleCategoryController.orig_SetData orig, RoR2.UI.RuleCategoryController self, RuleCategoryDef categoryDef, RuleChoiceMask availability, RuleBook ruleBook)
        {
            orig(self, categoryDef, availability, ruleBook);
            if(categoryDef is VAPIRuleCategoryDef vapiCategoryDef && vapiCategoryDef.associatedBodyIndex != BodyIndex.None)
            {
                CharacterBody bodyComponent = BodyCatalog.GetBodyPrefabBodyComponent(vapiCategoryDef.associatedBodyIndex);

                if (self.popoutPanelInstance)
                {
                    self.popoutPanelInstance.popoutPanelDescriptionText.formatArgs = new object[] { Language.GetString(bodyComponent.baseNameToken) };
                }
                if (self.rulesToDisplay != null && self.popoutButtonIconAllocator?.elements != null)
                {
                    for (int i = 0; i < self.rulesToDisplay.Count; i++)
                    {
                        self.popoutButtonIconAllocator.elements[i].UpdateFromVotes();
                    }
                }
                RuleBookViewer ruleBookViewer = self.GetComponentInParent<RuleBookViewer>();
                if (ruleBookViewer && ruleBookViewer.categoryElementAllocator != null)
                {
                    RuleCategoryController bodyVariantsCategoryController = ruleBookViewer.categoryElementAllocator?.elements?.FirstOrDefault(x => x.currentCategory != null && x.currentCategory == RuleCatalogAdditions.bodyVariantsCategory);
                    if (bodyVariantsCategoryController && bodyVariantsCategoryController.rulesToDisplay != null)
                    {
                        int ruleIndex = bodyVariantsCategoryController.rulesToDisplay.FindIndex(x => x.choices[x.defaultChoiceIndex].extraData as CharacterBody == bodyComponent);
                        if (ruleIndex >= 0)
                        {
                            RuleChoiceController ruleChoiceController = bodyVariantsCategoryController.voteResultIconAllocator?.elements?[ruleIndex];
                            if (ruleChoiceController && ruleChoiceController.TryGetComponent(out HGButton hGButton))
                            {
                                hGButton.onClick.RemoveListener(self.TogglePopoutPanel);
                                hGButton.onClick.AddListener(self.TogglePopoutPanel);
                                hGButton.allowAllEventSystems = true;
                                hGButton.submitOnPointerUp = true;
                                hGButton.selectOnPointerEnter = true;
                                hGButton.disablePointerClick = false;
                                hGButton.disableGamepadClick = false;
                                hGButton.defaultFallbackButton = true;
                                hGButton.navigation = new Navigation { mode = Navigation.Mode.Automatic };
                                //hGButton.requiredTopLayer = self.stripPrefab?.GetComponent<RuleBookViewerStrip>()?.choicePrefab?.GetComponentInChildren<HGButton>()?.requiredTopLayer;
                            }
                        }
                    }
                }
                var hidden = new GameObject();
                hidden.transform.SetParent(self.transform.parent, false);
                self.transform.SetParent(hidden.transform);
                hidden.SetActive(false);
            }
            else if (categoryDef == RuleCatalogAdditions.bodyVariantsCategory)
            {
                GridLayoutGroup gridLayoutGroup = self.voteResultGridContainer ? self.voteResultGridContainer.GetComponent<GridLayoutGroup>() : null;
                if (gridLayoutGroup)
                {
                    gridLayoutGroup.padding = new RectOffset(6, 6, 12, 12);
                    gridLayoutGroup.spacing = Vector2.zero;
                }
            }
        }

        private static void RuleChoiceController_UpdateChoiceDisplay(On.RoR2.UI.RuleChoiceController.orig_UpdateChoiceDisplay orig, RoR2.UI.RuleChoiceController self, RuleChoiceDef displayChoiceDef)
        {
            orig(self, displayChoiceDef);
            if (displayChoiceDef.extraData is BodyVariantPackContentRule bodyVariantPackContentRule)
            {
                ReadOnlyVariantPack variantPack = VariantPackManager.GetVariantPack(bodyVariantPackContentRule.associatedPackIndex);
                CharacterBody body = BodyCatalog.GetBodyPrefabBodyComponent(bodyVariantPackContentRule.associatedBodyIndex);

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
                    Sprite sprite = _lockSprite;
                    if (bodyVariantPackContentRule.enabled)
                    {
                        sprite = variantPack.packIcon;
                    }
                    subIcon.SetAssetOrDirectReference(new AssetReferenceT<Sprite>(""), sprite);
                }
                foreach (RuleChoiceController ruleChoiceController in RuleChoiceController.instancesList)
                {
                    if (ruleChoiceController.choiceDef != null && ruleChoiceController.choiceDef.extraData as CharacterBody == body)
                    {
                        ruleChoiceController.UpdateChoiceDisplay(ruleChoiceController.choiceDef);
                    }
                }
            }
            else if (displayChoiceDef.extraData is CharacterBody characterBody)
            {
                if (!RuleCatalogAdditions.bodyToVariantCategories.TryGetValue(characterBody.bodyIndex, out VAPIRuleCategoryDef variantsCategoryDef))
                {
                    return;
                }

                if (PreGameController.instance && self.tooltipProvider && variantsCategoryDef.children.Count > 0)
                {
                    self.tooltipProvider.overrideBodyText = null;
                    string bodyText = self.tooltipProvider.bodyText;
                    bodyText += "\n" + (displayChoiceDef.localName == "On" ? "Interact to enable or disable specific variants\n" : "");
                    foreach (RuleDef ruleDef in variantsCategoryDef.children.OrderByDescending(x => PreGameController.instance.readOnlyRuleBook.GetRuleChoiceIndex(x) == x.defaultChoiceIndex))
                    {
                        RuleChoiceDef choice = PreGameController.instance.readOnlyRuleBook.GetRuleChoice(ruleDef);
                        if (choice != null)
                        {
                            bodyText += Language.GetStringFormatted("VAPI_VARIANT_CHOICE_" + choice.localName.ToUpperInvariant(), choice.getTooltipName(choice));
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
                    baseOutline.sprite = _texUIHeaderSharp;
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
                    hoverOutline.sprite = _texUIHeaderSharp;
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
                if (PreGameController.instance && variantsCategoryDef.children.Count > 0)
                {
                    rulesNonDefault = variantsCategoryDef.children.Any(x => PreGameController.instance.readOnlyRuleBook.GetRuleChoiceIndex(x) != x.defaultChoiceIndex);
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
            if (self.choiceDef?.extraData is CharacterBody && newChoiceDef?.extraData is not CharacterBody)
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
    }
}