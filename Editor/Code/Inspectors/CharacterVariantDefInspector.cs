using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.Inspectors
{
    [CustomEditor(typeof(CharacterVariantDef))]
    public sealed class CharacterVariantDefInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement() { name = $"{nameof(CharacterVariantDefInspector)}_Root" };
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            return root;
        }
    }
}