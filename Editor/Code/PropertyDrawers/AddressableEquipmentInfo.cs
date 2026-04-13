using RoR2.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VariantInventoryDefinition.AddressableEquipmentInfo))]
    public sealed class AddressableEquipmentInfoDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(AddressableEquipmentInfoDrawer), null, s => s.Contains("varianceapi"));

            root.Q<PropertyField>().BindProperty(property.FindPropertyRelative("equipmentDef"));

            var canTriggerEquipmentInfo = root.Q<VisualElement>("CanTriggerEquipmentInfo");
            var canTriggerEquipmentToggle = root.Q<Toggle>();
            canTriggerEquipmentToggle.BindProperty(property.FindPropertyRelative("canTriggerEquipment"));
            canTriggerEquipmentToggle.RegisterValueChangedCallback(e => canTriggerEquipmentInfo.SetDisplay(e.newValue));

            canTriggerEquipmentInfo.Q<FloatField>("AIMaxUseHealthFraction").BindProperty(property.FindPropertyRelative("aiMaxUseHealthFraction"));
            canTriggerEquipmentInfo.Q<FloatField>("AIMaxUseDistance").BindProperty(property.FindPropertyRelative("aiMaxUseDistance"));
            canTriggerEquipmentInfo.Q<FloatField>("TimeBetweenEquipmentSwitches").BindProperty(property.FindPropertyRelative("timeBetweenEquipmentSwitches"));

            return root;
        }
    }
}