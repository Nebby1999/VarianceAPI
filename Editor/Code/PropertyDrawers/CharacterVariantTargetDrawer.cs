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
            PropertyField pField = rootVisualElement.Q<PropertyField>();
            pField.BindProperty(serializedProperty.FindPropertyRelative("targetBody"));
        }
    }
}