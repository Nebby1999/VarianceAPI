using Path = System.IO.Path;
using RoR2;
using UnityEngine;
using System.IO;
using System;
using RoR2.ContentManagement;

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

        private const string VAPIAssetsName = "/VAPIAssets";

        public static AssetBundle VAPIAssets { get; set; }

        internal static void Initialize()
        {
            VAPIAssets = AssetBundle.LoadFromFile(assemblyPath + VAPIAssetsName);
            ContentPacks.serializableContentPack = VAPIAssets.LoadAsset<SerializableContentPack>("VAPIContent");

            var GameMaterials = Resources.FindObjectsOfTypeAll<Material>();
            MapMaterials(VAPIAssets.LoadAllAssets<Material>(), GameMaterials);
        }

        private static void MapMaterials(Material[] materials, Material[] gameMaterials)
        {
        }
    }
}
