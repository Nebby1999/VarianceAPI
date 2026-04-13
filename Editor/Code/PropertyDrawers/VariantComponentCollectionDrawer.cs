using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VariantComponentCollection))]
    public class VariantComponentCollectionDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new PropertyField(property.FindPropertyRelative("variantComponents"));
        }
    }
}