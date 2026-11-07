using HG;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A runtime component that's added to a Character that needs to have a specific scale. You do not need to add this component manually.
    /// <para></para>
    /// Some vanilla characters have their model locators disabled or their autoUpdateModelTransform set to false, in scenarios where this is the case, VAPI adds this component to the character which will manually drive the size change.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class ModelLocatorSizeUpdater : MonoBehaviour
    {
        private ModelLocator _modelLocator;
        private Vector3 _origScale;
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
            if (!_modelLocator)
                return;

            if(_modelLocator.enabled == false || _modelLocator.autoUpdateModelTransform == false)
            {
                UpdateModelLocatorSize(Time.deltaTime);
            }
        }

        private void UpdateModelLocatorSize(float deltaTime)
        {
            if (_modelLocator.modelTransform == null)
                return;

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