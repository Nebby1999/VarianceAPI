#nullable enable
using HG;
using RoR2;
using System;
using UnityEngine.Networking;

namespace VAPI
{
    public class CharacterBodyVariantController : NetworkBehaviour
    {
        public const uint fallbackVariantsDirtyBit = (1 << 0);
        public const uint doNotRollForVariantsDirtyBit = (1 << 1);
        public const uint allDirtyBits = fallbackVariantsDirtyBit | doNotRollForVariantsDirtyBit;

        //So, VAPI 3.0 has the ability to store the variants on a master, however, we want to allow the ability for masterless variants to be a thing.
        //As a result, the main variants for the body _are_ the ones found on the master, if said master storage is not found then it must utilize it's internal storage.
        //In terms of networking, the "Source of Truth" is the Master, if no master, then its this component.
        public NetworkedVariantCollection variantsForBody
        {
            get
            {
                if(characterMasterVariantStorage)
                {
                    //return characterMasterVariantStorage!.variantsForCharacter;
                }

                return _fallbackVariantStorage;
            }
        }
        private NetworkedVariantCollection _fallbackVariantStorage = new NetworkedVariantCollection();
        public CharacterMasterVariantStorage? characterMasterVariantStorage { get; private set; }
        public CharacterBody characterBody { get; private set; }

        public bool doNotRollForVariants
        {
            get => _doNotRollForVariants;
            [Server]
            set
            {
                if (_doNotRollForVariants != value)
                {
                    _doNotRollForVariants = value;
                    SetDirtyBit(doNotRollForVariantsDirtyBit);
                }
            }
        }
        private bool _doNotRollForVariants;

        public event Action<NetworkedVariantCollection>? onBecameVariantGlobal;

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
        }

        private void Start()
        {
            TryLinkCharacterMasterVariantStorage();
            Apply();
        }

        private bool _hasApplied;
        public void Apply()
        {
            if(_hasApplied)
            {
                VAPILog.Warning($"Cannot apply variants to a CharacterBody twice.");
                return;
            }

            _hasApplied = true;
            onBecameVariantGlobal?.Invoke(variantsForBody);
        }

        public bool TryLinkCharacterMasterVariantStorage()
        {
            //Shortcircuit to true if we already have the master's variant storage.
            if(characterMasterVariantStorage)
            {
                return true;
            }

            if(!characterBody.master)
            {
                return false;
            }

            if(!characterBody.master.TryGetComponent<CharacterMasterVariantStorage>(out var masterVariantStorage))
            {
                return false;
            }

            if(masterVariantStorage.doNotRollForVariants)
            {
                //Should "cannotBeVariant" be inherited from the master's variant storage?...
                if(NetworkServer.active)
                {
                    doNotRollForVariants = masterVariantStorage.doNotRollForVariants;
                }
                return false;
            }

            //We've obtained our master's storage, so assign it here.
            characterMasterVariantStorage = masterVariantStorage;
            return true;
        }

        [Server]
        public void SetFallbackVariants(CharacterVariantDef[] fallbackVariants)
        {
            if(characterMasterVariantStorage)
            {
                //TODO: Log this situation, which shouldn't happen.
                return;
            }

            _fallbackVariantStorage.SetVariantServer(fallbackVariants);
            SetDirtyBit(fallbackVariantsDirtyBit);
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            uint dirtyBits = syncVarDirtyBits;

            if (initialState)
            {
                dirtyBits = allDirtyBits;
            }

            bool writeFallbackVariants = (dirtyBits & fallbackVariantsDirtyBit) != 0;
            bool writeDoNotRollForVariants = (dirtyBits & doNotRollForVariantsDirtyBit) != 0;

            writer.Write((byte)dirtyBits);

            if (writeFallbackVariants)
            {
                _fallbackVariantStorage.Serialize(writer);
            }

            if (writeDoNotRollForVariants)
            {
                writer.Write(doNotRollForVariants);
            }

            return ((!initialState) && dirtyBits != 0u);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            byte mainMask = reader.ReadByte();

            bool readFallbackVariants = (mainMask & fallbackVariantsDirtyBit) != 0;
            bool readDoNotRollForVariants = (mainMask & doNotRollForVariantsDirtyBit) != 0;

            if (readFallbackVariants)
            {
                _fallbackVariantStorage.Deserialize(reader);
            }

            if (readDoNotRollForVariants)
            {
                _doNotRollForVariants = reader.ReadBoolean();
            }
        }
    }
}