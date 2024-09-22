using EntityStates;
using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VAPI.Modules;
namespace VAPI
{
    public class VAPIContent : IContentPackProvider
    {
        public string identifier => VAPIMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(contentPack);

        internal static ContentPack contentPack { get; } = new ContentPack();

        internal static ParallelMultiStartCoroutine _parallelPreLoadDispatchers = new ParallelMultiStartCoroutine();

        private static Func<IEnumerator>[] _loadDispatchers;

        internal static ParallelMultiStartCoroutine _parallelPostLoadDispatchers = new ParallelMultiStartCoroutine();

        private static Action[] _fieldAssignDispatchers;

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var enumerator = VAPIAssets.Initialize();
            while(!enumerator.IsDone())
            {
                yield return null;
            }

            _parallelPreLoadDispatchers.Start();
            while (!_parallelPreLoadDispatchers.isDone)
                yield return null;

            for (int i = 0; i < _loadDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _loadDispatchers.Length, 0.1f, 0.2f)); //report progress
                enumerator = _loadDispatchers[i](); //call method

                while (enumerator?.MoveNext() ?? false) yield return null; //await
            }

            _parallelPostLoadDispatchers.Start();
            while (!_parallelPostLoadDispatchers.isDone)
                yield return null;

            for(int i = 0; i < _fieldAssignDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _fieldAssignDispatchers.Length, 0.95f, 0.99f));
                _fieldAssignDispatchers[i]();
            }
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield return null;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private static IEnumerator AddBaseVariantPack()
        {
            VAPILog.Info($"Adding base VariantPack");

            var request = VAPIAssets.LoadAssetAsync<VariantPackDef>("BaseVariantPack");
            while (!request.isDone)
                yield return null;

            VariantPackCatalog.AddVariantPack(request.asset, VAPIConfig.rewardsConfig);
        }

        private static void LoadFromAssetBundles()
        {
            _parallelPreLoadDispatchers.Add(PopulateWithAssetCollectionContentPack);
            _parallelPostLoadDispatchers.Add(LoadEmptySkillDef);
            _parallelPostLoadDispatchers.Add(LoadLockedIconAndAssignToExpansionDef);

            IEnumerator LoadEmptySkillDef()
            {
                var request = VAPIAssets.LoadAssetAsync<SkillDef>("EmptySkillDef");
                
                while (!request.isDone)
                    yield return null;

                VAPIAssets._emptySkillDef = request.asset;
            }

            IEnumerator LoadLockedIconAndAssignToExpansionDef()
            {
                ParallelCoroutine coroutine = new ParallelCoroutine();

                var request = VAPIAssets.LoadAssetAsync<ExpansionDef>("VarianceExpansion");
                var iconRequest = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png");

                coroutine.Add(request);
                coroutine.Add(iconRequest);

                while (!coroutine.IsDone())
                    yield return null;

                request.asset.disabledIconSprite = iconRequest.Result;
            }

            IEnumerator PopulateWithAssetCollectionContentPack()
            {
                var request = VAPIAssets.LoadAssetAsync<AssetCollection>("acContentPack");
                while (!request.isDone) 
                    yield return null;

                contentPack.AddContentFromAssetCollection(request.asset);
            }
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProviderDelegate)
        {
            addContentPackProviderDelegate(this);
        }

        internal VAPIContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
            VAPIAssets.assetsAvailability.CallWhenAvailable(() =>
            {
                _parallelPreLoadDispatchers.Add(LanguageFileLoader.AddLanguageFilesFromModAsync, VAPIMain.instance, "languages");
                LoadFromAssetBundles();
            });
        }

        static VAPIContent()
        {
            VAPIMain main = VAPIMain.instance;
            _loadDispatchers = new Func<IEnumerator>[]
            {
                () =>
                {
                    VAPILog.Info($"Initializing Items...");
                    MSU.ItemModule.AddProvider(main, ContentUtil.CreateGenericContentPieceProvider<ItemDef>(main, contentPack));
                    return MSU.ItemModule.InitializeItems(main);
                },
                () =>
                {
                    VAPILog.Info($"Initializing Artifacts...");
                    MSU.ArtifactModule.AddProvider(main, ContentUtil.CreateGenericContentPieceProvider<ArtifactDef>(main, contentPack));
                    return ArtifactModule.InitializeArtifacts(main);
                },
                AddBaseVariantPack,
                InfiniteTower.Init,
            };
            _fieldAssignDispatchers = new Action[]
            {
                () => ContentUtil.PopulateTypeFields(typeof(Artifacts), contentPack.artifactDefs),
                () => ContentUtil.PopulateTypeFields(typeof(Buffs), contentPack.buffDefs),
                () => ContentUtil.PopulateTypeFields(typeof(Items), contentPack.itemDefs)
            };
        }

        /// <summary>
        /// A static list of all the default VariantTierDefs
        /// </summary>
        public static class VariantTiers
        {
            /// <summary>
            /// Loads the Common VariantTierDef
            /// </summary>
            public static VariantTierDef Common => Load(nameof(Common));
            /// <summary>
            /// Loads the Uncommon VariantTierDef
            /// </summary>
            public static VariantTierDef Uncommon => Load(nameof(Uncommon));
            /// <summary>
            /// Loads the Rare VariantTierDef
            /// </summary>
            public static VariantTierDef Rare => Load(nameof(Rare));
            /// <summary>
            /// Loads the Legendary VariantTierDef
            /// </summary>
            public static VariantTierDef Legendary => Load(nameof(Legendary));

            private static VariantTierDef Load(string name)
            {
                if (!VAPIAssets.assetsAvailability.available)
                    throw new InvalidOperationException($"Cannot load tier {name} without VAPIAssets initialized.");

                return VAPIAssets.LoadAsset<VariantTierDef>(name);
            }
        }

        /// <summary>
        /// A static class with the ArtifactOfVariance's ArtifactDef
        /// </summary>
        public static class Artifacts
        {
            public static ArtifactDef Variance;
        }

        /// <summary>
        /// A static class with the VarianceAPI's variant buffDef
        /// </summary>
        public static class Buffs
        {
            public static BuffDef bdVariant;
        }

        /// <summary>
        /// A static class with VarianceAPI's intrinsic variant items
        /// </summary>
        public static class Items
        {
            public static ItemDef ExtraPrimary;
            public static ItemDef ExtraSecondary;
            public static ItemDef ExtraSpecial;
            public static ItemDef ExtraUtility;
            public static ItemDef GlobalCDR;
            public static ItemDef GreenHealthbar;
            public static ItemDef Plus1Crit;
            public static ItemDef PrimaryCDR;
            public static ItemDef SecondaryCDR;
            public static ItemDef SpecialCDR;
            public static ItemDef UtilityCDR;
        }
    }
}
