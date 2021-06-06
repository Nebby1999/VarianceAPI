using BepInEx;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using VarianceAPI.Modules;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
namespace VarianceAPI
{
	[BepInPlugin("com.Nebby.VarianceAPI", "VarianceAPI", "0.4.0")]
	internal class MainClass : BaseUnityPlugin
    {
        public static MainClass instance;
        public static AssetBundle varianceAPIAssets = null;
        internal static string assetBundleName = "VarianceAPIAssets";

        internal void Awake()
        {
            instance = this;
			ConfigLoader.SetupConfigLoader(Config);
            //LoadAssetsAndRegisterContentPack();
        }
        internal void LoadAssetsAndRegisterContentPack()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            varianceAPIAssets = AssetBundle.LoadFromFile(Path.Combine(path, assetBundleName));
            ContentPackProvider.serializedContentPack = varianceAPIAssets.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
        }
	}
	public class ContentPackProvider : IContentPackProvider
	{
		public static SerializableContentPack serializedContentPack;
		public static ContentPack contentPack;
		//Should be the same names as your SerializableContentPack in the asset bundle
		public static string contentPackName = "VarianceAPIAssets";

		public string identifier
		{
			get
			{
				//If I see this name while loading a mod I will make fun of you
				return "VarianceAPI";
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