using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;

namespace VAPI.Editor
{
    [CustomEditor(typeof(VariantSpawnCondition))]
    public class VariantSpawnConditionInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement RootVisualElement = new VisualElement();
            List<SerializedProperty> visibleChildren = base.serializedObject.GetVisibleChildren();
            foreach (SerializedProperty item in visibleChildren)
            {
                if (item.name == "m_Script")
                {
                    ObjectField objectField = new ObjectField();
                    objectField.SetObjectType<MonoScript>();
                    objectField.value = item.objectReferenceValue;
                    objectField.label = item.displayName;
                    objectField.name = item.name;
                    objectField.bindingPath = item.propertyPath;
                    objectField.SetEnabled(value: false);
                    RootVisualElement.Add(objectField);
                }
                else
                {
                    PropertyField propertyField = new PropertyField(item);
                    propertyField.name = item.name;
                    RootVisualElement.Add(new PropertyField(item));
                }
            }
            return RootVisualElement;
        }
    }
}