using BepInEx;
using BepInEx.Logging;
using RoR2.ContentManagement;
using System.Collections;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using Path = System.IO.Path;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
namespace TheOriginal30
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.Nebby.VarianceAPI", BepInDependency.DependencyFlags.HardDependency)]
    public class MainClass : BaseUnityPlugin
    {
        public static AssetBundle theOriginal30Assets = null;
        internal static string assetBundleName = "/TheOriginal30Assets";

        public const string GUID = "com.Nebby.TheOriginal30";
        public const string NAME = "VP - The Original 30";
        public const string VERSION = "1.2.1";

        public static PluginInfo pluginInfo;
        public static ManualLogSource logger;

        public void Awake()
        {
            logger = Logger;
            pluginInfo = Info;
            LoadAssets();

            InitializeEntityStates();
            MaterialGrabber.CreateCorrectMaterials();
            VarianceAPI.VariantRegister.AddVariant(theOriginal30Assets, Config);
            ContentPackProvider.Initialize();
        }
        private void LoadAssets()
        {
            var path = Path.GetDirectoryName(Info.Location);
            theOriginal30Assets = AssetBundle.LoadFromFile(path + assetBundleName);
            ContentPackProvider.serializedContentPack = theOriginal30Assets.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
        }
        private void InitializeEntityStates()
        {
            GetType().Assembly.GetTypes()
                .Where(type => typeof(EntityStates.EntityState).IsAssignableFrom(type))
                .ToList()
                .ForEach(state =>
                {
                    HG.ArrayUtils.ArrayAppend(ref ContentPackProvider.serializedContentPack.entityStateTypes, new EntityStates.SerializableEntityStateType(state));
                });
        }
    }
    public class ContentPackProvider : IContentPackProvider
    {
        public static SerializableContentPack serializedContentPack;
        public static ContentPack contentPack;
        //Should be the same names as your SerializableContentPack in the asset bundle
        public static string contentPackName = "TheOriginal30Content";

        public string identifier
        {
            get
            {
                //If I see this name while loading a mod I will make fun of you
                return "TheOriginal30";
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