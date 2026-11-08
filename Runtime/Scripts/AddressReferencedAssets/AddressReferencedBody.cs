#nullable enable
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections;
using UnityEngine;

namespace VAPI.AddressReferencedAssets
{
    /// <summary>
    /// An <see cref="AddressReferencedAsset{T}"/> that loads GameObjects, and has support for loading via the Body Catalog.
    /// <br></br>
    /// Like other uses of AddressReferencedAsset, it is not recommended to reuse an instance across various <see cref="CharacterVariantTarget"/>, instead clone them via <see cref="VAPIUtils.CloneAddressReferencedAsset{T1, T2}(T1)"/>
    /// </summary>
    [Serializable]
    public sealed class AddressReferencedBody : AddressReferencedAsset<GameObject>
    {
        /// <summary>
        ///Wether this AddressReferencedAsset can load an Asset using the game's catalogues.
        ///
        ///     If this is true, you're encouraged to wait for AddressReferencedAsset to initialize
        ///     fully using R2API.AddressReferencedAssets.AddressReferencedAsset.OnAddressReferencedAssetsLoaded
        /// </summary>
        public override bool CanLoadFromCatalog { get => _canLoadFromCatalog; protected set => _canLoadFromCatalog = value; }

        [SerializeField, HideInInspector]
        private bool _canLoadFromCatalog = true;

        protected override bool IsCatalogAvailable()
        {
            return BodyCatalog.availability.available;
        }

        /// <summary>
        /// Loads the body asynchronously via a coroutine.
        /// <br></br>
        /// If the underlying address is not empty, it'll try to load the asset via the body catalog.
        /// </summary>
        protected override IEnumerator LoadAsyncCoroutine()
        {
            if (CanLoadFromCatalog)
            {
                BodyIndex bodyIndex = BodyCatalog.FindBodyIndex(Address);
                if (bodyIndex != BodyIndex.None)
                {
                    Asset = BodyCatalog.GetBodyPrefab(bodyIndex);
                    yield break;
                }
            }
            var subroutine = LoadFromAddressAsyncCoroutine();
            while (subroutine.MoveNext())
            {
                yield return null;
            }
        }

        /// <summary>
        /// Loads the body.
        /// <br></br>
        /// If the underlying address is not empty, it'll try to load the asset via the body catalog.
        /// </summary>
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

        /// <summary>
        /// Implicitly converts this into a boolean, converts to true if the asset is not null
        /// </summary>
        public static implicit operator bool(AddressReferencedBody addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        /// <summary>
        /// Implicitly converts this into a GameObject
        /// </summary>
        public static implicit operator GameObject?(AddressReferencedBody addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        /// <summary>
        /// Implicitly encapsulates <paramref name="address"/> into an instance of AddressReferencedBody
        /// </summary>
        public static implicit operator AddressReferencedBody(string address)
        {
            return new AddressReferencedBody(address);
        }

        /// <summary>
        /// Implicitly encapsulates <paramref name="asset"/> into an instance of AddressReferencedBody
        /// </summary>
        /// <param name="asset"></param>
        public static implicit operator AddressReferencedBody(GameObject asset)
        {
            return new AddressReferencedBody(asset);
        }

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public AddressReferencedBody() : base() { }

        /// <summary>
        /// Constructor, sets the underlying asset value to <paramref name="asset"/>
        /// </summary>
        public AddressReferencedBody(GameObject asset) : base(asset) { }

        /// <summary>
        /// Constructor, sets the underlying address value to <paramref name="addressOrName"/>
        /// </summary>
        public AddressReferencedBody(string addressOrName) : base(addressOrName) { }
    }
}