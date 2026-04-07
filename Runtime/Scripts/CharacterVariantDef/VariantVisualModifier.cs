#nullable enable
using HG;
using MSU;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.ContentManagement;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VAPI
{
    [CreateAssetMenu(fileName = "new VariantVisualModifier", menuName = "VarianceAPI/VariantVisualModifier")]
    public class VariantVisualModifier : ScriptableObject
    {
        #region Subclasses
        //Memopt made it mandatory for characters to have ModelSkinControllers and Skins. Since our visual modifiers utilize material, mesh, light and GameObjectActivations we can just dispose of the changes by reapplying the skin.
        //This is done via calling the "StartModelSkinControllerApplySkinCoroutine" method on the VariantController, which will save the first instance of the ModelSkinController's ApplySkinAsync method and execute it until completion.
        //As a result, once any visual modifier is disposed, the character should return back to normal.
        private struct ReapplySkinOnDisposed : IDisposable
        {
            private CharacterBodyVariantController _variantController;
            private ModelSkinController _skinController;

            public ReapplySkinOnDisposed(CharacterBodyVariantController variantController, ModelSkinController skinController)
            {
                _variantController = variantController;
                _skinController = skinController;
            }

            public void Dispose()
            {
                if(_skinController)
                {
                    bool isPlayerControlled = _skinController.characterModel && _skinController.characterModel.body && _skinController.characterModel.body.isPlayerControlled;

                    _variantController.StartModelSkinControllerApplySkinCoroutine(_skinController.ApplySkinAsync(_skinController.currentSkinIndex, isPlayerControlled && Run.instance ? AsyncReferenceHandleUnloadType.OnRunEnd : AsyncReferenceHandleUnloadType.AtWill));
                }
            }
        }
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

            public bool TryGetRenderer(CharacterModel characterModel, out Renderer? renderer)
            {
                renderer = null;
                if (string.IsNullOrWhiteSpace(transformPath))
                    return false;

                var characterModelTransform = characterModel.transform;
                var child = characterModelTransform.Find(transformPath);
                if (!child)
                    return false;

                return child.TryGetComponent<Renderer>(out renderer);
            }
        }

        [Serializable]
        public struct LightReplacement
        {
            private Light? _dummy;
            [TransformPath(nameof(vanillaTargetObject), allowSelectingRoot = false, rootComponentType = typeof(CharacterModel), siblingPropertyComponentTypeRequirement = nameof(_dummy))]
            public string? transformPath;
            public int lightIndex;

            public bool useIndex;

            public Color lightColor;

            public bool TryGetLight(CharacterModel characterModel, out Light? light)
            {
                light = null;
                if (string.IsNullOrWhiteSpace(transformPath))
                    return false;

                var characterModelTransform = characterModel.transform;
                var child = characterModelTransform.Find(transformPath);
                if (!child)
                    return false;

                return child.TryGetComponent<Light>(out light);
            }
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

            internal bool TryGetChildLocatorTransform(CharacterModel characterModel, out Transform? resultTransform)
            {
                resultTransform = null;
                if(!string.IsNullOrWhiteSpace(childLocatorEntry) && characterModel.childLocator && characterModel.childLocator.TryFindChild(childLocatorEntry, out resultTransform))
                {
                    return true;
                }

                return true;
            }

            internal bool TryGetTransformFromPath(CharacterModel characterModel, out Transform? resultTransform)
            {
                resultTransform = null;
                if (string.IsNullOrWhiteSpace(transformPath))
                    return false;

                var characterModelTransform = characterModel.transform;
                var child = characterModelTransform.Find(transformPath);
                if (!child)
                    return false;

                resultTransform = child;
                return true;
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

        public IDisposable ApplyVisualModifiers(CharacterModel characterModel, CharacterBodyVariantController characterBodyVariantController)
        {
            if(!characterModel.TryGetComponent<ModelSkinController>(out var mdlSkinController))
            {
                //Log warning abt the lack of mdlskincontroller
            }

            for(int i = 0; i < materialReplacements.Length; i++)
            {
                RendererTargetedReplacement<Material> rendererTargetedReplacement = materialReplacements[i];

                if(!rendererTargetedReplacement.replacement)
                {
                    //Log error, continue;
                    continue;
                }
                if(rendererTargetedReplacement.useIndex && HG.ArrayUtils.IsInBounds(characterModel.baseRendererInfos, rendererTargetedReplacement.rendererIndex))
                {
                    ref var rendererInfo = ref characterModel.baseRendererInfos[rendererTargetedReplacement.rendererIndex];
                    rendererInfo.defaultMaterial = rendererTargetedReplacement.replacement!;
                }
                else if(rendererTargetedReplacement.TryGetRenderer(characterModel, out var renderer))
                {
                    renderer!.sharedMaterial = rendererTargetedReplacement.replacement;
                }
                else
                {
                    //Log error here
                }
            }

            for(int i = 0; i < meshReplacements.Length; i++)
            {
                RendererTargetedReplacement<Mesh> rendererTargetedReplacement = meshReplacements[i];
                if(!rendererTargetedReplacement.replacement)
                {
                    //Log error, continue;
                    continue;
                }

                if(rendererTargetedReplacement.useIndex && HG.ArrayUtils.IsInBounds(characterModel.baseRendererInfos, rendererTargetedReplacement.rendererIndex))
                {
                    ref var rendererInfo = ref characterModel.baseRendererInfos[rendererTargetedReplacement.rendererIndex];
                    SetMesh(rendererInfo.renderer, rendererTargetedReplacement.replacement!);
                }
                else if(rendererTargetedReplacement.TryGetRenderer(characterModel, out var renderer))
                {
                    SetMesh(renderer!, rendererTargetedReplacement.replacement!);
                }
                else
                {
                    //Log error here
                }
            }

            for(int i = 0; i < lightReplacements.Length; i++)
            {
                LightReplacement lightReplacement = lightReplacements[i];

                if(lightReplacement.useIndex && HG.ArrayUtils.IsInBounds(characterModel.baseLightInfos, lightReplacement.lightIndex))
                {
                    ref var lightInfo = ref characterModel.baseLightInfos[lightReplacement.lightIndex];
                    lightInfo.defaultColor = lightReplacement.lightColor;
                }
                else if(lightReplacement.TryGetLight(characterModel, out var lightComponent))
                {
                    lightComponent!.color = lightReplacement.lightColor;
                }
                else
                {
                    //Log error here.
                }
            }

            for(int i = 0; i < prefabInstantiationDatas.Length; i++)
            {
                var prefabInstantiationData = prefabInstantiationDatas[i];

                if(!prefabInstantiationData.prefab)
                {
                    //Log error, continue.
                    continue;
                }

                Transform? parentTransform = null;
                if(prefabInstantiationData.TryGetChildLocatorTransform(characterModel, out parentTransform))
                {
                    characterModel.customGameObjectActivationTransforms.Add(InstantiatePrefabOnTransform(prefabInstantiationData.prefab!, parentTransform!, prefabInstantiationData.localPosition, prefabInstantiationData.localRotation, prefabInstantiationData.localScale).transform);
                }
                else if(prefabInstantiationData.TryGetTransformFromPath(characterModel, out parentTransform))
                {
                    characterModel.customGameObjectActivationTransforms.Add(InstantiatePrefabOnTransform(prefabInstantiationData.prefab!, parentTransform!, prefabInstantiationData.localPosition, prefabInstantiationData.localRotation, prefabInstantiationData.localScale).transform);
                }
                else
                {
                    //Log error
                }
            }

            return new ReapplySkinOnDisposed(characterBodyVariantController, mdlSkinController);
        }

        private void SetMesh(Renderer renderer, Mesh mesh)
        {
            if (renderer.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                meshFilter.sharedMesh = mesh;
            }
            else if(renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                skinnedMeshRenderer.sharedMesh = mesh;
            }
            else
            {
                //Log warning
            }
        }

        private GameObject InstantiatePrefabOnTransform(GameObject prefab, Transform parentTransform, Vector3 localPos, Vector3 localRot, Vector3 localScale)
        {
            var instance = Instantiate(prefab, parentTransform);
            var instanceTransform = instance.transform;
            instanceTransform.SetLocalPositionAndRotation(localPos, Quaternion.Euler(localRot));
            instanceTransform.localScale = localScale;
            return instance;
        }
    }
}