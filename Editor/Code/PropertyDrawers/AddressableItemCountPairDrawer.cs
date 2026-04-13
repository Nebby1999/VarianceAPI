using RoR2.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(AddressableItemCountPair))]
    public class AddressableItemCountPairDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(AddressableItemCountPairDrawer), null, (s => s.Contains("varianceapi")));
            root.Q<PropertyField>().BindProperty(property.FindPropertyRelative("itemDef"));

            var integerField = root.Q<IntegerField>();
            integerField.isDelayed = true;
            var countProperty = property.FindPropertyRelative("count");
            integerField.RegisterValueChangedCallback((evt) =>
            {
                countProperty.intValue = Mathf.Max(evt.newValue, 0);
                integerField.value = countProperty.intValue;
            });
            integerField.BindProperty(countProperty);
            return root;
        }
    }
}