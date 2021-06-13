using BepInEx;
using BepInEx.Logging;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using R2API;
using R2API.Utils;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using VarianceAPI.Modules;
using VarianceAPI.Modules.Items.ItemBases;
using Path = System.IO.Path;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
namespace VarianceAPI
{
	[BepInPlugin("com.Nebby.VarianceAPI", "VarianceAPI", "0.5.0")]
	[R2APISubmoduleDependency(nameof(ItemAPI), nameof(PrefabAPI), nameof(ProjectileAPI))]
	internal class MainClass : BaseUnityPlugin
    {
        public static MainClass instance;
		public static ManualLogSource Log;
        public static AssetBundle varianceAPIAssets = null;
        internal static string assetBundleName = "VarianceAPIAssets";

		public List<R2API_ItemBase> R2APIItems = new List<R2API_ItemBase>();
		public List<Thunderkit_ItemBase> Items = new List<Thunderkit_ItemBase>();

        internal void Awake()
        {
			Log = Logger;
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
			Log.LogMessage("Adding VarianceAPI's Intrinsic Items...");
			FinishIntrinsicItems();
        }
		internal void FinishArtifactOfVariance()
        {
			var VarianceDef = varianceAPIAssets.LoadAsset<ArtifactDef>("VarianceDef");
			var VarianceContent = varianceAPIAssets.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
			if(ConfigLoader.EnableArtifactOfVariance.Value)
            {
				Log.LogMessage("Adding the Artifact of Variance...");
				ArtifactDef[] ArtifactDefs = new ArtifactDef[] { VarianceDef };
				VarianceDef.descriptionToken = "All Variant's Spawn Rates are Multiplied by " + ConfigLoader.VarianceMultiplier.Value;
				VarianceContent.artifactDefs = ArtifactDefs;
				if(VarianceContent.artifactDefs != null)
                {
					Logger.LogMessage("Succesfully added " + VarianceDef.nameToken + " to " + ContentPackProvider.contentPackName);
                }
            }
			else
            {
				return;
            }
        }
		internal void FinishIntrinsicItems()
        {
			var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Thunderkit_ItemBase)));

			foreach (var itemType in ItemTypes)
            {
				Thunderkit_ItemBase item = (Thunderkit_ItemBase)System.Activator.CreateInstance(itemType);
				item.Init();
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
			MainClass.Log.LogMessage("Registering VarianceAPIContent...");
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