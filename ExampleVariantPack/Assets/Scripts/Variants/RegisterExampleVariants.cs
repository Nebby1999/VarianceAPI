using UnityEngine;
using VarianceAPI.Modules;

namespace ExampleVariantPack.Variants
{
    public class RegisterExampleVariants : VariantRegisterBase<RegisterExampleVariants>
    {
        public override AssetBundle AssetBundle => MainScript.exampleVariancePackAssets;

        public static void Init()
        {
            var meme = new RegisterExampleVariants.Register();
        }
        public override void Register()
        {
            if (AssetBundle == null)
            {
                Debug.LogError("Asset Bundle is null! Aborting!");
                return;
            }
            else
            {
                GetVariantInfos();
                RegisterVariantInfos();
            }
        }
    }
}
