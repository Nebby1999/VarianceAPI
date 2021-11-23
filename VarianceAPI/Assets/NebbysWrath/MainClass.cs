using BepInEx;
using BepInEx.Logging;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using VarianceAPI.Utils;
using Path = System.IO.Path;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
namespace NebbysWrath
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.Nebby.VarianceAPI", BepInDependency.DependencyFlags.HardDependency)]
    public class MainClass : BaseUnityPlugin
    {
        public static AssetBundle nebbysWrathAssets = null;
        internal static string assetBundleName = "/NebbysWrathAssets";

        public const string GUID = "com.Nebby.NebbysWrath";
        public const string NAME = "VP - Nebby's Wrath";
        public const string VERSION = "1.1.1";

        public static PluginInfo pluginInfo;
        public static ManualLogSource logger;
        public static Material[] ihatehopooshaders;

        public void Awake()
        {
            pluginInfo = Info;
            logger = Logger;

            LoadAssets();

            var ingameMaterials = Resources.FindObjectsOfTypeAll<Material>();
            SwapShaders(nebbysWrathAssets, ingameMaterials);

            LoadEffects();

            new DamageTypes.DamageTypes().Init();
            new Projectiles.Projectiles().Init();

            MaterialGrabber.CreateCorrectMaterials();
            VarianceAPI.VariantRegister.AddVariant(nebbysWrathAssets, Config);
            ContentPackProvider.Initialize();
        }

        public void LoadAssets()
        {
            var path = Path.GetDirectoryName(Info.Location);
            nebbysWrathAssets = AssetBundle.LoadFromFile(path + assetBundleName);
            ContentPackProvider.serializedContentPack = nebbysWrathAssets.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
        }
        public void SwapShaders(AssetBundle assetBundle, Material[] gameMaterials)
        {
            List<Material> swappedMaterials = new List<Material>();

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
                swappedMaterials.Add(material);
                assetBundleMaterials[i] = material;
            }
            ihatehopooshaders = swappedMaterials.ToArray();
        }

        public void LoadEffects()
        {
            var Effects = nebbysWrathAssets.LoadAllAssets<GameObject>().Where(gameObject => gameObject.GetComponent<EffectComponent>());
            foreach (GameObject go in Effects)
            {
                HG.ArrayUtils.ArrayAppend(ref ContentPackProvider.serializedContentPack.effectDefs, new EffectDef(go));
            }
        }

        public void RegisterContentPack()
        {
            ContentPackProvider.Initialize();
        }
    }
    public class ContentPackProvider : IContentPackProvider
    {
        public static SerializableContentPack serializedContentPack;
        public static ContentPack contentPack;
        //Should be the same names as your SerializableContentPack in the asset bundle
        public static string contentPackName = "NebbysWrathContent";

        public string identifier
        {
            get
            {
                //If I see this name while loading a mod I will make fun of you
                return "NebbysWrath";
            }
        }

        internal static void Initialize()
        {
            contentPack = serializedContentPack.CreateContentPack();
            ContentManager.collectContentPackProviders += AddCustomContent;
        }

        private static void AddCustomContent(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new ContentPackProvider());
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
