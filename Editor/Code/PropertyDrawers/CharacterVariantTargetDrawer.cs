using RoR2.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(CharacterVariantTarget))]
    public class CharacterVariantTargetDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(CharacterVariantTargetDrawer), null, (s => s.Contains("varianceapi")));

            TextField textField = root.Q<TextField>("Key");
            textField.BindProperty(property.FindPropertyRelative("key"));

            EnumField enumField = root.Q<EnumField>("TargetType");
            enumField.BindProperty(property.FindPropertyRelative("type"));
            return root;
        }
    }
}