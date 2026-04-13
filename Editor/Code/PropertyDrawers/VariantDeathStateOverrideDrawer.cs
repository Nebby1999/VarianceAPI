using UnityEditor;
using UnityEngine;

namespace VAPI
{
    [CustomPropertyDrawer(typeof(VariantDeathStateOverride))]
    public class VariantDeathStateOverrideDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("deathStateOverride"));
            EditorGUI.EndProperty();
        }
    }
}