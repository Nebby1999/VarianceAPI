using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using RoR2;

namespace VAPI.EditorUtils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(VariantInventory.ItemPair))]
    public class ItemPairDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var fieldRect = new Rect(position.x, position.y, position.width * 0.75f, position.height);
            EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("item"));

            var amountProp = property.FindPropertyRelative("amount");
            var enumRect = new Rect(fieldRect.xMax, fieldRect.y, position.width * 0.25f, position.height);
            amountProp.intValue = EditorGUI.IntField(enumRect, amountProp.intValue);
        }
    }
}