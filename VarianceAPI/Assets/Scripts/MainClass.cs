using BepInEx;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using VarianceAPI.Modules;
using Path = System.IO.Path;

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
            LoadAssetsAndRegisterContentPack();
        }
        internal void LoadAssetsAndRegisterContentPack()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            varianceAPIAssets = AssetBundle.LoadFromFile(Path.Combine(path, assetBundleName));
			FinishArtifactOfVariance();
			ContentPackProvider.serializedContentPack = varianceAPIAssets.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
			ContentPackProvider.Initialize();
        }
		internal void FinishArtifactOfVariance()
        {
			var VarianceDef = varianceAPIAssets.LoadAsset<ArtifactDef>("VarianceDef");
			var VarianceContent = varianceAPIAssets.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
			if(ConfigLoader.EnableArtifactOfVariance.Value)
            {
				Debug.Log("VarianceAPI: Registering Artifact Of Variance...");
				ArtifactDef[] ArtifactDefs = new ArtifactDef[] { VarianceDef };
				VarianceDef.descriptionToken = "All Variant's Spawn Rates are Multiplied by " + ConfigLoader.VarianceMultiplier.Value;
				VarianceContent.artifactDefs = ArtifactDefs;
				if(VarianceContent.artifactDefs != null)
                {
					Debug.Log("VarianceAPI: Succesfully added " + VarianceDef.nameToken + " to " + ContentPackProvider.contentPackName);
                }
            }
			else
            {
				return;
            }
        }
	}
	public class ContentPackProvider : IContentPackProvider
	{
		public static SerializableContentPack serializedContentPack;
		public static ContentPack contentPack;
		//Should be the same names as your SerializableContentPack in the asset bundle
		public static string contentPackName = "VarianceAPIContent";

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
			Debug.Log("VarianceAPI: Registering VarianceAPIContent...");
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