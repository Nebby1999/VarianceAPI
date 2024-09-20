using RoR2.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System;

namespace VAPI.Editor.Inspectors
{
    [CustomEditor(typeof(VariantDef))]
    public sealed class VariantDefInspector : VAPIScriptableInspector<VariantDef>
    {
        PropertyField _variantTier;
        PropertyField _variantTierDef;
        PropertyField _arrivalToken;

        private void OnTierSet(ChangeEvent<string> evt = null)
        {
            VariantTierIndex index = evt == null ? targetType.variantTier : (VariantTierIndex)Enum.Parse(typeof(VariantTierIndex), evt.newValue.Replace(" ", ""));

            _variantTierDef.SetDisplay(index == VariantTierIndex.AssignedAtRuntime);
        }

        private void AutoPopulateToken(DropdownMenuAction dma)
        {
            string tokenBase = R2EKSettings.instance.GetTokenAllUpperCase();
            targetType.arrivalToken = $"{tokenBase}_{name.ToUpperInvariant()}_ARRIVAL";
            serializedObject.ApplyModifiedProperties();
        }

        protected override void InitializeVisualElement(VisualElement templateInstanceRoot)
        {
            _variantTier = templateInstanceRoot.Q<PropertyField>("variantTier");
            _variantTierDef = templateInstanceRoot.Q<PropertyField>("variantTierDef");
            _arrivalToken = templateInstanceRoot.Q<PropertyField>("arrivalToken");

            _variantTier.RegisterCallback<ChangeEvent<string>>(OnTierSet);
            OnTierSet();

            _arrivalToken.AddSimpleContextMenu(new ContextMenuData
            {
                menuAction = AutoPopulateToken,
                menuName = "Auto Populate",
                actionStatusCheck = dma => R2EKSettings.instance.tokenExists ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled
            });
        }
    }
}