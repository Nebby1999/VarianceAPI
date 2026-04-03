#nullable enable

namespace VAPI
{
    public struct LazyLoader<T> where T : UnityEngine.Object
    {
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

        public static explicit operator T?(LazyLoader<T> lazyLoader)
        {
            return lazyLoader.asset;
        }

        public LazyLoader(string assetName)
        {
            _asset = null;
            _assetName = assetName;
        }
    }
}