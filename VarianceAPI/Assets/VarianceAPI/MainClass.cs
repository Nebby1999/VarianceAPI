using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using VarianceAPI.Components;
using VarianceAPI.Modules;
using VarianceAPI.Utils;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
namespace VarianceAPI
{
	[BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("iHarbHD.DebugToolkit", BepInDependency.DependencyFlags.SoftDependency)]
	[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
	[BepInPlugin(GUID, NAME, VERSION)]
	[R2APISubmoduleDependency(new string[]
		{
			nameof(LanguageAPI),
			nameof(ArtifactCodeAPI),
			nameof(DamageAPI),
			nameof(CommandHelper),
			nameof(PrefabAPI)
		})]
	internal class MainClass : BaseUnityPlugin
    {
		public const string GUID = "com.Nebby.VarianceAPI";
		public const string NAME = "VarianceAPI";
		public const string VERSION = "1.1.0";

		private readonly static bool DEBUG = false;

        public static MainClass Instace;
		public static PluginInfo PluginInfo;


        internal void Awake()
        {
			if(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("iHarbHD.DebugToolkit"))
            {
				R2API.Utils.CommandHelper.AddToConsoleWhenReady();
			}
			VAPILog.logger = Logger;
			Instace = this;
			PluginInfo = Info;
			

			if(DEBUG)
            {
				gameObject.AddComponent<VAPIDebug>();
            }
			Initialize();
			new ContentPacks().Initialize();
		}

		internal void Initialize()
		{
			VariantRegister.Initialize();
			Assets.Initialize();
			Interfaces.Initialize();
			VAPILanguage.Initialize();
			ConfigLoader.Initialize(Config);

			new Pickups().Initialize();

            #region artifact moment
			if(ConfigLoader.EnableArtifactOfVariance.Value)
            {
				var artifact = Assets.VAPIAssets.LoadAsset<ArtifactDef>("Variance");
				HG.ArrayUtils.ArrayAppend(ref ContentPacks.serializableContentPack.artifactDefs, artifact);
				R2API.ArtifactCodeAPI.AddCode(artifact, Assets.VAPIAssets.LoadAsset<ArtifactCode>("VarianceCode"));
            }
			#endregion
		}
    }
}