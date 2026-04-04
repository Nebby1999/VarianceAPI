#nullable enable
using HG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace VAPI
{
    public sealed class NetworkedVariantCollection : IEnumerable<CharacterVariantDef>
    {
        private struct Enumerator : IEnumerator<CharacterVariantDef>
        {
            public Enumerator(ReadOnlyArray<CharacterVariantDef> characterVariantDefs)
            {
                _index = -1;
                _src = characterVariantDefs;
            }

            public CharacterVariantDef Current => _src[_index];

            object IEnumerator.Current => Current;

            private int _index;
            private ReadOnlyArray<CharacterVariantDef> _src;

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                _index++;
                return _index < _src.Length;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
        public CharacterVariantDef this[int index] => characterVariantDefs[index];
        public int count => characterVariantDefs.Length;
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

        public IEnumerator<CharacterVariantDef> GetEnumerator()
        {
            return new Enumerator(characterVariantDefs);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}