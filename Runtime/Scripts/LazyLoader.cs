#nullable enable

namespace VAPI
{
    /// <summary>
    /// A Struct that lazily loads an asset of type <see cref="T"/> from within VarianceAPI's Asset Bundle.
    /// <br></br>
    /// Only interact with these structs if <see cref="VAPIAssets"/> has initialized, otherwise an exception will be thrown when you try to load the asset.
    /// </summary>
    /// <typeparam name="T">The type of asset to load</typeparam>
    public struct LazyLoader<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// The asset, it's loaded on demand if its unloaded
        /// </summary>
        public T? asset
        {
            get
            {
                if(_asset == null)
                {
                    LoadAsset();
                }
                return _asset;
            }
        }
        private T? _asset;
        private string _assetName;

        private void LoadAsset()
        {
            _asset = VAPIAssets.LoadAsset<T>(_assetName);
        }

        /// <summary>
        /// Explicitly converts this into <see cref="T"/>
        /// <br></br>
        /// May be null if the asset doesnt exist
        /// </summary>
        /// <param name="lazyLoader"></param>
        public static explicit operator T?(LazyLoader<T> lazyLoader)
        {
            return lazyLoader.asset;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assetName">The name of the asset to load</param>
        public LazyLoader(string assetName)
        {
            _asset = null;
            _assetName = assetName;
        }
    }
}