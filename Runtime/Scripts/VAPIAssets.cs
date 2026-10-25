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
    /// <summary>
    /// VarianceAPI's assets class
    /// </summary>
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
        
        /// <summary>
        /// Resource availability that becomes available when VAPI initializes this class
        /// </summary>
        public static ResourceAvailability assetsAvailability;

        private static AssetBundle? _assetBundle;

        /// <summary>
        /// Loads an Asset from VAPI
        /// </summary>
        /// <typeparam name="TAsset">The Asset's type</typeparam>
        /// <param name="name">The name of the Asset</param>
        /// <returns>The loaded asset, which may be null if it doesnt exist</returns>
        public static TAsset? LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            ThrowIfUnavailable();
            return _assetBundle!.LoadAsset<TAsset>(name);
        }

        /// <summary>
        /// Loads an Asset from VAPI asynchronously.
        /// <br></br>
        /// Once you retrieve the asset you should dispose of the request by calling <see cref="VAPIAssetRequest{TAsset}.Dispose"/>
        /// </summary>
        /// <typeparam name="TAsset">The type of asset</typeparam>
        /// <param name="name">The name of the asset</param>
        /// <returns>A <see cref="VAPIAssetRequest{TAsset}"/> which can be awaited</returns>
        public static VAPIAssetRequest<TAsset> LoadAssetAsync<TAsset>(string name) where TAsset : UnityEngine.Object
        {
            ThrowIfUnavailable();
            return VAPIAssetRequest<TAsset>.RequestFromPool(_assetBundle!, name);
        }

        /// <summary>
        /// Loads all assets of type <typeparamref name="TAsset"/>
        /// </summary>
        /// <typeparam name="TAsset">The type of assets to load</typeparam>
        /// <returns>All assets of type <typeparamref name="TAsset"/>. The array may be null if VAPI has no assets of type <typeparamref name="TAsset"/></returns>
        public static TAsset[]? LoadAssets<TAsset>() where TAsset : UnityEngine.Object
        {
            ThrowIfUnavailable();
            return _assetBundle!.LoadAllAssets<TAsset>();
        }

        /// <summary>
        /// Loads all assets of type <typeparamref name="TAsset"/> asynchronously
        /// <br></br>
        /// Once you retrieve the assets you should dispose of the request by calling <see cref="VAPIAssetRequest{TAsset}.Dispose"/>
        /// </summary>
        /// <typeparam name="TAsset">The type of assets to load</typeparam>
        /// <returns>A VAPIAssetRequest which can be awaited</returns>
        public static VAPIAssetRequest<TAsset> LoadAllAssetsAsync<TAsset>() where TAsset : UnityEngine.Object
        {
            ThrowIfUnavailable();
            return VAPIAssetRequest<TAsset>.RequestFromPool(_assetBundle!);
        }

        internal static IEnumerator Initialize()
        {
            //Don't load assets twice
            if (assetsAvailability.available)
                yield break;

            VAPILog.Info($"Initializing Assets...");

            var loadRoutine = LoadAssetBundle();
            while (loadRoutine.MoveNext())
            {
                yield return null;
            }


            var coroutine = ParallelCoroutine.RequestFromPool();

            coroutine.Add(SwapShaders());
            coroutine.Add(SwapAddressableShaders()); //Setup materials

            while (coroutine.MoveNext())
                yield return null;

            ParallelCoroutine.ReturnToPool(coroutine);

            assetsAvailability.MakeAvailable(); //Make available
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

    /// <summary>
    /// An abstract class that represents an Async request for a VarianceAPI Asset.
    /// <br></br>
    /// Look at <see cref="VAPIAssetRequest{TAsset}"/> for a generic version
    /// </summary>
    public abstract class VAPIAssetRequest : IEnumerator
    {
        /// <summary>
        /// The loaded asset, boxed in a <see cref="UnityEngine.Object"/>
        /// </summary>
        public abstract UObject? boxedAsset { get; }
        /// <summary>
        /// The Loaded assets, boxed in an array of <see cref="UnityEngine.Object"/>
        /// </summary>
        public abstract UObject[]? boxedAssets { get; }

        /// <summary>
        /// Returns true if the AssetRequest has finished loading
        /// </summary>
        public bool isComplete
        {
            get
            {
                internalCoroutine ??= LoadAsset();

                return !internalCoroutine.MoveNext();
            }
        }

        object? IEnumerator.Current => internalCoroutine?.Current;

        private IEnumerator? internalCoroutine;

        /// <summary>
        /// Implement the asset loading here
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator LoadAsset();

        bool IEnumerator.MoveNext()
        {
            return isComplete == false;
        }

        /// <summary>
        /// Resets the asset request, you should not call this yourself.
        /// </summary>
        public virtual void Reset()
        {
            internalCoroutine = null;
        }
    }

    /// <summary>
    /// A class that represents an async asset request from VarianceAPI's assetbundle.
    /// <para></para>
    /// This class is pooled internally, to return the request to the pool, call <see cref="Dispose"/>.
    /// </summary>
    /// <typeparam name="TAsset">The type of asset being loaded</typeparam>
    public sealed class VAPIAssetRequest<TAsset> : VAPIAssetRequest, IDisposable where TAsset : UObject
    {
        #region Pool implementation
        private static Stack<VAPIAssetRequest<TAsset>> _stack = new Stack<VAPIAssetRequest<TAsset>>();

        internal static VAPIAssetRequest<TAsset> RequestFromPool(AssetBundle assetBundle)
        {
            var request = GetExistingOrCreateRequest();
            request.Init(assetBundle);
            return request;
        }
        internal static VAPIAssetRequest<TAsset> RequestFromPool(AssetBundle assetBundle, string name)
        {
            var request = GetExistingOrCreateRequest();
            request.Init(assetBundle, name);
            return request;
        }

        internal static void ReturnToPool(VAPIAssetRequest<TAsset> request)
        {
            _stack.Push(request);
        }

        private static VAPIAssetRequest<TAsset> GetExistingOrCreateRequest()
        {
            if(_stack.TryPop(out var assetRequest))
            {
                return assetRequest;
            }
            return new VAPIAssetRequest<TAsset>();
        }
        #endregion

        public override UObject? boxedAsset => asset;

        /// <summary>
        /// The loaded asset
        /// </summary>
        public TAsset? asset => _asset;
        private TAsset? _asset;

        public override UObject[]? boxedAssets => assets;
        /// <summary>
        /// The array of loaded assets
        /// </summary>
        public TAsset[]? assets => _assets;
        private TAsset[]? _assets;

        private bool isSingleAssetLoad => !string.IsNullOrWhiteSpace(_assetName);
        private string? _assetName;
        private AssetBundle? _assetBundle;
        private Type assetType => typeof(TAsset);

        protected override IEnumerator LoadAsset()
        {
            ThrowIfUninitialized();
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
                _assets = new TAsset[request.allAssets.Length];
                for(int i = 0; i < request.allAssets.Length; i++)
                {
                    _assets[i] = (TAsset)request.allAssets[i];
                }

#if DEBUG
                if (_assets == null || _assets.Length == 0)
                {
                    VAPILog.Warning($"No Assets of type {typeof(TAsset).Name} exists within the VAPI Asset Bundle.");
                }
#endif
            }
        }

        /// <summary>
        /// Disposes this AssetRequest and returns it to the pool
        /// </summary>
        public void Dispose()
        {
            ThrowIfUninitialized();
            Reset();
            ReturnToPool(this);
        }

        public override void Reset()
        {
            ThrowIfUninitialized();
            base.Reset();

            _assetBundle = null;
            _assetName = null;

            _assets = null;
            _asset = null;
        }

        private void ThrowIfUninitialized()
        {
            if (_assetBundle == null || (_assetBundle == null && _assetName == null))
                throw new InvalidOperationException("You should not use a VAPIAssetRequest that has been disposed of.");
        }

        private void Init(AssetBundle assetBundle)
        {
            _assetBundle = assetBundle;
        }

        private void Init(AssetBundle assetBundle, string assetName)
        {
            _assetBundle = assetBundle;
            _assetName = assetName;
        }

        private VAPIAssetRequest() { }
    }
}