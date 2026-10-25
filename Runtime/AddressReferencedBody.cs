using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections;
using UnityEngine;

namespace VAPI.AddressReferencedAssets
{
    [Serializable]
    public sealed class AddressReferencedBody : AddressReferencedAsset<GameObject>
    {
        public override bool CanLoadFromCatalog { get => _canLoadFromCatalog; protected set => _canLoadFromCatalog = value; }

        [SerializeField, HideInInspector]
        private bool _canLoadFromCatalog = true;

        protected override IEnumerator LoadAsyncCoroutine()
        {
            if(CanLoadFromCatalog)
            {
                BodyIndex bodyIndex = BodyCatalog.FindBodyIndex(Address);
                if(bodyIndex != BodyIndex.None)
                {
                    Asset = BodyCatalog.GetBodyPrefab(bodyIndex);
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
            if (CanLoadFromCatalog)
            {
                BodyIndex bodyIndex = BodyCatalog.FindBodyIndex(Address);
                if (bodyIndex != BodyIndex.None)
                {
                    Asset = BodyCatalog.GetBodyPrefab(bodyIndex);
                    return;
                }
            }
            LoadFromAddress();
        }

        public static implicit operator bool(AddressReferencedBody addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        public static implicit operator GameObject(AddressReferencedBody addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        public static implicit operator AddressReferencedBody(string address)
        {
            return new AddressReferencedBody(address);
        }

        public static implicit operator AddressReferencedBody(GameObject asset)
        {
            return new AddressReferencedBody(asset);
        }

        public AddressReferencedBody() : base() { }

        public AddressReferencedBody(GameObject asset) : base(asset) { }

        public AddressReferencedBody(string addressOrName) : base(addressOrName) { }
    }
}