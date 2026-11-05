using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace VAPI.Editor
{
    [InitializeOnLoad]
    internal static class XMLDocCopier
    {
        private const string SRC_FILE = "VarianceAPI.Runtime.xml";
        private const string XML_DOC_GUID = "af11842c94fa99c44bde14d5d6456119";
        static XMLDocCopier()
        {
            EditorApplication.delayCall += CopyIfChanged;
        }

        private static void CopyIfChanged()
        {
            if(!File.Exists(SRC_FILE))
            {
                return;
            }

            var destPath = AssetDatabase.GUIDToAssetPath(XML_DOC_GUID);

            if(File.GetLastWriteTimeUtc(destPath) >= File.GetLastWriteTimeUtc(SRC_FILE))
            {
                return;
            }

            File.Copy(SRC_FILE, destPath, true);
            Debug.Log($"Updated VAPI XML Doc file.");
            AssetDatabase.Refresh();
        }
    }
}