using RoR2.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace VAPI.Editor
{
    public abstract class VAPIPropertyDrawer<T> : ExtendedPropertyDrawer<T>
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            base.CreatePropertyGUI(property);
            var root = VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, null, ValidatePath);
            FinishUI(root, property);
            return root;
        }

        protected virtual void FinishUI(VisualElement rootVisualElement, SerializedProperty serializedProperty)
        {

        }

        protected bool ValidatePath(string path)
        {
            return path.Contains("varianceapi");
        }
    }
}