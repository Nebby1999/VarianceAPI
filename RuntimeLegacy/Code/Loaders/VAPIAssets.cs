using MSU;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VAPI.Legacy
{
    public static class VAPIAssets
    {
        private const string ASSET_BUNDLE_NAME = "vapiassets";
        private const string ASSET_BUNDLE_FOLDER_NAME = "assetbundles";

        private static string assetBundleFolderPath => Path.Combine(Path.GetDirectoryName(VAPIMain.instance.Info.Location), ASSET_BUNDLE_FOLDER_NAME);

        public static ResourceAvailability assetsAvailability;

        internal static SkillDef _emptySkillDef;

        private static AssetBundle _assetBundle;

        public static TAsset LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            return _assetBundle.LoadAsset<TAsset>(name);
        }

        public static VAPIAssetRequest<TAsset> LoadAssetAsync<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            return new VAPIAssetRequest<TAsset>(_assetBundle.LoadAssetAsync(name));
        }

        public static TAsset[] LoadAssets<TAsset>() where TAsset : UnityEngine.Object
        {
            return _assetBundle.LoadAllAssets<TAsset>();
        }

        public static VAPIAssetRequest<TAsset> LoadAssetsAsync<TAsset>() where TAsset : UnityEngine.Object
        {
            return new VAPIAssetRequest<TAsset>(_assetBundle.LoadAllAssetsAsync<TAsset>());
        }

        internal static IEnumerator Initialize()
        {
            if (assetsAvailability.available)
                yield break;

            VAPILog.Info($"Initializing Assets...");

            var loadRoutine = LoadAssetBundle();
            while (!loadRoutine.IsDone())
            {
                yield return null;
            }

            ParallelCoroutine coroutine = new ParallelCoroutine();

            coroutine.Add(SwapShaders());
            coroutine.Add(SwapAddressableShaders());

            while (!coroutine.isDone)
                yield return null;

            assetsAvailability.MakeAvailable();
        }

        private static IEnumerator LoadAssetBundle()
        {
            var request = AssetBundle.LoadFromFileAsync(Path.Combine(assetBundleFolderPath, ASSET_BUNDLE_NAME));

            while (!request.isDone)
                yield return null;

            _assetBundle = request.assetBundle;
        }

        private static IEnumerator SwapShaders()
        {
            return ShaderUtil.SwapStubbedShadersAsync(_assetBundle);
        }

        private static IEnumerator SwapAddressableShaders()
        {
            return ShaderUtil.LoadAddressableMaterialShadersAsync(_assetBundle);
        }
    }

    public class VAPIAssetRequest<TAsset> : IEnumerator where TAsset : UnityEngine.Object
    {
        public TAsset asset => (TAsset)_request.asset;
        public TAsset[] assets => _request.allAssets.OfType<TAsset>().ToArray();
        public bool isDone => _request.isDone;
        public float progress => _request.progress;

        object IEnumerator.Current => _request.asset;

        private AssetBundleRequest _request;
        internal VAPIAssetRequest(AssetBundleRequest request)
        {
            _request = request;
        }

        bool IEnumerator.MoveNext()
        {
            return !_request.isDone;
        }

        void IEnumerator.Reset()
        {
            throw new System.NotSupportedException();
        }
    }
}