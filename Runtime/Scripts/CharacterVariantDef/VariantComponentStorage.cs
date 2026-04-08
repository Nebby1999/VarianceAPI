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
    [Serializable]
    public sealed class VariantComponentCollection
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

        [SerializableSystemType.RequiredBaseType(typeof(VariantComponent))]
        public SerializableSystemType[] variantComponents = Array.Empty<SerializableSystemType>();

        private ComponentTypeData[] _serverComponents = Array.Empty<ComponentTypeData>();
        private ComponentTypeData[] _clientComponents = Array.Empty<ComponentTypeData>();
        private ComponentTypeData[] _sharedComponents = Array.Empty<ComponentTypeData>();

        private bool _initialized;
        internal void Initialize()
        {
            List<ComponentTypeData> serverComponents = new List<ComponentTypeData>();
            List<ComponentTypeData> clientComponents = new List<ComponentTypeData>();
            List<ComponentTypeData> sharedComponents = new List<ComponentTypeData>();

            if (_initialized)
                return;

            _initialized = true;
        
            for(int i = 0; i < variantComponents.Length; i++)
            {
                SerializableSystemType serializableSystemType = variantComponents[i];

                Type componentType = (Type)serializableSystemType;
                if (componentType == null)
                    continue;

                if (!componentType.IsSubclassOf(typeof(VariantComponent)))
                    continue;

                VariantComponent.TargetComponentObjectAttribute? attribute = componentType.GetCustomAttribute<VariantComponent.TargetComponentObjectAttribute>();
                if (attribute == null)
                    continue;

                var targetObject = attribute.targetObject;

                if(attribute.useOnServer)
                {
                    serverComponents.Add(new ComponentTypeData { targetObject = targetObject, componentType = componentType });
                }
                if(attribute.useOnClient)
                {
                    clientComponents.Add(new ComponentTypeData { targetObject = targetObject, componentType = componentType });
                }
                if (attribute.useOnServer || attribute.useOnClient)
                {
                    sharedComponents.Add(new ComponentTypeData { targetObject = targetObject, componentType = componentType });
                }
            }

            _serverComponents = serverComponents.ToArray();
            _clientComponents = clientComponents.ToArray();
            _sharedComponents = sharedComponents.ToArray();
        }

        public IDisposable? ApplyComponents(CharacterMaster master)
        {
            if (!master)
                return null;

            var components = GetComponentTypeDataBasedOnNetworkContext();
            return ApplyComponentsWithTargetObjectMatch(components, master.gameObject, VariantComponent.TargetComponentObjectAttribute.TargetObject.CharacterMaster);
        }

        public IDisposable? ApplyComponents(CharacterBody body)
        {
            if (!body)
                return null;

            var components = GetComponentTypeDataBasedOnNetworkContext();
            return ApplyComponentsWithTargetObjectMatch(components, body.gameObject, VariantComponent.TargetComponentObjectAttribute.TargetObject.CharacterBody);
        }

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
            for(int i = 0; i < componentTypeDatas.Length; i++)
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

            if(serverActive)
            {
                if(clientActive)
                {
                    return _sharedComponents;
                }
                return _serverComponents;
            }
            if(clientActive)
            {
                return _clientComponents;
            }

            throw new InvalidOperationException("Neither server nor client is running.");
        }

        public VariantComponentCollection(SerializableSystemType[] variantComponentTypes)
        {
            variantComponents = variantComponentTypes;
        }
        public VariantComponentCollection() { }
    }
}