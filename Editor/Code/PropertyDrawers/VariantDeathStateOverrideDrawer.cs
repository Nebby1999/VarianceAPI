using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VAPI.Editor;

namespace VAPI
{
    [CustomPropertyDrawer(typeof(VariantDeathStateOverride))]
    public class VariantDeathStateOverrideDrawer : VAPIPropertyDrawer<VariantDeathStateOverride>
    {
        protected override void FinishUI(VisualElement rootVisualElement, SerializedProperty serializedProperty)
        {
            rootVisualElement.Q<PropertyField>().BindProperty(serializedProperty.FindPropertyRelative("deathStateOverride"));
        }
    }
}