using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> whose name is cached to avoid allocations.
    /// </summary>
    public class CachedNameScriptableObject : ScriptableObject
    {
        /// <summary>
        /// Obsolete, use <see cref="cachedName"/> instead
        /// </summary>
        [Obsolete("\".name\" should not be used. Use \".cachedName\" instead. If retrieving the value from the engine is absolutely necessary, cast to ScriptableObject first.", error: true)]
        public new string name
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The cached name for this scriptable object
        /// </summary>
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

        /// <summary>
        /// Base method assigns <see cref="_cachedName"/> to base.name
        /// </summary>
        protected virtual void Awake()
        {
            _cachedName = base.name;
        }

        /// <summary>
        /// Base method assigns <see cref="_cachedName"/> to base.name
        /// </summary>
        protected virtual void OnValidate()
        {
            _cachedName = base.name;
        }

        /// <summary>
        /// Creates an instance of a <see cref="CachedNameScriptableObject"/>
        /// </summary>
        /// <typeparam name="T">The instance of <see cref="CachedNameScriptableObject"/> to create</typeparam>
        /// <param name="name">The name for the CachedNameScriptableObject</param>
        public static T CreateInstance<T>(string name) where T : CachedNameScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            instance.cachedName = name;
            return instance;
        }
    }
}