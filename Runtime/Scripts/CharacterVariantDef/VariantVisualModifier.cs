#nullable enable
using MSU;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VAPI
{
    [CreateAssetMenu(fileName = "new VariantVisualModifier", menuName = "VarianceAPI/VariantVisualModifier")]
    public class VariantVisualModifier : ScriptableObject
    {
        #region Subclasses
        [Serializable]
        public struct RendererTargetedReplacement<T> where T : UnityEngine.Object
        {
            //TODO: TransformPath can only infer the required component via a sibling property, not ideal, fix later in MSU.
            [SerializeField, HideInInspector]
            private Renderer _dummy;
            [TransformPath(nameof(vanillaTargetObject), allowSelectingRoot = false, rootComponentType = typeof(CharacterModel), siblingPropertyComponentTypeRequirement = nameof(_dummy))]
            public string? transformPath;
            public int rendererIndex;
            public bool useIndex;

            public T? replacement;
        }

        [Serializable]
        public struct LightReplacement
        {
            private Light? _dummy;
            [TransformPath(nameof(vanillaTargetObject), allowSelectingRoot = false, rootComponentType = typeof(CharacterModel), siblingPropertyComponentTypeRequirement = nameof(_dummy))]
            public string? transformPath;
            public int lightIndex;

            public bool useIndex;

            public Color? lightColor;
        }

        [Serializable]
        public struct PrefabInstantiationData
        {
            public GameObject? prefab;

            [TransformPath(nameof(vanillaTargetObject), allowSelectingRoot = true, rootComponentType = typeof(CharacterModel))]
            public string? transformPath;
            public string? childLocatorEntry;

            public bool useChildLocatorEntry;

            public Vector3 localPosition;
            public Vector3 localRotation;
            public Vector3 localScale;
        }

        private struct DisposableVariantVisualModifier : IDisposable
        {
            private CharacterBodyVariantController _variantController;
            private ModelSkinController _mdlSkinController;

            //Thank fuck model skin controller is basically a requirement now huh... :clueless:
            public DisposableVariantVisualModifier(CharacterBodyVariantController characterBodyVariantController, ModelSkinController skinController)
            {
                _mdlSkinController = skinController;
                _variantController = characterBodyVariantController;
            }

            public void Dispose()
            {
                if(_mdlSkinController && _variantController)
                {
                    _variantController.StartModelSkinControllerApplySkinCoroutine(_mdlSkinController.ApplySkinAsync(_mdlSkinController.currentSkinIndex, RoR2.ContentManagement.AsyncReferenceHandleUnloadType.OnSceneUnload));
                }
            }
        }
        #endregion

        /// <summary>
        /// The vanilla target object, this is utilized exclusively for editor time and it's not used in runtime. If you're creating variants at runtime do not fill this field.
        /// </summary>
        [SerializeField, AddressableComponentRequirement(typeof(ModelSkinController), searchInChildren = true)]
        private AssetReferenceGameObject vanillaTargetObject = new AssetReferenceGameObject("");

        public RendererTargetedReplacement<Material>[] materialReplacements = Array.Empty<RendererTargetedReplacement<Material>>();
        public RendererTargetedReplacement<Mesh>[] meshReplacements = Array.Empty<RendererTargetedReplacement<Mesh>>();
        public LightReplacement[] lightReplacements = Array.Empty<LightReplacement>();
        public PrefabInstantiationData[] prefabInstantiationDatas = Array.Empty<PrefabInstantiationData>();

        /*public IDisposable ApplyVisualModifiers(CharacterModel targetModel, ModelSkinController mdlSkinController)
        {

        }*/
    }
}