using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections;
using UnityEngine;

namespace VAPI.Addressables
{
    public class AddressReferencedCharacterBody : AddressReferencedPrefab
    {
        public override bool CanLoadFromCatalog { get => _canLoadFromCatalog; protected set => _canLoadFromCatalog = value; }

        [SerializeField, HideInInspector]
        private bool _canLoadFromCatalog = true;

        protected override IEnumerator LoadAsyncCoroutine()
        {
            if (CanLoadFromCatalog)
            {
                GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(Address);
                if(bodyPrefab)
                {
                    Asset = bodyPrefab;
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
                GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(Address);
                if (bodyPrefab)
                {
                    Asset = bodyPrefab;
                    return;
                }
            }
            LoadFromAddress();
        }

        /// <summary>
        /// Operator for casting <see cref="AddressReferencedCharacterBody"/> to a boolean value
        /// <br>Allows you to keep using the unity Syntax for checking if an object exists.</br>
        /// </summary>
        public static implicit operator bool(AddressReferencedCharacterBody addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        /// <summary>
        /// Operator for casting <see cref="AddressReferencedCharacterBody"/> to it's currently loaded <see cref="AddressReferencedAsset{T}.Asset"/> value
        /// </summary>
        public static implicit operator GameObject(AddressReferencedCharacterBody addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        /// <summary>
        /// Operator for encapsulating a <see cref="string"/> inside an <see cref="AddressReferencedCharacterBody"/>
        /// </summary>
        public static implicit operator AddressReferencedCharacterBody(string address)
        {
            return new AddressReferencedCharacterBody(address);
        }

        /// <summary>
        /// Operator for encapsulating an <see cref="GameObject"/> inside an <see cref="AddressReferencedCharacterBody"/>
        /// </summary>
        public static implicit operator AddressReferencedCharacterBody(GameObject asset)
        {
            return new AddressReferencedCharacterBody(asset);
        }

        /// <summary>
        /// <inheritdoc cref="AddressReferencedAsset{T}.AddressReferencedAsset()"/>
        /// <br>T is <see cref="GameObject"/></br>
        /// </summary>
        public AddressReferencedCharacterBody() : base() { }

        /// <summary>
        /// <inheritdoc cref="AddressReferencedAsset{T}.AddressReferencedAsset(T)"/>
        /// <br>T is <see cref="GameObject"/></br>
        /// </summary>
        public AddressReferencedCharacterBody(GameObject def) : base(def) { }

        /// <summary>
        /// <inheritdoc cref="AddressReferencedAsset{T}.AddressReferencedAsset(string)"/>
        /// <br>T is <see cref="GameObject"/></br>
        /// </summary>
        public AddressReferencedCharacterBody(string addressOrName) : base(addressOrName) { }
    }
}