using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections;
using UnityEngine;

namespace VAPI.Addressables
{
    public class AddressReferencedCharacterMaster : AddressReferencedPrefab
    {
        public override bool CanLoadFromCatalog { get => _canLoadFromCatalog; protected set => _canLoadFromCatalog = value; }

        [SerializeField, HideInInspector]
        private bool _canLoadFromCatalog = true;

        protected override IEnumerator LoadAsyncCoroutine()
        {
            if (CanLoadFromCatalog)
            {
                GameObject masterPrefab = MasterCatalog.FindMasterPrefab(Address);
                if(masterPrefab)
                {
                    Asset = masterPrefab;
                    yield break;
                }
            }
            var subroutine = LoadFromAddressAsyncCoroutine();
            while (subroutine.MoveNext())
            {
                yield return null;
            }
        }

        protected override void Load()
        {
            if (CanLoadFromCatalog)
            {
                GameObject masterPrefab = MasterCatalog.FindMasterPrefab(Address);
                if (masterPrefab)
                {
                    Asset = masterPrefab;
                    return;
                }
            }
            LoadFromAddress();
        }

        /// <summary>
        /// Operator for casting <see cref="AddressReferencedCharacterMaster"/> to a boolean value
        /// <br>Allows you to keep using the unity Syntax for checking if an object exists.</br>
        /// </summary>
        public static implicit operator bool(AddressReferencedCharacterMaster addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        /// <summary>
        /// Operator for casting <see cref="AddressReferencedCharacterMaster"/> to it's currently loaded <see cref="AddressReferencedAsset{T}.Asset"/> value
        /// </summary>
        public static implicit operator GameObject(AddressReferencedCharacterMaster addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        /// <summary>
        /// Operator for encapsulating a <see cref="string"/> inside an <see cref="AddressReferencedCharacterMaster"/>
        /// </summary>
        public static implicit operator AddressReferencedCharacterMaster(string address)
        {
            return new AddressReferencedCharacterMaster(address);
        }

        /// <summary>
        /// Operator for encapsulating an <see cref="GameObject"/> inside an <see cref="AddressReferencedCharacterMaster"/>
        /// </summary>
        public static implicit operator AddressReferencedCharacterMaster(GameObject asset)
        {
            return new AddressReferencedCharacterMaster(asset);
        }

        /// <summary>
        /// <inheritdoc cref="AddressReferencedAsset{T}.AddressReferencedAsset()"/>
        /// <br>T is <see cref="GameObject"/></br>
        /// </summary>
        public AddressReferencedCharacterMaster() : base() { }

        /// <summary>
        /// <inheritdoc cref="AddressReferencedAsset{T}.AddressReferencedAsset(T)"/>
        /// <br>T is <see cref="GameObject"/></br>
        /// </summary>
        public AddressReferencedCharacterMaster(GameObject def) : base(def) { }

        /// <summary>
        /// <inheritdoc cref="AddressReferencedAsset{T}.AddressReferencedAsset(string)"/>
        /// <br>T is <see cref="GameObject"/></br>
        /// </summary>
        public AddressReferencedCharacterMaster(string addressOrName) : base(addressOrName) { }
    }
}