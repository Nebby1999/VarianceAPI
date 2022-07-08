using RoR2.ContentManagement;
using UnityEngine;
using VarianceAPI.Utils;
using Path = System.IO.Path;

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
            MapMaterials(VAPIAssets, GameMaterials);
        }

        public static void MapMaterials(AssetBundle assetBundle, Material[] gameMaterials)
        {
            if (assetBundle.isStreamedSceneAssetBundle)
                return;

            var cloudMat = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningStrikeOrbEffect").transform.Find("Ring").GetComponent<ParticleSystemRenderer>().material;

            Material[] assetBundleMaterials = assetBundle.LoadAllAssets<Material>();

            for (int i = 0; i < assetBundleMaterials.Length; i++)
            {
                var material = assetBundleMaterials[i];
                // If it's stubbed, just switch out the shader unless it's fucking cloudremap
                if (material.shader.name.StartsWith("StubbedShader"))
                {
                    material.shader = Resources.Load<Shader>("shaders" + material.shader.name.Substring(13));
                    if (material.shader.name.Contains("Cloud Remap"))
                    {
                        var eatShit = new RuntimeCloudMaterialMapper(material);
                        material.CopyPropertiesFromMaterial(cloudMat);
                        eatShit.SetMaterialValues(ref material);
                    }
                }

                //If it's this shader it searches for a material with the same name and copies the properties
                if (material.shader.name.Equals("CopyFromRoR2"))
                {
                    foreach (var gameMaterial in gameMaterials)
                        if (material.name.Equals(gameMaterial.name))
                        {
                            material.shader = gameMaterial.shader;
                            material.CopyPropertiesFromMaterial(gameMaterial);
                            break;
                        }
                }
                assetBundleMaterials[i] = material;
            }
        }
    }
}
