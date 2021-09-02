using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using VarianceAPI;

[CustomPropertyDrawer(typeof(SerializableVariantComponentType), true)]
public class VariantComponentDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var componentReference = property.FindPropertyRelative("_typeName");
        HandleDragAndDrop(componentReference, position);

        position = EditorGUI.PrefixLabel(position,
            GUIUtility.GetControlID(FocusType.Passive), label);

        var style = new GUIStyle(GUI.skin.button);
        style.alignment = TextAnchor.MiddleLeft;
        style.fontStyle = FontStyle.Normal;
        style.fontSize = 9;

        var componentName = GetComponentName(componentReference);
        if (string.IsNullOrEmpty(componentName))
        {
            componentName = "None";
            componentReference.stringValue = string.Empty;
            style.normal.textColor = new Color(0.7f, 0, 0);
        }

        if (GUI.Button(position, componentName, style))
        {
            new VariantComponentTreePicker.PickerCreator
            {
                variantComponentReference = componentReference,
                pickerPosition = GetLastRectAbsolute(position),
                serializedObject = property.serializedObject
            };
        }

        EditorGUI.EndProperty();
    }
    protected virtual string GetComponentName(SerializedProperty componentName)
    {
        string reference = componentName.stringValue;
        if (string.IsNullOrEmpty(reference) || Type.GetType(reference, false) == null)
            return string.Empty;
        else
            return new SerializableVariantComponentType(reference).componentType.Name;
    }

    private void HandleDragAndDrop(SerializedProperty componentReference, Rect dropArea)
    {
        var currentEvent = Event.current;
        if (!dropArea.Contains(currentEvent.mousePosition))
            return;

        if (currentEvent.type != EventType.DragUpdated && currentEvent.type != EventType.DragPerform)
            return;

        var reference = DragAndDrop.objectReferences[0];
        if (reference != null && !(reference is TextAsset))
            reference = null;

        DragAndDrop.visualMode = reference != null ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;

        if (currentEvent.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();

            if (reference != null)
                componentReference.stringValue = ((TextAsset)reference).text;
            DragAndDrop.PrepareStartDrag();
            GUIUtility.hotControl = 0;
        }
        currentEvent.Use();
    }


    public static Rect GetLastRectAbsolute(Rect relativePos)
    {
        Rect result = relativePos;
        result.x += EditorWindow.focusedWindow.position.x;
        result.y += EditorWindow.focusedWindow.position.y;
        try
        {
            Type type = EditorWindow.focusedWindow.GetType();
            FieldInfo field = type.GetField("s_CurrentInspectorWindow", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo field2 = type.GetField("m_ScrollPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Vector2 vector = (Vector2)field2.GetValue(field.GetValue(null));
            result.x -= vector.x;
            result.y -= vector.y;
        }
        catch
        {
        }
        return result;
    }

}