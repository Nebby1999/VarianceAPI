#if !MACKYSOFT_INSTALLED
using UnityEditor;
using UnityEditor.PackageManager;

namespace VAPI.Editor.Importers
{
    internal static class AddMackysoftIfMissing
    {
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            EditorUtility.DisplayDialog("Mackysoft SerializeReferenceExtensions is Missing", "Mackysoft's SerializeReferenceExtensions is missing, this package is required for VarianceAPI's editor capabilities to work properly. The editor will now install version 2.1.0 of Mackysoft's SerializeReferenceExtensions.", "Ok!");

            try
            {
                EditorApplication.LockReloadAssemblies();
                string url = $"https://github.com/mackysoft/Unity-SerializeReferenceExtensions.git?path=Assets/MackySoft/MackySoft.SerializeReferenceExtensions#2.1.0";
                var request = Client.Add(url);
                EditorUtility.DisplayProgressBar("Installing Mackysoft SerializeReferenceExtensions", "Installing the package, please wait...", 0);
                while(!request.IsCompleted)
                {
                }
                EditorUtility.ClearProgressBar();
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }
        }
    }
}
#endif