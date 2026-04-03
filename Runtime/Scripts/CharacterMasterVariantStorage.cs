#nullable enable
using HG;
using RoR2;
using System;
using UnityEngine.Networking;

namespace VAPI
{
    //Stores the CharacterVariantDefs for this master, the CharacterBodyVariantController will attempt to link itself to this, if the link is successful, then this is the single source of truth for which variants to use
    public class CharacterMasterVariantStorage : NetworkBehaviour
    {
        public const uint variantsDirtyBit = (1 << 0);
        public const uint cannotBeVariantDirtyBit = (1 << 1);
        public const uint allDirtyBits = variantsDirtyBit | cannotBeVariantDirtyBit;
        public CharacterMaster characterMaster { get; private set; }

        public NetworkedVariantCollection variantsForCharacter { get; private set; } = new NetworkedVariantCollection();

        public bool cannotBeVariant
        {
            get => _cannotBeVariant;
            [Server]
            set
            {
                if(_cannotBeVariant != value)
                {
                    _cannotBeVariant = value;
                    SetDirtyBit(cannotBeVariantDirtyBit);
                }
            }
        }
        public bool _cannotBeVariant;

        private void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
        }

        [Server]
        public void SetVariantDefsForCharacter(CharacterVariantDef[] characterVariantDefs)
        {
            variantsForCharacter.SetVariantServer(characterVariantDefs);
            SetDirtyBit(variantsDirtyBit);
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            uint dirtyBits = syncVarDirtyBits;

            if(initialState)
            {
                dirtyBits = allDirtyBits;
            }

            bool writeVariantArray = (dirtyBits & variantsDirtyBit) != 0;
            bool writeCannotBeVariant = (dirtyBits & cannotBeVariantDirtyBit) != 0;

            writer.Write((byte)dirtyBits);

            if(writeVariantArray)
            {
                variantsForCharacter.Serialize(writer);
            }

            if(writeCannotBeVariant)
            {
                writer.Write(cannotBeVariant);
            }

            return ((!initialState) && dirtyBits != 0u);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            byte mainMask = reader.ReadByte();

            bool readVariantArray = (mainMask & variantsDirtyBit) != 0;
            bool readCannotBeVariant = (mainMask & cannotBeVariantDirtyBit) != 0;

            if(readVariantArray)
            {
                variantsForCharacter.Deserialize(reader);
            }

            if(readCannotBeVariant)
            {
                _cannotBeVariant = reader.ReadBoolean();
            }
        }
    }
}