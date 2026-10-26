using RoR2.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VariantInventoryDefinition.AddressableEquipmentInfo))]
    public sealed class AddressableEquipmentInfoDrawer : VAPIPropertyDrawer<VariantInventoryDefinition.AddressableEquipmentInfo>
    {
        protected override void FinishUI(VisualElement rootVisualElement, SerializedProperty serializedProperty)
        {
            rootVisualElement.Q<PropertyField>().BindProperty(serializedProperty.FindPropertyRelative("equipmentDef"));

            var canTriggerEquipmentInfo = rootVisualElement.Q<VisualElement>("CanTriggerEquipmentInfo");
            var canTriggerEquipmentToggle = rootVisualElement.Q<Toggle>();
            canTriggerEquipmentToggle.BindProperty(serializedProperty.FindPropertyRelative("canTriggerEquipment"));
            canTriggerEquipmentToggle.RegisterValueChangedCallback(e => canTriggerEquipmentInfo.SetDisplay(e.newValue));

            canTriggerEquipmentInfo.Q<FloatField>("AIMaxUseHealthFraction").BindProperty(serializedProperty.FindPropertyRelative("aiMaxUseHealthFraction"));
            canTriggerEquipmentInfo.Q<FloatField>("AIMaxUseDistance").BindProperty(serializedProperty.FindPropertyRelative("aiMaxUseDistance"));
            canTriggerEquipmentInfo.Q<FloatField>("TimeBetweenEquipmentSwitches").BindProperty(serializedProperty.FindPropertyRelative("timeBetweenEquipmentSwitches"));
        }
    }
}