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
    /// <summary>
    /// VarianceAPI's <see cref="IContentPackProvider"/>
    /// </summary>
    public sealed class VAPIContent : IContentPackProvider
    {
        /// <summary>
        /// Access to VAPI's ExpansionDef, this expansion must be enabled for Variants to spawn. Loads on demand
        /// </summary>
        public static readonly LazyLoader<ExpansionDef> VAPIExpansion = new LazyLoader<ExpansionDef>(nameof(VAPIExpansion));

        //TODO: Implement artifact code
        /// <summary>
        /// VAPI's Artifact
        /// </summary>
        public static class Artifacts
        {
            /// <summary>
            /// Reference to the Artifact of Variance. Loads on demand
            /// </summary>
            public static readonly LazyLoader<ArtifactDef> Variance = new LazyLoader<ArtifactDef>(nameof(Variance));
        }

        /// <summary>
        /// VAPI's Built in <see cref="CharacterVariantTierDef"/>
        /// </summary>
        public static class VariantTierDefs
        {
            /// <summary>
            /// Reference to the Common tier. Loads on demand
            /// </summary>
            public static readonly LazyLoader<CharacterVariantTierDef> Common = new LazyLoader<CharacterVariantTierDef>(nameof(Common));

            /// <summary>
            /// Reference to the Uncommon tier. Loads on demand
            /// </summary>
            public static readonly LazyLoader<CharacterVariantTierDef> Uncommon = new LazyLoader<CharacterVariantTierDef>(nameof(Uncommon));

            /// <summary>
            /// Reference to the Rare tier. Loads on demand
            /// </summary>
            public static readonly LazyLoader<CharacterVariantTierDef> Rare = new LazyLoader<CharacterVariantTierDef>(nameof(Rare));

            /// <summary>
            /// Reference to the Legendary tier. Loads on demand
            /// </summary>
            public static readonly LazyLoader<CharacterVariantTierDef> Legendary = new LazyLoader<CharacterVariantTierDef>(nameof(Legendary));
        }

        /// <summary>
        /// VAPI's Built in <see cref="BuffDef"/>
        /// </summary>
        public static class Buffs
        {
            /// <summary>
            /// Reference to a <see cref="BuffDef"/> that's applied to a character that's a Variant. Loads on demand
            /// </summary>
            public static LazyLoader<BuffDef> VAPIVariant = new LazyLoader<BuffDef>("bd" + nameof(VAPIVariant));
            /// <summary>
            /// Reference to a <see cref="BuffDef"/> that adds +1 armor per stack. Loads on demand
            /// </summary>
            public static LazyLoader<BuffDef> VAPILinearArmorBonus = new LazyLoader<BuffDef>("bd" + nameof(VAPILinearArmorBonus));
            /// <summary>
            /// Reference to a <see cref="BuffDef"/> that increases the character's scale by 1%. Loads on demand
            /// </summary>
            public static LazyLoader<BuffDef> VAPIIncreaseScale = new LazyLoader<BuffDef>("bd" + nameof(VAPIIncreaseScale));
            /// <summary>
            /// Reference to a <see cref="BuffDef"/> that reduces the character's scale by 1%. Loads on demand
            /// </summary>
            public static LazyLoader<BuffDef> VAPIDecreaseScale = new LazyLoader<BuffDef>("bd" + nameof(VAPIIncreaseScale));
        }

        /// <summary>
        /// VAPI's Built in <see cref="ItemDef"/>
        /// </summary>
        public static class Items
        { 
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that changes a Character's health bar to green by default. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> GreenHealthbar = new LazyLoader<ItemDef>(nameof(GreenHealthbar));
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that adds +1% crit chance per stack. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> Plus1Crit = new LazyLoader<ItemDef>(nameof(Plus1Crit));
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that adds +1 primary stock per stack. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> ExtraPrimary = new LazyLoader<ItemDef>(nameof(ExtraPrimary));
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that adds +1 Secondary stock per stack. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> ExtraSecondary = new LazyLoader<ItemDef>(nameof(ExtraSecondary));
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that adds +1 Utility stock per stack. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> ExtraUtility = new LazyLoader<ItemDef>(nameof(ExtraUtility));
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that adds +1 Special stock per stack. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> ExtraSpecial = new LazyLoader<ItemDef>(nameof(ExtraSpecial));
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that decreases ALL cooldowns by 1% per stack. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> GlobalCDR = new LazyLoader<ItemDef>(nameof(GlobalCDR));
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that decreases Primary cooldown by 1% per stack. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> PrimaryCDR = new LazyLoader<ItemDef>(nameof(PrimaryCDR));
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that decreases Secondary cooldown by 1% per stack. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> SecondaryCDR = new LazyLoader<ItemDef>(nameof(SecondaryCDR));
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that decreases Utility cooldown by 1% per stack. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> UtilityCDR = new LazyLoader<ItemDef>(nameof(UtilityCDR));
            /// <summary>
            /// Reference to an <see cref="ItemDef"/> that decreases Special cooldown by 1% per stack. Loads on demand
            /// </summary>
            public static LazyLoader<ItemDef> SpecialCDR = new LazyLoader<ItemDef>(nameof(SpecialCDR));
        }

        /// <summary>
        /// The ConntentPack's identifier
        /// </summary>
        public string identifier => VAPIMain.GUID;

        /// <summary>
        /// A <see cref="ReadOnlyContentPack"/> of this contentpack
        /// </summary>
        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(contentPack);
        /// <summary>
        /// A <see cref="ReadOnlyVariantPack"/> of VAPI's base variantpack.
        /// </summary>
        public static ReadOnlyVariantPack readOnlyVariantPack => variantPack != null ? new ReadOnlyVariantPack(variantPack) : default;


        private static ContentPack contentPack { get; } = new ContentPack();
        private static VariantPack? variantPack { get; set; } //Created on demand during load static content async

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

            var parallelProgressCoroutine = ParallelCoroutine.RequestFromPool();

            parallelProgressCoroutine.Add(new VAPIConfig().InitializeAsync(VAPIMain.instance!));
            parallelProgressCoroutine.Add(LanguageFileLoader.AddLanguageFilesFromModAsync(VAPIMain.instance, "lang")); //Configure VAPI and add language tokens
        
            while(parallelProgressCoroutine.MoveNext())
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelProgressCoroutine);
        }

        private IEnumerator PopulateContentPack()
        {
            var parallelLoadCoroutine = ParallelCoroutine.RequestFromPool(); ;

            using var artifactLoad = VAPIAssets.LoadAssetsAsync<ArtifactDef>();
            using var buffLoad = VAPIAssets.LoadAssetsAsync<BuffDef>();
            using var itemLoad = VAPIAssets.LoadAssetsAsync<ItemDef>();
            using var prefabLoad = VAPIAssets.LoadAssetsAsync<GameObject>();
            using var skillDefLoad = VAPIAssets.LoadAssetsAsync<SkillDef>();

            parallelLoadCoroutine.Add(artifactLoad);
            parallelLoadCoroutine.Add(buffLoad);
            parallelLoadCoroutine.Add(itemLoad);
            parallelLoadCoroutine.Add(prefabLoad);
            parallelLoadCoroutine.Add(skillDefLoad);

            while (parallelLoadCoroutine.MoveNext())
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(parallelLoadCoroutine);

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
            //Load the pack's icon and the default tiers
            using var iconRequest = VAPIAssets.LoadAssetAsync<Sprite>("texVAPIIcon");
            using var variantTierDefRequest = VAPIAssets.LoadAssetsAsync<CharacterVariantTierDef>();

            var parallelCoroutine = ParallelCoroutine.RequestFromPool();

            parallelCoroutine.Add(iconRequest);
            parallelCoroutine.Add(variantTierDefRequest);

            while (parallelCoroutine.MoveNext())
                yield return null;

            ParallelCoroutine.ReturnToPool(parallelCoroutine);

            //Initialize the pack
            variantPack = new VariantPack(identifier, iconRequest.asset!, VAPIConfig.rewardsConfig!, VAPIMain.instance!.Info.Metadata);
            variantPack.characterVariantTierDefs.Add(variantTierDefRequest.assets!);
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