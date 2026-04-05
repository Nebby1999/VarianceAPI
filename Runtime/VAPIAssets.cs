#nullable enable
using MSU;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace VAPI
{
    public static class VAPIAssets
    {
        private const string ASSET_BUNDLE_NAME = "vapiassets";
        private const string ASSET_BUNDLE_FOLDER_NAME = "assetbundles";

        private static string assetBundleFolderPath
        {
            get
            {
                VAPIMain.ThrowIfUninitialized();
                return Path.Combine(Path.GetDirectoryName(VAPIMain.instance!.Info.Location), ASSET_BUNDLE_FOLDER_NAME);
            }
        }
        

        public static ResourceAvailability assetsAvailability;

        private static AssetBundle? _assetBundle;

        public static TAsset LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            ThrowIfUnavailable();
            return _assetBundle!.LoadAsset<TAsset>(name);
        }

        public static VAPIAssetRequest<TAsset> LoadAssetAsync<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            ThrowIfUnavailable();
            return new VAPIAssetRequest<TAsset>(_assetBundle!, name);
        }

        public static TAsset[] LoadAssets<TAsset>() where TAsset : UnityEngine.Object
        {
            ThrowIfUnavailable();
            return _assetBundle!.LoadAllAssets<TAsset>();
        }

        public static VAPIAssetRequest<TAsset> LoadAssetsAsync<TAsset>() where TAsset : UnityEngine.Object
        {
            ThrowIfUnavailable();
            return new VAPIAssetRequest<TAsset>(_assetBundle!);
        }

        internal static IEnumerator Initialize()
        {
            if (assetsAvailability.available)
                yield break;

            VAPILog.Info($"Initializing Assets...");

            var loadRoutine = LoadAssetBundle();
            while (loadRoutine.MoveNext())
            {
                yield return null;
            }

            HG.Coroutines.ParallelCoroutine coroutine = new HG.Coroutines.ParallelCoroutine();

            coroutine.Add(SwapShaders());
            coroutine.Add(SwapAddressableShaders());

            while (coroutine.MoveNext())
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
            return ShaderUtil.SwapStubbedShadersAsync(_assetBundle!);
        }

        private static IEnumerator SwapAddressableShaders()
        {
            return ShaderUtil.LoadAddressableMaterialShadersAsync(_assetBundle!);
        }

        private static void ThrowIfUnavailable()
        {
            if (!assetsAvailability.available)
            {
                throw new InvalidOperationException("Cannot call VAPIAssets methods before VAPIMain intializes it! consider subscribing to it's assetsAvailability.");
            }
        }
    }

    public abstract class VAPIAssetRequest
    {
        public abstract UObject? boxedAsset { get; }
        public abstract IEnumerable<UObject>? boxedAssets { get; }
        public bool IsComplete
        {
            get
            {
                internalCoroutine ??= LoadAsset();

                return !internalCoroutine.MoveNext();
            }
        }
        private IEnumerator? internalCoroutine;

        protected abstract IEnumerator LoadAsset();
    }

    public class VAPIAssetRequest<TAsset> : VAPIAssetRequest where TAsset : UObject
    {
        public override UObject? boxedAsset => asset;
        public TAsset? asset => _asset;
        private TAsset? _asset;

        public override IEnumerable<UObject>? boxedAssets => assets;
        public IEnumerable<TAsset>? assets => _assets;
        private TAsset[]? _assets;

        private bool isSingleAssetLoad => !string.IsNullOrWhiteSpace(_assetName);
        private string? _assetName;
        private AssetBundle _assetBundle;

        protected override IEnumerator LoadAsset()
        {
            AssetBundleRequest? request = null;
            if (isSingleAssetLoad)
            {
                request = _assetBundle.LoadAssetAsync<TAsset>(_assetName);
            }
            else
            {
                request = _assetBundle.LoadAllAssetsAsync<TAsset>();
            }

            while (!request.isDone)
                yield return null;

            if (isSingleAssetLoad)
            {
                _asset = (TAsset)request.asset;
#if DEBUG
                if (!_asset)
                {
                    VAPILog.Warning($"Asset of type {typeof(TAsset).Name} with name {_assetName} was not found.");
                }
#endif
            }
            else
            {
                _assets = (TAsset[])request.allAssets;

#if DEBUG
                if (_assets == null || _assets.Length == 0)
                {
                    VAPILog.Warning($"No Assets of type {typeof(TAsset).Name} exists within the VAPI Asset Bundle.");
                }
#endif
            }
        }

        internal VAPIAssetRequest(AssetBundle assetBundle, string assetName)
        {
            _assetName = assetName;
            _assetBundle = assetBundle;
        }

        internal VAPIAssetRequest(AssetBundle assetBundle)
        {
            _assetBundle = assetBundle;
        }
    }
}