#nullable enable
using EntityStates;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    public sealed class VAPIContent : IContentPackProvider
    {
        public static readonly LazyLoader<ExpansionDef> VAPIExpansion = new LazyLoader<ExpansionDef>(nameof(VAPIExpansion));
        public static class Artifacts
        {
            public static readonly LazyLoader<ArtifactDef> Variance = new LazyLoader<ArtifactDef>(nameof(Variance));
        }

        public static class VariantTierDefs
        {
            public static readonly LazyLoader<CharacterVariantTierDef> Common = new LazyLoader<CharacterVariantTierDef>(nameof(Common));
            public static readonly LazyLoader<CharacterVariantTierDef> Uncommon = new LazyLoader<CharacterVariantTierDef>(nameof(Uncommon));
            public static readonly LazyLoader<CharacterVariantTierDef> Rare = new LazyLoader<CharacterVariantTierDef>(nameof(Rare));
            public static readonly LazyLoader<CharacterVariantTierDef> Legendary = new LazyLoader<CharacterVariantTierDef>(nameof(Legendary));
        }

        public static class Buffs
        {
            public static LazyLoader<BuffDef> Variant = new LazyLoader<BuffDef>("bd" + nameof(Variant));
            public static LazyLoader<BuffDef> LinearArmorBonus = new LazyLoader<BuffDef>("bd" + nameof(LinearArmorBonus));
        }

        public static class Items
        { 
            public static LazyLoader<ItemDef> GreenHealthbar = new LazyLoader<ItemDef>(nameof(GreenHealthbar));
            public static LazyLoader<ItemDef> Plus1Crit = new LazyLoader<ItemDef>(nameof(Plus1Crit));
            public static LazyLoader<ItemDef> ExtraPrimary = new LazyLoader<ItemDef>(nameof(ExtraPrimary));
            public static LazyLoader<ItemDef> ExtraSecondary = new LazyLoader<ItemDef>(nameof(ExtraSecondary));
            public static LazyLoader<ItemDef> ExtraUtility = new LazyLoader<ItemDef>(nameof(ExtraUtility));
            public static LazyLoader<ItemDef> ExtraSpecial = new LazyLoader<ItemDef>(nameof(ExtraSpecial));
            public static LazyLoader<ItemDef> GlobalCDR = new LazyLoader<ItemDef>(nameof(GlobalCDR));
            public static LazyLoader<ItemDef> PrimaryCDR = new LazyLoader<ItemDef>(nameof(PrimaryCDR));
            public static LazyLoader<ItemDef> SecondaryCDR = new LazyLoader<ItemDef>(nameof(SecondaryCDR));
            public static LazyLoader<ItemDef> UtilityCDR = new LazyLoader<ItemDef>(nameof(UtilityCDR));
            public static LazyLoader<ItemDef> SpecialCDR = new LazyLoader<ItemDef>(nameof(SpecialCDR));
        }

        public string identifier => VAPIMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(contentPack);
        public static ReadOnlyVariantPack readOnlyVariantPack => variantPack != null ? new ReadOnlyVariantPack(variantPack) : default;
        private static ContentPack contentPack { get; } = new ContentPack();
        private static VariantPack? variantPack { get; set; }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;

            //Initialize Assets, then Config
            IEnumerator initialization = InitializeAssetsAndConfig();
            while(initialization.MoveNext())
            {
                yield return null;
            }

            //Populate VAPI Content Pack
            var packPopulation = PopulateContentPack();
            while(packPopulation.MoveNext())
            {
                yield return null;
            }

            //Populate Base VariantPack
            var variantPackPopulation = PopulateVariantPack();
            while(variantPackPopulation.MoveNext())
            {
                yield return null;
            }

            VariantPackManager.AddVariantPack(variantPack);
        }

        private IEnumerator InitializeAssetsAndConfig()
        {
            var assetsInit = VAPIAssets.Initialize();
            while(assetsInit.MoveNext())
            {
                yield return null;
            }

            var parallelProgressCoroutine = new HG.Coroutines.ParallelCoroutine();
            parallelProgressCoroutine.Add(new VAPIConfig().InitializeAsync(VAPIMain.instance));
            parallelProgressCoroutine.Add(LanguageFileLoader.AddLanguageFilesFromModAsync(VAPIMain.instance, "lang"));
        
            while(parallelProgressCoroutine.MoveNext())
            {
                yield return null;
            }
        }

        private IEnumerator PopulateContentPack()
        {
            HG.Coroutines.ParallelCoroutine parallelLoadCoroutine = new HG.Coroutines.ParallelCoroutine();
            var artifactLoad = VAPIAssets.LoadAssetsAsync<ArtifactDef>();
            var buffLoad = VAPIAssets.LoadAssetsAsync<BuffDef>();
            var itemLoad = VAPIAssets.LoadAssetsAsync<ItemDef>();
            var prefabLoad = VAPIAssets.LoadAssetsAsync<GameObject>();
            var skillDefLoad = VAPIAssets.LoadAssetsAsync<SkillDef>();

            parallelLoadCoroutine.Add(artifactLoad);
            parallelLoadCoroutine.Add(buffLoad);
            parallelLoadCoroutine.Add(itemLoad);
            parallelLoadCoroutine.Add(prefabLoad);
            parallelLoadCoroutine.Add(skillDefLoad);

            while (parallelLoadCoroutine.MoveNext())
            {
                yield return null;
            }

            contentPack.artifactDefs.Add(artifactLoad.assets);
            contentPack.buffDefs.Add(buffLoad.assets);
            contentPack.itemDefs.Add(itemLoad.assets);
            contentPack.skillDefs.Add(skillDefLoad.assets);
            contentPack.entityStateTypes.AddSingle(typeof(GoToMain));

            for (int i = 0; i < prefabLoad.assets!.Length; i++)
            {
                if (prefabLoad.assets[i].TryGetComponent<NetworkIdentity>(out var netPrefab))
                {
                    contentPack.networkedObjectPrefabs.AddSingle(prefabLoad.assets[i]);
                }
            }
        }

        private IEnumerator PopulateVariantPack()
        {
            var iconRequest = VAPIAssets.LoadAssetAsync<Sprite>("texVAPIIcon");
            var variantTierDefRequest = VAPIAssets.LoadAssetsAsync<CharacterVariantTierDef>();

            var parallelCoroutine = new HG.Coroutines.ParallelCoroutine();
            parallelCoroutine.Add(iconRequest);
            parallelCoroutine.Add(variantTierDefRequest);

            while (parallelCoroutine.MoveNext())
                yield return null;

            variantPack = new VariantPack(identifier, iconRequest.asset!, VAPIConfig.rewardsConfig!, VAPIMain.instance!.Info.Metadata);
            variantPack.characterVariantTierDefs.Add(variantTierDefRequest.assets!);
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }
        
        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProviderDelegate)
        {
            addContentPackProviderDelegate(this);
        }

        internal VAPIContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
        }
    }
}