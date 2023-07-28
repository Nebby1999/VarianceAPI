using RoR2EditorKit.Inspectors;
using RoR2EditorKit;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System;

namespace VAPI.EditorUtils.Inspectors
{
    [CustomEditor(typeof(VariantDef))]
    public sealed class VariantDefInspector : VAPIScriptableInspector<VariantDef>
    {
        VisualElement inspectorData;

        PropertyField variantTier;
        PropertyField variantTierDef;
        PropertyField arrivalToken;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorData = container.Q<VisualElement>("InspectorData");

                variantTier = inspectorData.Q<PropertyField>("variantTier");
                variantTierDef = inspectorData.Q<PropertyField>("variantTierDef");
                arrivalToken = inspectorData.Q<PropertyField>("arrivalToken");
            };
        }
        protected override void DrawInspectorGUI()
        {
            variantTier.RegisterCallback<ChangeEvent<string>>(OnTierSet);
            OnTierSet();

            arrivalToken.AddSimpleContextMenu(new RoR2EditorKit.ContextMenuData("Auto Populate",
                AutoPopulateToken,
                dma => Settings.tokenPrefix.IsNullOrEmptyOrWhitespace() ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal));
        }

        private void OnTierSet(ChangeEvent<string> evt = null)
        {
            VariantTierIndex index = evt == null ? TargetType.variantTier : (VariantTierIndex)Enum.Parse(typeof(VariantTierIndex), evt.newValue.Replace(" ", ""));

            variantTierDef.SetDisplay(index == VariantTierIndex.AssignedAtRuntime);
        }

        private void AutoPopulateToken(DropdownMenuAction dma)
        {
            string tokenBase = Settings.GetPrefixUppercase();
            TargetType.arrivalToken = $"{tokenBase}_{name.ToUpperInvariant()}_ARRIVAL";
            serializedObject.ApplyModifiedProperties();
        }
    }
}