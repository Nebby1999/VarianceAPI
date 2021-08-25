using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using System.Security;
using System.Security.Permissions;
using Path = System.IO.Path;
using System.Reflection;
using RoR2.ContentManagement;
using System.Linq;
using VarianceAPI.Modules.Prefabs;
using VarianceAPI.Modules.Items.ItemBases;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
namespace NebbysWrath
{
	[BepInPlugin("com.Nebby.NebbysWrath", "Nebby's Wrath", "1.0.0")]
	[BepInDependency("com.Nebby.VarianceAPI", BepInDependency.DependencyFlags.HardDependency)]
	public class MainClass : BaseUnityPlugin
	{
		public static MainClass instance;
		public static AssetBundle nebbysWrathAssets = null;
		internal static string assetBundleName = "NebbysWrathAssets";

		public void Awake()
		{
			instance = this;
			LoadAssets();
			SwapShaders();
			//GrabVanillaMaterials();
			CreateProjectiles();
			RegisterContentPack();
			FinishItems();
			//RegisterVariants();
		}

		public void LoadAssets()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			nebbysWrathAssets = AssetBundle.LoadFromFile(Path.Combine(path, assetBundleName));
		}
		private void SwapShaders()
		{
			var Materials = nebbysWrathAssets.LoadAllAssets<Material>();
			foreach (Material material in Materials)
			{
				if (material.shader.name.StartsWith("StubbedShader"))
				{
					material.shader = Resources.Load<Shader>("shaders" + material.shader.name.Substring(13));
				}
			}
		}
		/*private void GrabVanillaMaterials()
		{
			var MG = new MaterialGrabber();
			MG.StartGrabber(nebbysWrathAssets);
		}*/
		private void CreateProjectiles()
        {
			var Prefabs = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(PrefabBase)));
			foreach (var prefabType in Prefabs)
            {
				PrefabBase prefab = (PrefabBase)System.Activator.CreateInstance(prefabType);
				prefab.Init();
            }
        }
        public void RegisterContentPack()
        {
			ContentPackProvider.serializedContentPack = nebbysWrathAssets.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
			ContentPackProvider.Initialize();
        }
		public void FinishItems()
        {
			var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Thunderkit_ItemBase)));

			foreach (var itemType in ItemTypes)
            {
				Thunderkit_ItemBase item = (Thunderkit_ItemBase)System.Activator.CreateInstance(itemType);
				item.Init();
            }
        }
		/*public void RegisterVariants()
        {
			var VR = new VariantRegister();
			VR.RegisterConfigs(nebbysWrathAssets, Config);
        }*/
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
