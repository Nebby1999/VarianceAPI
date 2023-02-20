using Moonstorm.Loaders;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VAPI
{
    /// <summary>
    /// VAPI's AssetsLoader
    /// </summary>
    public class VAPIAssets : AssetsLoader<VAPIAssets>
    {
        public string AssemblyDir => Path.GetDirectoryName(VAPIMain.Instance.Info.Location);
        public override AssetBundle MainAssetBundle => _assetBundle;
        private AssetBundle _assetBundle;
        internal SkillDef emptySkillDef;

        internal void Init()
        {
            var bundlePath = Path.Combine(AssemblyDir, "assetbundles", "vapiassets");
            _assetBundle = AssetBundle.LoadFromFile(bundlePath);
            emptySkillDef = MainAssetBundle.LoadAsset<SkillDef>("EmptySkillDef");
            _assetBundle.LoadAsset<ExpansionDef>("VarianceExpansion").disabledIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();
        }

        internal void SwapShaders()
        {
            SwapShadersFromMaterials(MainAssetBundle.LoadAllAssets<Material>().Where(mat => mat.shader.name.StartsWith("Stubbed")));
        }
    }
}