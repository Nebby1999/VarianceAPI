using Path = System.IO.Path;
using RoR2;
using UnityEngine;
using System.IO;
using System;

namespace VarianceAPI
{
    public static class Assets
    {
        public static string assemblyPath
        {
            get
            {
                return Path.GetDirectoryName(MainClass.PluginInfo.Location);
            }
        }

        private const string VAPIAssetsName = "VAPIAssets";

        public static AssetBundle VAPIAssets { get; private set; }

        internal static void Initialize()
        {
            VAPIAssets = AssetBundle.LoadFromFile(assemblyPath + VAPIAssetsName);

            var GameMaterials = Resources.FindObjectsOfTypeAll<Material>();
            MapMaterials(VAPIAssets.LoadAllAssets<Material>(), GameMaterials);
        }

        private static void MapMaterials(Material[] materials, Material[] gameMaterials)
        {
            throw new NotImplementedException();
        }
    }
}
