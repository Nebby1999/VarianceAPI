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
    public sealed class VariantComponentStorage : IDisposable
    {
        private List<Component> _addedComponents = new List<Component>();
        private List<Type> _componentsToAdd = new List<Type>();
        private GameObject? _targetObject;
        private bool _applied;

        public void Dispose()
        {
            for (int i = _addedComponents.Count - 1; i >= 0; i--)
            {
                if (_addedComponents[i])
                {
                    UnityEngine.Object.Destroy(_addedComponents[i]);
                }
            }
            _addedComponents.Clear();
            _componentsToAdd.Clear();
            _targetObject = null;
        }

        public void ApplyComponents()
        {
            if (!_targetObject || _applied)
                return;

            _applied = true;
            for(int i = 0; i < _componentsToAdd.Count; i++)
            {
                _targetObject!.AddComponent(_componentsToAdd[i]);
            }
        }

        public VariantComponentStorage(CharacterBody body, ReadOnlyArray<CharacterVariantDef> characterVariantDefs)
        {
            _targetObject = body.gameObject;
            FilterAndAddComponentsWithTarget(_componentsToAdd, characterVariantDefs, VariantComponent.TargetComponentObjectAttribute.TargetObject.CharacterBody);
        }

        public VariantComponentStorage(CharacterMaster master, ReadOnlyArray<CharacterVariantDef> characterVariantDefs)
        {
            _targetObject = master.gameObject;
            FilterAndAddComponentsWithTarget(_componentsToAdd, characterVariantDefs, VariantComponent.TargetComponentObjectAttribute.TargetObject.CharacterMaster);
        }

        public VariantComponentStorage(CharacterModel model, ReadOnlyArray<CharacterVariantDef> characterVariantDefs)
        {
            _targetObject = model.gameObject;
            FilterAndAddComponentsWithTarget(_componentsToAdd, characterVariantDefs, VariantComponent.TargetComponentObjectAttribute.TargetObject.CharacterModel);
        }

        private void FilterAndAddComponentsWithTarget(List<Type> target, ReadOnlyArray<CharacterVariantDef> variantDefs, VariantComponent.TargetComponentObjectAttribute.TargetObject targetObject)
        {
            for (int i = 0; i < variantDefs.Length; i++)
            {
                for (int j = 0; j < variantDefs[i].additionalComponents.Length; j++)
                {
                    SerializableSystemType serializableType = variantDefs[i].additionalComponents[j];
                    Type type = (Type)serializableType;
                    if (type == null)
                    {
                        continue;
                    }

                    VariantComponent.TargetComponentObjectAttribute? attribute = type.GetCustomAttribute<VariantComponent.TargetComponentObjectAttribute>();
                    if (attribute == null)
                        continue;

                    if (attribute.targetObject == targetObject)
                    {
                        if (attribute.forServer && NetworkServer.active)
                            target.Add(type);
                        else if (attribute.forClient && NetworkClient.active)
                            target.Add(type);
                        else
                            target.Add(type);
                    }
                }
            }
        }
    }
}