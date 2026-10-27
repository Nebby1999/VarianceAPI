using HG;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VAPI
{

    [AddComponentMenu("")]
    public sealed class ModelLocatorSizeUpdater : MonoBehaviour
    {
        ModelLocator _modelLocator;
        public Vector3 _origScale;
        private void Awake()
        {
            _modelLocator = GetComponent<ModelLocator>();
        }

        private void Start()
        {
            _origScale = _modelLocator.initialScale;
        }

        private void OnDestroy()
        {
            if(_modelLocator != null && _modelLocator.modelTransform != null)
            {
                _modelLocator.modelTransform.localScale = _origScale;
            }
        }

        private void LateUpdate()
        {
            if(_modelLocator.enabled == false || _modelLocator.autoUpdateModelTransform == false)
            {
                UpdateModelLocatorSize(Time.deltaTime);
            }
        }

        private void UpdateModelLocatorSize(float deltaTime)
        {
            if (_modelLocator.modelScale != _modelLocator.desiredModelScale)
            {
                _modelLocator.modelScale = _modelLocator.desiredModelScale;
                Vector3 localScale = _modelLocator.initialScale * _modelLocator.modelScale;
                Transform transform = _modelLocator.modelBaseTransform.AsValidOrNull() ?? base.transform;
                Transform transform2 = _modelLocator.modelTransform.parent.AsValidOrNull();
                if (transform2 != transform)
                {
                    Vector3 b = transform2?.lossyScale ?? Vector3.one;
                    localScale.Scale(transform.lossyScale.ComponentDiv(in b));
                }
                _modelLocator.modelTransform.localScale = localScale;
            }
        }
    }
}