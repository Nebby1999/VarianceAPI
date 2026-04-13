using RoR2;
using RoR2.Editor;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VariantSkillReplacement))]
    public class VariantSkillReplacementDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(VariantSkillReplacementDrawer), null, s => s.Contains("varianceapi"));

            root.Q<PropertyField>().BindProperty(property.FindPropertyRelative(nameof(VariantSkillReplacement.skillDef)));

            var slotNameTextField = root.Q<TextField>();
            slotNameTextField.BindProperty(property.FindPropertyRelative(nameof(VariantSkillReplacement.slotName)));
            
            var slotEnumField = root.Q<EnumField>();
            slotEnumField.RegisterValueChangedCallback(e =>
            {
                Enum val = e.newValue;
                if(val == null)
                {
                    val = e.previousValue;
                }
                slotNameTextField?.SetDisplay(((SkillSlot)val) == SkillSlot.None);
            });
            slotEnumField.BindProperty(property.FindPropertyRelative(nameof(VariantSkillReplacement.slot)));

            return root;
        }
    }
}