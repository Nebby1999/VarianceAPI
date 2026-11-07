#nullable enable
using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// Class that represents a collection of <see cref="VariantComponent"/>s to apply to a variant. Can be cloned
    /// </summary>
    [Serializable]
    public sealed class VariantComponentCollection : ICloneable
    {
        private struct DisposableVariantComponentCollectionModification : IDisposable
        {
            public bool isEmpty => _addedComponents == null || _addedComponents.Count == 0;
            private List<Component>? _addedComponents;

            public void AddComponent(Component component)
            {
                _addedComponents ??= new List<Component>();
                _addedComponents.Add(component);
            }

            public void Dispose()
            {
                if (_addedComponents == null)
                    return;

                for (int i = _addedComponents.Count - 1; i >= 0; i--)
                {
                    if (_addedComponents[i])
                        UnityEngine.Component.Destroy(_addedComponents[i]);
                }
            }
        }
        private struct ComponentTypeData
        {
            public Type componentType;
            public VariantComponent.TargetComponentObjectAttribute.TargetObject targetObject;
        }

        [Tooltip("The VariantComponents that are stored in this.")]
        [SerializableSystemType.RequiredBaseType(typeof(VariantComponent))]
        public SerializableSystemType[] variantComponents = Array.Empty<SerializableSystemType>();

        private ComponentTypeData[] _serverComponents = Array.Empty<ComponentTypeData>();
        private ComponentTypeData[] _clientComponents = Array.Empty<ComponentTypeData>();
        private ComponentTypeData[] _sharedComponents = Array.Empty<ComponentTypeData>();

        private bool _initialized;
        private static List<ComponentTypeData> _initializationServerComponents = new List<ComponentTypeData>();
        private static List<ComponentTypeData> _initializationClientComponents = new List<ComponentTypeData>();
        private static List<ComponentTypeData> _initializationSharedComponents = new List<ComponentTypeData>();
        //Initializes this collection and fills the component type data arrays.
        internal void Initialize()
        {
            if (_initialized)
                return;

            _initializationServerComponents.Clear();
            _initializationClientComponents.Clear();
            _initializationSharedComponents.Clear();

            _initialized = true;

            for (int i = 0; i < variantComponents.Length; i++)
            {
                SerializableSystemType serializableSystemType = variantComponents[i];

                Type componentType = (Type)serializableSystemType;
                if (componentType == null)
                {
                    VAPILog.Warning($"SerializableSystemType at index {i} failed to resolve into a Type.");
                    continue;
                }

                if (!componentType.IsSubclassOf(typeof(VariantComponent)))
                {
                    VAPILog.Warning($"SerializableSystemType at index {i} does not represent a VariantComponent");
                    continue;
                }

                if (componentType.IsAbstract)
                {
                    VAPILog.Warning($"SerializableSystemType at index {i} represents a VariantComponent that's abstract, this is not allowed.");
                    continue;
                }

                VariantComponent.TargetComponentObjectAttribute? attribute = componentType.GetCustomAttribute<VariantComponent.TargetComponentObjectAttribute>();
                if (attribute == null)
                {
                    VAPILog.Warning($"SerializableSystemType at index {i} represents a VariantComponent Type that lacks a TargetComponentObjectAttribute. This is not allowed.");
                    continue;
                }

                var targetObject = attribute.targetObject;

                if (attribute.useOnClient == false && attribute.useOnServer == false)
                {
                    VAPILog.Warning($"SerialiableSystemType at index {i} represents a VariantComponent Type. However it's TargetComponentObjectAttribute has useOnClient and useOnServer set to false! This component will not be added to objects at all.");
                    continue;
                }

                if (attribute.useOnServer || attribute.useOnClient)
                {
                    _initializationSharedComponents.Add(new ComponentTypeData { targetObject = targetObject, componentType = componentType });
                }
                if (attribute.useOnServer)
                {
                    _initializationServerComponents.Add(new ComponentTypeData { targetObject = targetObject, componentType = componentType });
                }
                if (attribute.useOnClient)
                {
                    _initializationClientComponents.Add(new ComponentTypeData { targetObject = targetObject, componentType = componentType });
                }
            }

            _serverComponents = _initializationServerComponents.ToArray();
            _clientComponents = _initializationClientComponents.ToArray();
            _sharedComponents = _initializationSharedComponents.ToArray();
        }

        /// <summary>
        /// Applies the components for a master stored in this to <paramref name="master"/>.
        /// <br></br>
        /// Respects networking context. Server only components will be added only to the server, and client only components only to the clients. "Host" machines will get both, as they're considered both Server and Client
        /// <para></para>
        /// Returns null if <paramref name="master"/> is null. Otherwise returns a Disposable object used to <b>destroy</b> all added components.
        /// </summary>
        public IDisposable? ApplyComponents(CharacterMaster master)
        {
            if (!master)
                return null;

            var components = GetComponentTypeDataBasedOnNetworkContext();
            return ApplyComponentsWithTargetObjectMatch(components, master.gameObject, VariantComponent.TargetComponentObjectAttribute.TargetObject.CharacterMaster);
        }

        /// <summary>
        /// Applies the components for a body stored in this to <paramref name="body"/>.
        /// <br></br>
        /// Respects networking context. Server only components will be added only to the server, and client only components only to the clients. "Host" machines will get both, as they're considered both Server and Client
        /// <para></para>
        /// Returns null if <paramref name="body"/> is null. Otherwise returns a Disposable object used to <b>destroy</b> all added components.
        /// </summary>
        public IDisposable? ApplyComponents(CharacterBody body)
        {
            if (!body)
                return null;

            var components = GetComponentTypeDataBasedOnNetworkContext();
            return ApplyComponentsWithTargetObjectMatch(components, body.gameObject, VariantComponent.TargetComponentObjectAttribute.TargetObject.CharacterBody);
        }

        /// <summary>
        /// Applies the components for a model stored in this to <paramref name="model"/>.
        /// <br></br>
        /// Respects networking context. Server only components will be added only to the server, and client only components only to the clients. "Host" machines will get both, as they're considered both Server and Client
        /// <para></para>
        /// Returns null if <paramref name="model"/> is null. Otherwise returns a Disposable object used to <b>destroy</b> all added components.
        /// </summary>
        public IDisposable? ApplyComponents(CharacterModel model)
        {
            if (!model)
                return null;

            var components = GetComponentTypeDataBasedOnNetworkContext();
            return ApplyComponentsWithTargetObjectMatch(components, model.gameObject, VariantComponent.TargetComponentObjectAttribute.TargetObject.CharacterModel);
        }

        private IDisposable? ApplyComponentsWithTargetObjectMatch(ComponentTypeData[] componentTypeDatas, GameObject targetObject, VariantComponent.TargetComponentObjectAttribute.TargetObject targetObjectValue)
        {
            if (componentTypeDatas == null || !targetObject)
                return null;

            DisposableVariantComponentCollectionModification disposableModification = new DisposableVariantComponentCollectionModification();
            for (int i = 0; i < componentTypeDatas.Length; i++)
            {
                if (componentTypeDatas[i].targetObject == targetObjectValue)
                {
                    Component addedComponent = targetObject.AddComponent(componentTypeDatas[i].componentType);
                    disposableModification.AddComponent(addedComponent);
                }
            }

            return disposableModification.isEmpty ? null : disposableModification;
        }

        private ComponentTypeData[] GetComponentTypeDataBasedOnNetworkContext()
        {
            bool serverActive = NetworkServer.active;
            bool clientActive = NetworkClient.active;

            if (serverActive)
            {
                if (clientActive)
                {
                    return _sharedComponents;
                }
                return _serverComponents;
            }
            if (clientActive)
            {
                return _clientComponents;
            }

            throw new InvalidOperationException("Neither server nor client is running.");
        }

        /// <summary>
        /// Clones the metadata in this into a new instance of <see cref="VariantComponentCollection"/>
        /// </summary>
        public object Clone()
        {
            var result = new VariantComponentCollection();
            HG.ArrayUtils.CloneTo(variantComponents, ref result.variantComponents);
            return result;
        }

        #region Constructors
        /// <summary>
        /// Constructor. Initializes a new component collection with the data stored in <paramref name="variantComponentTypes"/>
        /// </summary>
        public VariantComponentCollection(SerializableSystemType[] variantComponentTypes)
        {
            variantComponents = variantComponentTypes;
        }

        /// <summary>
        /// Constructor. Initializes a new component collection with the data stored in <paramref name="variantComponentType"/>
        /// </summary>
        public VariantComponentCollection(SerializableSystemType variantComponentType)
        {
            variantComponents = new SerializableSystemType[1] { variantComponentType };
        }

        /// <summary>
        /// Parameterless Constructor
        /// </summary>
        public VariantComponentCollection() { }

        /// <summary>
        /// Creates a new component collection with a single component, which is identified in <typeparamref name="T"/>
        /// </summary>
        public static VariantComponentCollection Create<T>() where T : VariantComponent => new VariantComponentCollection((SerializableSystemType)typeof(T));
        private static List<SerializableSystemType> _createParamsTypeHelper = new List<SerializableSystemType>();

        /// <summary>
        /// Creates a new component collection using the types stored in <paramref name="variantComponentTypes"/>
        /// </summary>
        public static VariantComponentCollection Create(params Type[] variantComponentTypes)
        {
            _createParamsTypeHelper.Clear();
            foreach (Type type in variantComponentTypes)
            {
                if (type.IsSubclassOf(typeof(VariantComponent)))
                {
                    _createParamsTypeHelper.Add((SerializableSystemType)type);
                }
            }
            return new VariantComponentCollection(_createParamsTypeHelper.ToArray());
        }
        #endregion
    }
}