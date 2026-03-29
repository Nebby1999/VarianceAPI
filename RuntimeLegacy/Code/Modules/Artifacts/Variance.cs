using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;

namespace VAPI.Artifacts
{
    public class Variance : IArtifactContentPiece
    {
        public NullableRef<ArtifactCode> artifactCode { get; private set; }

        public ArtifactDef asset { get; private set; }

        public void Initialize()
        {
        }

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public IEnumerator LoadContentAsync()
        {
            var assetRequest = VAPIAssets.LoadAssetAsync<AssetCollection>("acVariance");
            while (!assetRequest.isDone)
                yield return null;

            var ac = assetRequest.asset;

            asset = ac.FindAsset<ArtifactDef>("Variance");
            artifactCode = ac.FindAsset<ArtifactCode>("VarianceCode");
        }

        public void OnArtifactDisabled()
        {
        }

        public void OnArtifactEnabled()
        {
        }
    }
}