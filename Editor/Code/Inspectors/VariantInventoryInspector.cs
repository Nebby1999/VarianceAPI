using RoR2EditorKit.Inspectors;
using RoR2EditorKit;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace VAPI.EditorUtils.Inspectors
{
    [CustomEditor(typeof(VariantInventory))]
    public class VariantInventoryInspector : VAPIScriptableInspector<VariantInventory>
    {
        VisualElement inspectorDataContainer;
        PropertyField usable;
        VisualElement usableData;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorDataContainer = container.Q<VisualElement>("InspectorData");
                usable = inspectorDataContainer.Q<PropertyField>("usable");
                usableData = inspectorDataContainer.Q<VisualElement>("usableData");
            };
        }
        protected override void DrawInspectorGUI()
        {
            usable.RegisterCallback<ChangeEvent<bool>>(OnUsableChanged);
            OnUsableChanged();
        }

        private void OnUsableChanged(ChangeEvent<bool> evt = null)
        {
            bool val = evt == null ? TargetType.equipmentInfo.usable : evt.newValue;
            usableData.SetDisplay(val);
        }
    }
}