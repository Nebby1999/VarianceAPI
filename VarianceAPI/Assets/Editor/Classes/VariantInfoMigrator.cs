/*using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VarianceAPI.Scriptables;

[CustomEditor(typeof(VarianceAPI.Scriptables.VariantInfo))]
public class VariantInfoMigrator : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10);
        VarianceAPI.Scriptables.VariantInfo variantInfo = (VariantInfo)target;

        if(GUILayout.Button("Migrate to new VariantInfo"))
        {
            VariantInfoCreator.CreateNerVariantInfo(variantInfo);
        }
    }
}*/
