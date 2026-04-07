#nullable enable
using EntityStates;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    //TODO: create VariantPackProvider
    public class VAPIContent : IContentPackProvider//, IVariantPackProvider
    {
        public static class Artifacts
        {
            public static ArtifactDef? Variance;
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
            public static BuffDef? Variant;
            public static BuffDef? LinearArmorBonus;
        }

        public static class Items
        {
            public static ItemDef? ExtraPrimary;
            public static ItemDef? ExtraSecondary;
            public static ItemDef? ExtraSpecial;
            public static ItemDef? ExtraUtility;
            public static ItemDef? GlobalCDR;
            public static ItemDef? GreenHealthbar;
            public static ItemDef? Plus1Crit;
            public static ItemDef? PrimaryCDR;
            public static ItemDef? SecondaryCDR;
            public static ItemDef? SpecialCDR;
            public static ItemDef? UtilityCDR;
        }

        public string identifier => VAPIMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(contentPack);

        private static ContentPack contentPack { get; } = new ContentPack();

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            //Initialize assets
            var enumerator = VAPIAssets.Initialize();
            while(enumerator.MoveNext())
            {
                yield return null;
            }

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

            while(parallelLoadCoroutine.MoveNext())
            {
                yield return null;
            }

            contentPack.artifactDefs.Add(artifactLoad.assets);
            contentPack.buffDefs.Add(buffLoad.assets);
            contentPack.itemDefs.Add(itemLoad.assets);
            contentPack.skillDefs.Add(skillDefLoad.assets);
            contentPack.entityStateTypes.AddSingle(typeof(GoToMain));

            for(int i = 0; i < prefabLoad.assets!.Length; i++)
            {
                if (prefabLoad.assets[i].TryGetComponent<NetworkIdentity>(out var netPrefab))
                {
                    contentPack.networkedObjectPrefabs.AddSingle(prefabLoad.assets[i]);
                }
            }
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
            //TODO: do language load
            ContentManager.collectContentPackProviders += AddSelf;
        }
    }
}