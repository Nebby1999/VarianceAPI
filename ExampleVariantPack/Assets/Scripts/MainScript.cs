using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using VarianceAPI;
using VarianceAPI.Modules;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using System.IO;
using RoR2.ContentManagement;
using ExampleVariantPack.ThunderkitMadeVariants;
using ExampleVariantPack.CodedMadeVariants;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
namespace ExampleVariantPack
{
    [BepInPlugin("com.Nebby.ExampleVariantPack", "ExampleVariantPack", "0.0.2")]
	[BepInDependency("com.Nebby.VarianceAPI", BepInDependency.DependencyFlags.HardDependency)]
    public class MainScript : BaseUnityPlugin
    {
        public static MainScript instance;
        public static AssetBundle exampleVariancePackAssets = null;
        internal static string assetBundleName = "ExampleVariantPackAssets";

        public void Awake()
        {
            instance = this;
			LoadAssetsAndRegisterContentPack();
			Init();

		}

        public void Init()
        {
			var VR = new VariantRegister();
			VR.RegisterConfigs(exampleVariancePackAssets, Config);
			var ECV = new ExampleCodedVariant();
			ECV.Init(Config);
        }

        public void LoadAssetsAndRegisterContentPack()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			exampleVariancePackAssets = AssetBundle.LoadFromFile(Path.Combine(path, assetBundleName));
			ContentPackProvider.serializedContentPack = exampleVariancePackAssets.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
		}
	}
	public class ContentPackProvider : IContentPackProvider
	{
		public static SerializableContentPack serializedContentPack;
		public static ContentPack contentPack;
		//Should be the same names as your SerializableContentPack in the asset bundle
		public static string contentPackName = "ExampleVariantPackAssets";

		public string identifier
		{
			get
			{
				//If I see this name while loading a mod I will make fun of you
				return "ExampleVariantPack";
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