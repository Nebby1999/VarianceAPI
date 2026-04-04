#nullable enable
using HG;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace VAPI
{
    //Stores the CharacterVariantDefs for this master, the CharacterBodyVariantController will attempt to link itself to this, if the link is successful, then this is the single source of truth for which variants to use
    public class CharacterMasterVariantStorage : NetworkBehaviour
    {
        public const uint variantsDirtyBit = (1 << 0);
        public const uint doNotRollForVariantsDirtyBit = (1 << 1);
        public const uint allDirtyBits = variantsDirtyBit | doNotRollForVariantsDirtyBit;

        public CharacterMaster characterMaster { get; private set; }
        public NetworkedVariantCollection variantsForCharacter { get; private set; } = new NetworkedVariantCollection();

        public List<ItemCountPair> _channeledItemCountPair = new List<ItemCountPair>();

        public bool doNotRollForVariants
        {
            get => _doNotRollForVariants;
            [Server]
            set
            {
                if(_doNotRollForVariants != value)
                {
                    _doNotRollForVariants = value;
                    SetDirtyBit(doNotRollForVariantsDirtyBit);
                }
            }
        }
        private bool _doNotRollForVariants;

        private void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
        }

        [Server]
        public void SetVariantDefsForCharacter(CharacterVariantDef[] characterVariantDefs)
        {
            UnapplyMasterModifications(variantsForCharacter.characterVariantDefs);

            variantsForCharacter.SetVariantServer(characterVariantDefs);
            SetDirtyBit(variantsDirtyBit);

            ApplyMasterModifications(variantsForCharacter.characterVariantDefs);
        }

        private List<IUndoable?> undoableModifications = new List<IUndoable>();
        private void UnapplyMasterModifications(ReadOnlyArray<CharacterVariantDef> variantDefs)
        {
            for (int i = variantDefs.Length - 1; i >= 0; i--)
            {
                //Apply channeled items && equipment info
                variantDefs[i].inventoryDefinition.UnapplyToInventory(characterMaster.inventory);
            }

            //Undo modifiers
            for(int i = undoableModifications.Count - 1; i >= 0; i--)
            {
                undoableModifications[i]?.Undo();
            }
            undoableModifications.Clear();
        }

        private void ApplyMasterModifications(ReadOnlyArray<CharacterVariantDef> variantDefs)
        {
            for (int i = 0; i < variantDefs.Length; i++)
            {
                //Apply channeled items && equipment info
                variantDefs[i].inventoryDefinition.ApplyToInventory(characterMaster.inventory);

                //Apply modifiers
                for (int j = 0; j < variantDefs[i].masterModifiers.Length; j++)
                {
                    undoableModifications.Add(variantDefs[i].masterModifiers[j]?.ModifyMaster(characterMaster));
                }
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            uint dirtyBits = syncVarDirtyBits;

            if(initialState)
            {
                dirtyBits = allDirtyBits;
            }

            bool writeVariantArray = (dirtyBits & variantsDirtyBit) != 0;
            bool writeDoNotRollForVariants = (dirtyBits & doNotRollForVariantsDirtyBit) != 0;

            writer.Write((byte)dirtyBits);

            if(writeVariantArray)
            {
                variantsForCharacter.Serialize(writer);
            }

            if(writeDoNotRollForVariants)
            {
                writer.Write(doNotRollForVariants);
            }

            return ((!initialState) && dirtyBits != 0u);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            byte mainMask = reader.ReadByte();

            bool readVariantArray = (mainMask & variantsDirtyBit) != 0;
            bool readDoNotRollForVariants = (mainMask & doNotRollForVariantsDirtyBit) != 0;

            if(readVariantArray)
            {
                variantsForCharacter.Deserialize(reader);
            }

            if(readDoNotRollForVariants)
            {
                _doNotRollForVariants = reader.ReadBoolean();
            }
        }
    }
}