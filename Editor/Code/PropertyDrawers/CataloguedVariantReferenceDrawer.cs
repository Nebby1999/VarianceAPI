using RoR2.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(CataloguedVariantReference))]
    public sealed class CataloguedVariantReferenceDrawer : VAPIPropertyDrawer<CataloguedVariantReference>
    {
        protected override void FinishUI(VisualElement rootVisualElement, SerializedProperty serializedProperty)
        {
            SerializedProperty _characterVariantDef = serializedProperty.FindPropertyRelative("_characterVariantDef");
            SerializedProperty _cataloguedVariantName = serializedProperty.FindPropertyRelative("_cataloguedVariantName");

            var rootForContextMenu = rootVisualElement.Q<VisualElement>("RootForContextMenu");

            TextField cataloguedVariantNameTextField = rootVisualElement.Q<TextField>();
            ObjectField characterVariantDefObjectField = rootVisualElement.Q<ObjectField>();

            characterVariantDefObjectField.BindProperty(_characterVariantDef);
            characterVariantDefObjectField.RegisterValueChangedCallback((evt) =>
            {
                cataloguedVariantNameTextField.SetDisplay(evt.newValue == null);
            });

            cataloguedVariantNameTextField.BindProperty(_cataloguedVariantName);
            cataloguedVariantNameTextField.RegisterValueChangedCallback((evt) =>
            {
                characterVariantDefObjectField.SetDisplay(string.IsNullOrWhiteSpace(evt.newValue));
            });
        }
    }
}