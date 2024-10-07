using RoR2.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace VAPI.Editor.Inspectors
{
    [CustomEditor(typeof(VariantInventory))]
    public class VariantInventoryInspector : VAPIScriptableInspector<VariantInventory>
    {
        PropertyField _usable;
        VisualElement _usableData;

        protected override void InitializeVisualElement(VisualElement templateInstanceRoot)
        {
            _usable = templateInstanceRoot.Q<PropertyField>("usable");
            _usableData = templateInstanceRoot.Q<VisualElement>("usableData");

            _usable.RegisterCallback<ChangeEvent<bool>>(OnUsableChanged);
            OnUsableChanged();
        }
        private void OnUsableChanged(ChangeEvent<bool> evt = null)
        {
            bool val = evt == null ? targetType.equipmentInfo.usable : evt.newValue;
            _usableData.SetDisplay(val);
        }
    }
}