using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VariantBuffStorage))]
    public class VariantBuffStorageDrawer : VAPIPropertyDrawer<VariantBuffStorage>
    {
        protected override void FinishUI()
        {
            rootVisualElement.Q<PropertyField>().BindProperty(serializedProperty.FindPropertyRelative("buffInfos"));
        }
    }
}