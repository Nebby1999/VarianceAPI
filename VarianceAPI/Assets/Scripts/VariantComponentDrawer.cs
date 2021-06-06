/*using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VarianceAPI;
using VarianceAPI.Components;
using Type = System.Type;

[CustomPropertyDrawer(typeof(VariantComponentTypeAttribute))]
public class VariantComponentDrawer : PropertyDrawer
{
    //static SearchSuggest<SEST> _suggestor;
    //static SearchSuggest<SEST> suggestor
    //{
    //    get
    //    {
    //        if (!_suggestor)
    //        {
    //            _suggestor = ScriptableObject.CreateInstance<SearchSuggest<SEST>>();
    //            if (suggestor)
    //            {
    //                suggestor.OnSuggestionGUI = RenderOption;
    //                suggestor.Evaluate = UpdateSearch;
    //            }
    //        }
    //        return _suggestor;
    //    }
    //}

    static List<Type> _Types;
    static List<Type> Types
    {
        get
        {
            if (_Types == null)
            {
                _Types = new List<Type>();
                foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in ass.GetTypes())
                    {
                        if(typeof(VariantComponent).IsAssignableFrom(type))
                        {
                            _Types.Add(type);
                        }
                    }
                }
            }
            return _Types;
        }
    }

    //private static IEnumerable<SEST> UpdateSearch(string searchString) => entityStates.Where(s => s.stateType.FullName.Contains(searchString));

    string searchText = "";
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) //TODO: Add search box and pretty it up
    {
        EditorGUI.BeginProperty(new Rect(position.x, position.y, position.width, GetPropertyHeight(property, label)), label, property);

        EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), label);

        var currentSearch = EditorGUI.TextField(new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), searchText != "" ? searchText : "Search");
        if (currentSearch != "Search") searchText = currentSearch;

        if (EditorGUI.DropdownButton(new Rect(position.x + EditorGUIUtility.labelWidth, position.y + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight, position.width - EditorGUIUtility.labelWidth, position.height), new GUIContent(label.text), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();

            var filteredTypes = Types
                .Where(type => type.FullName.ToLower().Contains(searchText.ToLower()))
                .GroupBy(type => type.Namespace)
                .SelectMany(grouping => grouping.AsEnumerable());
            foreach (var type in filteredTypes)
            {
                menu.AddItem(
                    new GUIContent(type.Namespace + "/" + type.FullName),
                    false,
                    (data) =>
                    {
                        var ser = data as SerializedProperty;
                        ser.FindPropertyRelative("_typeName").stringValue = type.AssemblyQualifiedName;
                        ser.serializedObject.ApplyModifiedProperties();
                    },
                    property
                );
            }

            if (filteredTypes.Count() == 0)
                menu.AddItem(new GUIContent("No items found for search"), false, null);

            menu.DropDown(new Rect(position.x + EditorGUIUtility.labelWidth, position.y + EditorGUIUtility.standardVerticalSpacing + (EditorGUIUtility.singleLineHeight * 2), position.width - EditorGUIUtility.labelWidth, 0));
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (2 * EditorGUIUtility.singleLineHeight) + EditorGUIUtility.standardVerticalSpacing;
    }

    //static bool RenderOption(int index, SEST state)
    //{
    //    if (GUILayout.Button(ObjectNames.NicifyVariableName(state.stateType.FullName)))
    //    {
    //        //AssetDatabase.AddObjectToAsset(state, target);

    //        //var stepField = runSteps.GetArrayElementAtIndex(runSteps.arraySize++);

    //        //stepField.objectReferenceValue = stepInstance;
    //        //stepField.serializedObject.SetIsDifferentCacheDirty();
    //        //stepField.serializedObject.ApplyModifiedProperties();

    //        //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stepInstance));
    //        //AssetDatabase.SaveAssets();
    //        //suggestor.Cleanup();
    //        return true;
    //    }
    //    return false;
    //}
}
*/