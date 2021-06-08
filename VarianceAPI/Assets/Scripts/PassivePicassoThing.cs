using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VarianceAPI
{
    public class PassivePicassoThing
    {
        static int updateWait = 10;
        [InitializeOnLoadMethod]
        public static void InitializeProject()
        {
            updateWait = 10;
            EditorApplication.update += InstallEditorPack;
        }

        private static void InstallEditorPack()
        {
            if (--updateWait > 0) return;
            Debug.Log("Configuring project for VarianceAPI");
            EditorApplication.update -= InstallEditorPack;

            var varianceAPIConfigured = AssetDatabase.IsValidFolder("Assets/VarianceAPI");
            if (varianceAPIConfigured) return;

            var editorPack = AssetDatabase.FindAssets("BoilerplatePackage", new[] { "Packages" }).Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();
            if (editorPack.Length > 0)
            {
                var assetPath = editorPack[0];
                var pwd = Directory.GetCurrentDirectory();
                var finalPath = Path.Combine(pwd, assetPath);
                var fullPath = Path.GetFullPath(finalPath);
                Debug.Log(fullPath);
                System.Diagnostics.Process.Start(fullPath);
            }
        }
    }
}