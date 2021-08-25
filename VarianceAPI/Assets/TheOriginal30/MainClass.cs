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
using EntityStates;
using RoR2;
using BepInEx.Configuration;
using Path = System.IO.Path;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
namespace TheOriginal30
{
	[BepInPlugin("com.Nebby.TheOriginal30", "VP - The Original 30", "1.1.7")]
	[BepInDependency("com.Nebby.VarianceAPI", BepInDependency.DependencyFlags.HardDependency)]
	public class MainClass : BaseUnityPlugin
	{
		public static MainClass instance;
		public static AssetBundle theOriginal30Assets = null;
		internal static string assetBundleName = "TheOriginal30Assets";

		internal static GameObject missileLauncherDisplayPrefab; // gotta cache this for lemurians

		public void Awake()
		{
			instance = this;
			GrabMaterials();
			LoadAssets();
			FixMaterials();
			RegisterContentPack();
			/*GrabVanillaMaterials();
			RegisterVariants();*/
		}
		private void GrabMaterials()
		{
			ItemDisplayRuleSet IDRS = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;
			missileLauncherDisplayPrefab = IDRS.FindDisplayRuleGroup(RoR2Content.Equipment.CommandMissile).rules[0].followerPrefab;
		}
		private void LoadAssets()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			theOriginal30Assets = AssetBundle.LoadFromFile(Path.Combine(path, assetBundleName));
		}
		private void FixMaterials()
        {
			var Materials = theOriginal30Assets.LoadAllAssets<Material>();
			foreach (Material material in Materials)
            {
				if(material.shader.name.StartsWith("StubbedShader"))
                {
					material.shader = Resources.Load<Shader>("shaders" + material.shader.name.Substring(13));
                }
            }
        }
        public void RegisterContentPack()
        {
            ContentPackProvider.serializedContentPack = theOriginal30Assets.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
			ContentPackProvider.Initialize();
        }
		/*public void GrabVanillaMaterials()
        {
			var MG = new MaterialGrabber();
			MG.StartGrabber(theOriginal30Assets);
        }
		public void RegisterVariants()
        {
			var VR = new VariantRegister();
			VR.RegisterConfigs(theOriginal30Assets, Config);
        }*/
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