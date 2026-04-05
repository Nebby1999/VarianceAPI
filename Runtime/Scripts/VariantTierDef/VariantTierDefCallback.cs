#nullable enable
using R2API.AddressReferencedAssets;
using RoR2;
using System;

namespace VAPI
{
    public interface IVariantTierDefCallback : IValidatable
    {
        public IDisposable? OnMasterBecameVariant(CharacterMasterVariantStorage variantStorage);
        public IDisposable? OnBodyBecameVariant(CharacterBodyVariantController variantController);
    }

    //Applies buff to variant
    [Serializable]
    public class CommonVariantTierCallback : IVariantTierDefCallback
    {
        private struct DisposableCommonVariantModifier : IDisposable
        {
            private CharacterBody _affectedBody;
            private BuffDef _appliedBuff;

            public DisposableCommonVariantModifier(CharacterBody affectedBody, BuffDef appliedBuff)
            {
                _affectedBody = affectedBody;
                _appliedBuff = appliedBuff;
            }

            public void Dispose()
            {
                if(_affectedBody && _appliedBuff)
                {
                    _affectedBody.RemoveBuff(_appliedBuff);
                }
            }
        }

        public AddressReferencedBuffDef variantBuffDef = new AddressReferencedBuffDef();
        public virtual IDisposable? OnBodyBecameVariant(CharacterBodyVariantController variantController)
        {
            if (!variantController.characterBody)
                return null;

            BuffDef buffDef = variantBuffDef.LoadAssetNow();
            if (!buffDef)
                return null;

            var result = new DisposableCommonVariantModifier(variantController.characterBody, buffDef);
            variantController.characterBody.AddBuff(buffDef);
            return result;
        }

        public virtual IDisposable? OnMasterBecameVariant(CharacterMasterVariantStorage variantStorage)
        {
            return null;
        }

        public virtual void Validate()
        {
        }
    }

    public class UncommonVariantTierCallback : CommonVariantTierCallback
    {
        public AddressableItemCountPair tierItem;
    }

    public class RareVariantTierCallback : UncommonVariantTierCallback
    {

    }

    public class LegendaryVariantTierCallback : RareVariantTierCallback
    {

    }
}