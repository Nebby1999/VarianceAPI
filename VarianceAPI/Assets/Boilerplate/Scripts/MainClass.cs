using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using System.IO;
using System.Reflection;
using RoR2.ContentManagement;
using YourPackNameHere.Variants;


//Usage of Namespace greatly reccomended, make the root namespace Your pack's name for ease of development
namespace YourPackNameHere
{
	[BepInPlugin("com.YourUserName.YourPackNameHere", "YourPackNameHere", "0.0.1")]
	//Bepin dependency for VarianceAPI MUST exist, otherwise your mod may load before VarianceAPI's systems are implemented!
	[BepInDependency("com.Nebby.VarianceAPI", BepInDependency.DependencyFlags.HardDependency)]
    
	/// <summary>
    /// Your Main Class is where you'll interact with VarianceAPI's methods.
    /// </summary>
    public class MainClass : BaseUnityPlugin
    {
        public static MainClass instance;

        /// <summary>
        /// This is where you'll access any Assets from your AssetBundle.
        /// </summary>
        public static AssetBundle yourPackNameHereAssets = null;

        /// <summary>
        /// Your AssetBundle's name as stated in your thunderkit manifest. 
        /// </summary>
        internal static string assetBundleName = "YourPackNameHereAssets";
        // Start is called before the first frame update

        public void Awake()
        {
            instance = this;
            LoadAssetsAndRegisterContentPack();
			InitVariantPack();
        }

		/// <summary>
		/// This method prepares your variants for registering using VarianceAPI.
		/// <para>If you need to modify certain things, such as Grabbing vanilla resources using MaterialGrabber. ALWAYS run them BEFORE using VariantRegister's RegisterConfigs()</para>
		/// </summary>
		public void InitVariantPack()
        {
			var MG = new MaterialGrabber();
			MG.StartGrabber(yourPackNameHereAssets);
			var MCV = new MyCodedVariants();
			MCV.Init(Config);
			var VR = new VariantRegister();
			VR.RegisterConfigs(yourPackNameHereAssets, Config);
        }
		/// <summary>
		/// This method loads your AssetBundle and registers your contentpack.
		/// <para>Always call this BEFORE making any modifications to your AssetBundle via code.</para>
		/// </summary>
		public void LoadAssetsAndRegisterContentPack()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            yourPackNameHereAssets = AssetBundle.LoadFromFile(Path.Combine(path, assetBundleName));
            
			/*
			 ContentPacks arent 100% necesary for making your VariantPack, unless you're planning on adding custom content such as Items for your Variants
			or Custom skills.
			As such, the line that enables the contentPack to load is commented out. only un-comment it once you've created your ContentPack in the Editor.
			 */
			//ContentPackProvider.serializedContentPack = yourPackNameHereAssets.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
        }
    }
	public class ContentPackProvider : IContentPackProvider
	{
		public static SerializableContentPack serializedContentPack;
		/// <summary>
		/// This is where you'll be calling the content in your contentPack.
		/// </summary>
		public static ContentPack contentPack;
		//Should be the same names as your SerializableContentPack in the asset bundle
		public static string contentPackName = "YourPackNameHereContent";

		public string identifier
		{
			get
			{
				//If I see this name while loading a mod Kevin from HP's Customer Service will make fun of you
				return "YourPackNameHere";
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
