#nullable enable
using HG;
using MSU;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VAPI
{
    [CreateAssetMenu(fileName = "new VariantVisualModifier", menuName = "VarianceAPI/VariantVisualModifier")]
    public sealed class CharacterVariantVisualModifier : CachedNameScriptableObject
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
        public struct RendererTargetedReplacement<T> : ICloneable where T : UnityEngine.Object
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

            public object Clone()
            {
                return this;
            }
        }

        [Serializable]
        public struct LightReplacement : ICloneable
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

            public object Clone()
            {
                return this;
            }
        }

        [Serializable]
        public struct PrefabInstantiationData : ICloneable
        {
            public GameObject? prefab;

            [TransformPath(nameof(vanillaTargetObject), allowSelectingRoot = true, rootComponentType = typeof(CharacterModel))]
            public string? transformPath;
            public string? childLocatorEntry;

            public bool useChildLocatorEntry;

            public Vector3 localPosition;
            public Vector3 localRotation;
            public Vector3 localScale;

            public object Clone()
            {
                return this;
            }

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
                VAPILog.Warning($"{characterModel} does not have a ModelSkinController! Characters require ModelSkinControllers and at the very least a BaseSkin since Memopt. Applying new variants to this character will not undo the skin changes.");
            }

            for(int i = 0; i < materialReplacements.Length; i++)
            {
                RendererTargetedReplacement<Material> rendererTargetedReplacement = materialReplacements[i];

                if(!rendererTargetedReplacement.replacement)
                {
                    VAPILog.Warning($"Material Replacement at index {i} of {this} lacks a Material. Not applying material replacement.");
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
                    VAPILog.Error($"Failed to apply Material Replacement at index {i} of {this}. The renderer could not be obtained via it's RendererInfo index or via the Transform Path.");
                }
            }

            for(int i = 0; i < meshReplacements.Length; i++)
            {
                RendererTargetedReplacement<Mesh> rendererTargetedReplacement = meshReplacements[i];
                if(!rendererTargetedReplacement.replacement)
                {
                    VAPILog.Warning($"Mesh Replacement at index {i} of {this} lacks a Mesh. Not applying mesh replacement.");
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
                    VAPILog.Error($"Failed to apply Mesh Replacement at index {i} of {this}. The renderer could not be obtained via it's RendererInfo index or via the Transform Path.");
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
                    VAPILog.Error($"Failed to apply Light Replacement at index {i} of {this}. The Light could not be obtained via it's LightInfo index or via the Transform Path.");
                }
            }

            for(int i = 0; i < prefabInstantiationDatas.Length; i++)
            {
                var prefabInstantiationData = prefabInstantiationDatas[i];

                if(!prefabInstantiationData.prefab)
                {
                    VAPILog.Error($"Failed to InstantiatePrefab at index {i} of {this}. There is no GameObject to spawn.");
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
                    VAPILog.Error($"Failed to instantiate Prefab at index {i} of {this}. The Target Transform could not be obtained via it's ChildLocator entry or via the Transform Path.");
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
                VAPILog.Warning($"Cannot apply mesh {mesh} to renderer {renderer} as the renderer is not a SkinnedMeshRenderer or lacks a MeshFilter component.");
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

        public struct CreateInstanceArgs
        {
            public string name;
            public List<RendererTargetedReplacement<Material>>? materialReplacements;
            public List<RendererTargetedReplacement<Mesh>>? meshReplacements;
            public List<LightReplacement>? lightReplacements;
            public List<PrefabInstantiationData>? prefabInstantiationDatas;

            public CreateInstanceArgs SetName(string _name)
            {
                name = _name ?? throw new ArgumentNullException(nameof(_name));
                return this;
            }

            public CreateInstanceArgs AddMaterialReplacement(RendererTargetedReplacement<Material> replacement)
            {
                materialReplacements ??= new List<RendererTargetedReplacement<Material>>();
                materialReplacements.Add(replacement);
                return this;
            }

            public CreateInstanceArgs AddMaterialReplacement(IEnumerable<RendererTargetedReplacement<Material>> replacements)
            {
                materialReplacements ??= new List<RendererTargetedReplacement<Material>>();
                materialReplacements.AddRange(replacements);
                return this;
            }

            public CreateInstanceArgs AddMeshReplacement(RendererTargetedReplacement<Mesh> replacement)
            {
                meshReplacements ??= new List<RendererTargetedReplacement<Mesh>>();
                meshReplacements.Add(replacement);
                return this;
            }

            public CreateInstanceArgs AddMeshReplacement(IEnumerable<RendererTargetedReplacement<Mesh>> replacements)
            {
                meshReplacements ??= new List<RendererTargetedReplacement<Mesh>>();
                meshReplacements.AddRange(replacements);
                return this;
            }

            public CreateInstanceArgs AddLightReplacement(LightReplacement replacement)
            {
                lightReplacements ??= new List<LightReplacement>();
                lightReplacements.Add(replacement);
                return this;
            }

            public CreateInstanceArgs AddLightReplacement(IEnumerable<LightReplacement> replacements)
            {
                lightReplacements ??= new List<LightReplacement>();
                lightReplacements.AddRange(replacements);
                return this;
            }

            public CreateInstanceArgs AddPrefabInstantiationData(PrefabInstantiationData data)
            {
                prefabInstantiationDatas ??= new List<PrefabInstantiationData>();
                prefabInstantiationDatas.Add(data);
                return this;
            }

            public CreateInstanceArgs AddPrefabInstantiationData(IEnumerable<PrefabInstantiationData> datas)
            {
                prefabInstantiationDatas ??= new List<PrefabInstantiationData>();
                prefabInstantiationDatas.AddRange(datas);
                return this;
            }
        }

        public static CharacterVariantVisualModifier CreateInstance(CreateInstanceArgs args)
        {
            CharacterVariantVisualModifier result = CreateInstance<CharacterVariantVisualModifier>();
            result.cachedName = args.name;
            result.materialReplacements = args.materialReplacements?.ToArray() ?? result.materialReplacements;
            result.meshReplacements = args.meshReplacements?.ToArray() ?? result.meshReplacements;
            result.lightReplacements = args.lightReplacements?.ToArray() ?? result.lightReplacements;
            result.prefabInstantiationDatas = args.prefabInstantiationDatas?.ToArray() ?? result.prefabInstantiationDatas;
            return result;
        }

        public static CharacterVariantVisualModifier CreateInstance(CharacterVariantVisualModifier other, string newName)
        {
            CreateInstanceArgs args = new CreateInstanceArgs();
            args.SetName(newName);
            for(int i = 0; i < other.materialReplacements.Length; i++)
            {
                args.AddMaterialReplacement((RendererTargetedReplacement<Material>)other.materialReplacements[i].Clone());
            }
            for(int i = 0; i < other.meshReplacements.Length; i++)
            {
                args.AddMeshReplacement((RendererTargetedReplacement<Mesh>)other.meshReplacements[i].Clone());
            }
            for(int i = 0; i < other.lightReplacements.Length; i++)
            {
                args.AddLightReplacement((LightReplacement)other.lightReplacements[i].Clone());
            }
            for(int i = 0; i < other.prefabInstantiationDatas.Length; i++)
            {
                args.AddPrefabInstantiationData((PrefabInstantiationData)other.prefabInstantiationDatas[i].Clone());
            }
            return CreateInstance(args);
        }
    }
}