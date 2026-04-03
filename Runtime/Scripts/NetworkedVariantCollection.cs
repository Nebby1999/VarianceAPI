#nullable enable
using HG;
using System;
using UnityEngine.Networking;

namespace VAPI
{
    public sealed class NetworkedVariantCollection
    {
        public ReadOnlyArray<CharacterVariantDef> characterVariantDefs => _characterVariantDefs;
        private CharacterVariantDef[] _characterVariantDefs = Array.Empty<CharacterVariantDef>();

        public void SetVariantServer(CharacterVariantDef[] newVariantDefs)
        {
            if(NetworkServer.active)
            {
                return;
            }

            _characterVariantDefs = newVariantDefs;
        }

        public void Serialize(NetworkWriter writer)
        {
            int count = _characterVariantDefs.Length;
            writer.Write(count);
            for(int i = 0; i < count; i++)
            {
                writer.Write(_characterVariantDefs[i].characterVariantIndex);
            }
        }

        public void Deserialize(NetworkReader reader)
        {
            int count = reader.ReadInt32();
            _characterVariantDefs = new CharacterVariantDef[count];
            for(int i = 0; i < count; i++)
            {
                _characterVariantDefs[i] = CharacterVariantManager.GetCharacterVariantDef(reader.ReadCharacterVariantIndex())!;
            }
        }
    }
}