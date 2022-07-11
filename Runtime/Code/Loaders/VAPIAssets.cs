using Moonstorm.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VAPI
{
    public class VAPIAssets : AssetsLoader<VAPIAssets>
    {
        public string AssemblyDir => Path.GetDirectoryName(VAPIMain.Instance.Info.Location);
        public override AssetBundle MainAssetBundle => _assetBundle;
        private AssetBundle _assetBundle;

        internal void Init()
        {
            var bundlePath = Path.Combine(AssemblyDir, "assetbundles", "vapiassets");
            _assetBundle = AssetBundle.LoadFromFile(bundlePath);
        }

        internal void SwapShaders()
        {
            SwapShadersFromMaterials(MainAssetBundle.LoadAllAssets<Material>().Where(mat => mat.shader.name.StartsWith("Stubbed")));
        }
    }
}