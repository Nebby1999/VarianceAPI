#nullable enable
using RoR2.ContentManagement;
using System.Collections;

namespace VAPI
{
    //TODO: create VariantPackProvider
    public class VAPIContent : IContentPackProvider//, IVariantPackProvider
    {
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