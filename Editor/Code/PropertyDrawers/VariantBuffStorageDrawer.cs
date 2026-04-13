using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VariantBuffStorage))]
    public class VariantBuffStorageDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new PropertyField(property.FindPropertyRelative("buffInfos"));
        }
    }
}