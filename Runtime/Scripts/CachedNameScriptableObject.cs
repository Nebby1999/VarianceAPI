using System;
using UnityEngine;

namespace VAPI
{
    public class CachedNameScriptableObject : ScriptableObject
    {
        [Obsolete("\".name\" should not be used. Use \".cachedName\" instead. If retrieving the value from the engine is absolutely necessary, cast to ScriptableObject first.", error: true)]
        public new string name
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string cachedName
        {
            get => _cachedName;
            set
            {
                if(string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("The value set to cachedName cannot be null, empty or whitespace.");
                }
                _cachedName = value;
                base.name = value;
            }
        }
        private string _cachedName;

        protected virtual void Awake()
        {
            _cachedName = base.name;
        }

        protected virtual void OnValidate()
        {
            _cachedName = base.name;
        }
    }
}