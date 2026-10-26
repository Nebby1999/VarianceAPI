using RoR2.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(AddressableItemCountPair))]
    public class AddressableItemCountPairDrawer : VAPIPropertyDrawer<AddressableItemCountPair>
    {
        protected override void FinishUI(VisualElement rootVisualElement, SerializedProperty serializedProperty)
        {
            rootVisualElement.Q<PropertyField>().BindProperty(serializedProperty.FindPropertyRelative("itemDef"));

            var integerField = rootVisualElement.Q<IntegerField>();
            integerField.isDelayed = true;
            var countProperty = serializedProperty.FindPropertyRelative("count");
            integerField.RegisterValueChangedCallback((evt) =>
            {
                countProperty.intValue = Mathf.Max(evt.newValue, 0);
                integerField.value = countProperty.intValue;
            });
            integerField.BindProperty(countProperty);
        }
    }
}