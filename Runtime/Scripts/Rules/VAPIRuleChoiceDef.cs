#nullable enable
using R2API;
using RoR2;
using UnityEngine;

namespace VAPI.Rules
{
    internal sealed class VAPIRuleChoiceDef : ExtendedRuleChoiceDef
    {
        public CharacterVariantIndex variantIndex = CharacterVariantIndex.none;

        public VariantPackIndex variantPackIndex = VariantPackIndex.None;

        public RuleChoiceDef? tiedPackEnabledChoice = null;

        public static VAPIRuleChoiceDef CreateAndAddToRule(RuleDef ruleDef, bool choiceKind, Sprite? sprite, object? extraData = null)
        {
            string localName = choiceKind ? "On" : "Off";
            VAPIRuleChoiceDef ruleChoice = new VAPIRuleChoiceDef()
            {
                ruleDef = ruleDef,
                localName = localName,
                globalName = $"{ruleDef.globalName}.{localName}",
                extraData = extraData,
                excludeByDefault = false,
                tooltipNameColor = choiceKind ? VAPIConfig._variantHealthBarColor : ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable),
            };
            if (choiceKind)
            {
                ruleChoice.sprite = sprite;
            }
            else
            {
                ruleChoice.spritePath = "Textures/MiscIcons/texUnlockIcon";
            }
            ruleDef.choices.Add(ruleChoice);
            return ruleChoice;
        }
    }
}