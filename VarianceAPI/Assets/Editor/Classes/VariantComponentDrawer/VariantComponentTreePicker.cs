using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using VarianceAPI.Components;

public class VariantComponentTreePicker : EditorWindow
{
    private static VariantComponentTreePicker componentTreePicker;

    private readonly VariantComponentTreeView treeView = new VariantComponentTreeView();
    private bool close;
    private VariantComponentTreeView.VariantComponentTreeInfo selectedComponent;
    private SerializedProperty serializableVariantComponentReference;
    private SerializedObject serializedObject;

    public static EditorWindow LastFocusedWindow = null;

    private void Update()
    {
        if (close)
        {
            Close();

            if (LastFocusedWindow)
            {
                EditorApplication.delayCall += LastFocusedWindow.Repaint;
                LastFocusedWindow = null;
            }
        }
    }

    private void OnGUI()
    {
        using (new GUILayout.VerticalScope())
        {
            treeView.DisplayTreeView(TreeListControl.DisplayTypes.USE_SCROLL_VIEW);

            using (new GUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Ok"))
                {
                    //Get the selected item
                    var selectedItem = treeView.GetSelectedItem();
                    var data = (VariantComponentTreeView.VariantComponentTreeInfo)selectedItem?.DataContext;
                    if (selectedItem != null && data.itemType == VariantComponentTreeView.ItemType.variantComponent)
                        SetState(selectedItem);

                    //The window can now be closed
                    close = true;
                }
                else if (GUILayout.Button("Cancel"))
                    close = true;
                else if (GUILayout.Button("Reset"))
                {
                    ResetState();
                    close = true;
                }
                else if (Event.current.type == EventType.Used && treeView.LastDoubleClickedItem != null)
                {
                    //We must be in 'used' mode in order for this to work
                    SetState(treeView.LastDoubleClickedItem);
                    close = true;
                }
            }
        }
    }

    private void SetState(TreeListItem in_item)
    {
        serializedObject.Update();

        selectedComponent = in_item.DataContext as VariantComponentTreeView.VariantComponentTreeInfo;
        serializableVariantComponentReference.stringValue = selectedComponent.fullName;
        serializedObject.ApplyModifiedProperties();
    }

    private void ResetState()
    {
        serializedObject.Update();
        serializableVariantComponentReference.stringValue = typeof(VariantComponent).AssemblyQualifiedName;
        selectedComponent = null;
        serializedObject.ApplyModifiedProperties();
    }


    public class PickerCreator
    {
        public SerializedProperty variantComponentReference;
        public Rect pickerPosition;
        public SerializedObject serializedObject;

        internal PickerCreator()
        {
            EditorApplication.delayCall += DelayCall;
        }

        private void DelayCall()
        {
            if (componentTreePicker != null)
                return;

            componentTreePicker = CreateInstance<VariantComponentTreePicker>();

            //position the window below the button
            var pos = new Rect(pickerPosition.x, pickerPosition.yMax, 0, 0);

            //If the window gets out of the screen, we place it on top of the button instead
            if (pickerPosition.yMax > Screen.currentResolution.height / 2)
                pos.y = pickerPosition.y - Screen.currentResolution.height / 2;

            //We show a drop down window which is automatically destroyed when focus is lost
            componentTreePicker.ShowAsDropDown(pos,
                new Vector2(pickerPosition.width >= 250 ? pickerPosition.width : 250,
                    Screen.currentResolution.height / 2));

            componentTreePicker.serializableVariantComponentReference = variantComponentReference;
            componentTreePicker.serializedObject = serializedObject;

            componentTreePicker.treeView.AssignDefaults();
            componentTreePicker.treeView.SetRootItem("Entity States");
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    assembly.GetTypes()
                        .Where(type => type.IsSubclassOf(typeof(VariantComponent)) && !type.IsAbstract)
                        .ToList()
                        .ForEach(type => componentTreePicker.treeView.PopulateItem(type));
                }
                catch { }
            }
        }
    }

}

