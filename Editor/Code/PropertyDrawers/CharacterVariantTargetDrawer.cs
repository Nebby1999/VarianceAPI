using RoR2.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(CharacterVariantTarget))]
    public class CharacterVariantTargetDrawer : VAPIPropertyDrawer<CharacterVariantTarget>
    {
        protected override void FinishUI()
        {
            TextField textField = rootVisualElement.Q<TextField>("Key");
            textField.BindProperty(serializedProperty.FindPropertyRelative("key"));

            EnumField enumField = rootVisualElement.Q<EnumField>("TargetType");
            enumField.BindProperty(serializedProperty.FindPropertyRelative("type"));
        }
    }
}