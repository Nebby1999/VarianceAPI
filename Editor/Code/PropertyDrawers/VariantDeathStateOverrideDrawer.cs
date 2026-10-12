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
        protected override void FinishUI()
        {
            rootVisualElement.Q<PropertyField>().BindProperty(serializedProperty.FindPropertyRelative("deathStateOverride"));
        }
    }
}