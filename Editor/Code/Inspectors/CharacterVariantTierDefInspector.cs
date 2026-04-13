using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VAPI.Editor.Inspectors
{
    [CustomEditor(typeof(CharacterVariantTierDef))]
    public sealed class CharacterVariantTierDefInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement() { name = $"{nameof(CharacterVariantTierDef)}_Root" };
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            return root;
        }
    }
}