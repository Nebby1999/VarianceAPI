using RoR2.ContentManagement;

namespace VarianceAPI
{
    internal class ContentPacks : IContentPackProvider
    {
        public static SerializableContentPack serializableContentPack;
        internal ContentPack contentPack;
        public string identifier => MainClass.GUID;

        public void Initialize()
        {
            contentPack = serializableContentPack.CreateContentPack();
            contentPack.identifier = identifier;
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
