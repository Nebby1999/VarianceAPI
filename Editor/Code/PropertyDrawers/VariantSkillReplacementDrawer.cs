using RoR2;
using RoR2.Editor;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VariantSkillReplacement))]
    public class VariantSkillReplacementDrawer : VAPIPropertyDrawer<VariantSkillReplacement>
    {
        protected override void FinishUI()
        {
            rootVisualElement.Q<PropertyField>().BindProperty(serializedProperty.FindPropertyRelative(nameof(VariantSkillReplacement.skillDef)));

            var slotNameTextField = rootVisualElement.Q<TextField>();
            slotNameTextField.BindProperty(serializedProperty.FindPropertyRelative(nameof(VariantSkillReplacement.slotName)));

            var slotEnumField = rootVisualElement.Q<EnumField>();
            slotEnumField.RegisterValueChangedCallback(e =>
            {
                Enum val = e.newValue;
                if (val == null)
                {
                    val = e.previousValue;
                }
                slotNameTextField?.SetDisplay(((SkillSlot)val) == SkillSlot.None);
            });
            slotEnumField.BindProperty(serializedProperty.FindPropertyRelative(nameof(VariantSkillReplacement.slot)));
        }
    }
}