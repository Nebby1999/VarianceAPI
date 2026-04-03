#nullable enable
using RoR2;
using RoR2.ContentManagement;
using System.Collections;

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
            public static readonly LazyLoader<VariantTierDef> Common = new LazyLoader<VariantTierDef>(nameof(Common));
            public static readonly LazyLoader<VariantTierDef> Uncommon = new LazyLoader<VariantTierDef>(nameof(Uncommon));
            public static readonly LazyLoader<VariantTierDef> Rare = new LazyLoader<VariantTierDef>(nameof(Rare));
            public static readonly LazyLoader<VariantTierDef> Legendary = new LazyLoader<VariantTierDef>(nameof(Legendary));
        }

        public static class Buffs
        {
            public static BuffDef? Variant;
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