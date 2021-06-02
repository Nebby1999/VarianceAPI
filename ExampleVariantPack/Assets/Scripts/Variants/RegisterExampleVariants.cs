using UnityEngine;
using VarianceAPI.Modules;

namespace ExampleVariantPack.Variants
{
    public class RegisterExampleVariants : VariantRegisterBase
    {
        public void RegisterVariants()
        {
            Init(MainScript.exampleVariancePackAssets);
        }
    }
}
