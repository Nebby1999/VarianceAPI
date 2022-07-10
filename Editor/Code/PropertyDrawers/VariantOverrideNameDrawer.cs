using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using RoR2;

namespace VAPI.EditorUtils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VariantDef.VariantOverrideName))]
    public class VariantOverrideNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var fieldRect = new Rect(position.x, position.y, position.width * 0.75f, position.height);
            EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("token"));

            var overrideProp = property.FindPropertyRelative("overrideType");
            var enumRect = new Rect(fieldRect.xMax, fieldRect.y, position.width * 0.25f, position.height);
            overrideProp.intValue = (int)(OverrideNameType)EditorGUI.EnumPopup(enumRect, (OverrideNameType)overrideProp.intValue);

            EditorGUI.EndProperty();
        }
    }
}