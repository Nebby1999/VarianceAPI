using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VAPI.EditorUtils
{
    public static class Constants
    {
        public const string VarianceAPI = nameof(VarianceAPI);
        public const string AssetFolderPath = "Assets/VarianceAPI";
        public const string PackageFolderPath = "Packages/nebby-varianceapi";
        public const string PackageName = "nebby-varianceapi";

        public const string MSUContextRoot = "Assets/Create/VarianceAPI/";
        public const string MSUScriptableRoot = "Assets/VarianceAPI/";
        public const string MSUMenuRoot = "Tools/VarianceAPI/";
        public const string MSUSettingsRoot = "Assets/ThunderkitSettings/VarianceAPI/";

        private const string xmlDocGUID = "ded440f4e5e23cd4a8bbfb38e5f13ebf";
        private const string vapiIconGUID = "17e7291e3ba22bf44987a915ddf8eed0";

        /// <summary>
        /// Loads the XMLDoc of RoR2EditorKit
        /// </summary>
        public static TextAsset XMLDoc => Load<TextAsset>(xmlDocGUID);

        public static Texture VAPIIconGUID => Load<Texture>(vapiIconGUID);

        private static T Load<T>(string guid) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }

        public static class FolderPaths
        {
            private const string assets = "Assets";
            private const string lib = "Library";
            private const string scriptAssemblies = "ScriptAssemblies";
            public static string LibraryFolder
            {
                get
                {
                    var assetsPath = Application.dataPath;
                    var libFolder = assetsPath.Replace(assets, lib);
                    return libFolder;
                }
            }

            public static string ScriptAssembliesFolder
            {
                get
                {
                    return Path.Combine(LibraryFolder, scriptAssemblies);
                }
            }
        }
    }
}