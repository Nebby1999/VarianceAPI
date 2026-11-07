using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VariantComponentCollection))]
    public class VariantComponentCollectionDrawer : VAPIPropertyDrawer<VariantComponentCollection>
    {
        protected override void FinishUI(VisualElement rootVisualElement, SerializedProperty serializedProperty)
        {
            rootVisualElement.Q<PropertyField>().BindProperty(serializedProperty.FindPropertyRelative("variantComponents"));
        }
    }
}