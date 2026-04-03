#nullable enable
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections;
using UnityEngine;

namespace VAPI.AddressableAssets
{
    [Serializable]
    public class AddressReferencedItemTierDef : AddressReferencedAsset<ItemTierDef>
    {
        public override bool CanLoadFromCatalog { get => _canLoadFromCatalog; protected set => _canLoadFromCatalog = value; }

        [SerializeField]
        [HideInInspector]
        private bool _canLoadFromCatalog = true;

        protected override IEnumerator LoadAsyncCoroutine()
        {
            if(CanLoadFromCatalog)
            {
                ItemTierDef tierDef = ItemTierCatalog.FindTierDef(Address);
                if(tierDef)
                {
                    Asset = tierDef;
                    yield break;
                }
            }

            var subroutine = LoadFromAddressAsyncCoroutine();
            while(subroutine.MoveNext())
            {
                yield return null;
            }
        }

        protected override void Load()
        {
            if(CanLoadFromCatalog)
            {
                ItemTierDef tierDef = ItemTierCatalog.FindTierDef(Address);
                if (tierDef)
                {
                    Asset = tierDef;
                    return;
                }
            }
            LoadFromAddress();
        }

        public static implicit operator bool(AddressReferencedItemTierDef addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        public static implicit operator ItemTierDef?(AddressReferencedItemTierDef addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        public static implicit operator AddressReferencedItemTierDef(string address)
        {
            return new AddressReferencedItemTierDef(address);
        }

        public static implicit operator AddressReferencedItemTierDef(ItemTierDef asset)
        {
            return new AddressReferencedItemTierDef(asset);
        }

        public AddressReferencedItemTierDef()
        {
        }

        public AddressReferencedItemTierDef(ItemTierDef def)
            : base(def)
        {
        }

        public AddressReferencedItemTierDef(string addressOrName)
            : base(addressOrName)
        {
        }
    }
}