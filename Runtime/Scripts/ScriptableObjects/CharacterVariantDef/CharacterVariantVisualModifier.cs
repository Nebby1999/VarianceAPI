#nullable enable
using HG;
using MSU;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.AddressableAssets;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace VAPI
{/*
    /// <summary>
    /// Issues?:
    /// 1. Modded bodies cannot use the transform path... this seems fine however, as we expose the index. If the address is empty then we can just hide or disable the transform path and force useIndex to be on.
    /// 2. Special "Mesh Replacement" enums from VAPI 2.0 are not implemented
    ///     These enums suck, both in concept and in terms of performance, technically speaking we can check if theyre still needed.
    ///     If we do still need them, the code from VAPI 2.0 only modified the bones array from the skinned mesh renderer, so we can use our disposable system to dispose of those changes and revert it back to how it's meant to be,
    /// </summary>*/
    
    /// <summary>
    /// The <see cref="CharacterVariantVisualModifier"/> is a "Skin like" system for applying visuals to a variant.
    /// </summary>
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

        /// <summary>
        /// Represents a replacement of <typeparamref name="T"/> for a <see cref="Renderer"/>
        /// </summary>
        /// <typeparam name="T">The type of replacement</typeparam>
        [Serializable]
        public struct RendererTargetedReplacement<T> : ICloneable where T : UnityEngine.Object
        {
            [TransformPath(nameof(_vanillaTargetObject), allowSelectingRoot = false, rootComponentType = typeof(CharacterModel), componentTypeRequirement = typeof(Renderer))]
            [Tooltip("The tranform path to replace it's value, this must point to a GameObject that has a Renderer component.")]
            public string? transformPath;

            [Tooltip("The renderer info index to replace it's value")]
            public int rendererIndex;

            [Tooltip("Wether VAPI should prefer using the rendererIndex value rather than the transformPath value")]
            public bool useIndex;
            private bool _triedUpgrading;

            [Tooltip("The replacement itself")]
            public T? replacement;

            internal void UpgradeTransformPathToRendererIndex(CharacterModel characterModel)
            {
                if (_triedUpgrading == true)
                    return;

                _triedUpgrading = true;

                if (string.IsNullOrWhiteSpace(transformPath))
                {
                    return;
                }

                var charModelTransform = characterModel.transform;
                var child = charModelTransform.Find(transformPath);
                if (!child)
                    return;

                Renderer renderer = child.GetComponent<Renderer>();
                for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
                {
                    if (characterModel.baseRendererInfos[i].renderer == renderer)
                    {
                        useIndex = true;
                        rendererIndex = i;
                        return;
                    }
                }
            }

            internal bool TryGetRenderer(CharacterModel characterModel, out Renderer? renderer)
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

            /// <summary>
            /// Clones this renderer replacement
            /// </summary>
            /// <returns></returns>
            public object Clone()
            {
                return this;
            }
        }

        /// <summary>
        /// Represents a replacement of color for a <see cref="Light"/>
        /// </summary>
        [Serializable]
        public struct LightReplacement : ICloneable
        {
            [TransformPath(nameof(_vanillaTargetObject), allowSelectingRoot = false, rootComponentType = typeof(CharacterModel), componentTypeRequirement = typeof(Light))]
            [Tooltip("The tranform path to replace it's value, this must point to a GameObject that has a Light component.")]
            public string? transformPath;

            [Tooltip("The light info index to replace it's color value")]
            public int lightIndex;

            [Tooltip("Wether VAPI should prefer using the lightIndex value rather than the transformPath value")]
            public bool useIndex;
            private bool _triedUpgrading;

            public Color lightColor;

            internal bool TryGetLight(CharacterModel characterModel, out Light? light)
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

            internal void UpgradeTransformPathToLightIndex(CharacterModel characterModel)
            {
                if (_triedUpgrading == true)
                    return;

                _triedUpgrading = true;

                if (string.IsNullOrWhiteSpace(transformPath))
                {
                    return;
                }

                var charModelTransform = characterModel.transform;
                var child = charModelTransform.Find(transformPath);
                if (!child)
                    return;

                Light light = child.GetComponent<Light>();
                for (int i = 0; i < characterModel.baseLightInfos.Length; i++)
                {
                    if (characterModel.baseLightInfos[i].light == light)
                    {
                        useIndex = true;
                        lightIndex = i;
                        return;
                    }
                }
            }

            /// <summary>
            /// Clones this light replacement
            /// </summary>
            /// <returns></returns>
            public object Clone()
            {
                return this;
            }
        }

        /// <summary>
        /// Represents a prefab to instantiate on the variant.
        /// </summary>
        [Serializable]
        public sealed class PrefabInstantiationData : ICloneable
        {
            [Tooltip("A direct reference to the prefab to instantiate")]
            public GameObject? prefabDirectReference;
            [Tooltip("An address reference to the prefab to instantiate")]
            public AssetReferenceGameObject prefabAddress = new AssetReferenceGameObject("");

            [Tooltip("The tranform path where the prefab will be instantiated")]
            [TransformPath(nameof(_vanillaTargetObject), allowSelectingRoot = true, rootComponentType = typeof(CharacterModel))]
            public string transformPath = "";

            [Tooltip("A child locator where the prefab will be instantiated")]
            public string childLocatorEntry = "";

            [Tooltip("Wether VAPI should prefer using the childLocatorEntry value rather than the transformPath value")]
            public bool useChildLocatorEntry;

            [Tooltip("The local position for the instantiated prefab")]
            public Vector3 localPosition;
            [Tooltip("The local rotation for the instantiated prefab")]
            public Vector3 localRotation;
            [Tooltip("The local scale for the instantiated prefab")]
            public Vector3 localScale = Vector3.one;

            /// <summary>
            /// Clones this PrefabInstantiationData
            /// </summary>
            /// <returns></returns>
            public object Clone()
            {
                return new PrefabInstantiationData
                {
                    prefabDirectReference = prefabDirectReference,
                    prefabAddress = prefabAddress,
                    transformPath = transformPath,
                    childLocatorEntry = childLocatorEntry,
                    useChildLocatorEntry = useChildLocatorEntry,
                    localPosition = localPosition,
                    localRotation = localRotation,
                    localScale = localScale
                };
            }

            internal GameObject? GetPrefabToInstantiate(out AsyncOperationHandle<GameObject>? loadedPrefabHandle)
            {
                loadedPrefabHandle = null;

                if (prefabDirectReference != null)
                    return prefabDirectReference;
                else if(prefabAddress != null && prefabAddress.RuntimeKeyIsValid())
                {
                    loadedPrefabHandle = AssetAsyncReferenceManager<GameObject>.LoadAsset(prefabAddress, AsyncReferenceHandleUnloadType.OnSceneUnload);
                    return loadedPrefabHandle.Value.WaitForCompletion();
                }
                return null;
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

        [SerializeField, AddressableComponentRequirement(typeof(ModelSkinController), searchInChildren = true)]
        [Tooltip("the vanilla target object, this is utilized exclusively for editor time and it's not used in runtime. If you're creating variants at runtime do not fill this field.")]
        public AssetReferenceGameObject _vanillaTargetObject = new AssetReferenceGameObject("");

        [Tooltip("An array of material replacements")]
        public RendererTargetedReplacement<Material>[] materialReplacements = Array.Empty<RendererTargetedReplacement<Material>>();
        [Tooltip("An array of mesh replacements")]
        public RendererTargetedReplacement<Mesh>[] meshReplacements = Array.Empty<RendererTargetedReplacement<Mesh>>();
        [Tooltip("An array of light replacements")]
        public LightReplacement[] lightReplacements = Array.Empty<LightReplacement>();
        [Tooltip("An array of prefabs to instantiate")]
        public PrefabInstantiationData[] prefabInstantiationDatas = Array.Empty<PrefabInstantiationData>();

        /// <summary>
        /// Applies the visual modifiers to <paramref name="characterModel"/>. Returns an <see cref="IDisposable"/> used to revert any changes done to the model.
        /// </summary>
        /// <param name="appliedMeshReplacements">Becomes true if any mesh replacement is applied.</param>
        public IDisposable ApplyVisualModifiers(CharacterModel characterModel, CharacterBodyVariantController characterBodyVariantController, out bool appliedMeshReplacements)
        {
            if(!characterModel.TryGetComponent<ModelSkinController>(out var mdlSkinController))
            {
                VAPILog.Warning($"{characterModel} does not have a ModelSkinController! Characters require ModelSkinControllers and at the very least a BaseSkin since Memopt. Applying new variants to this character will not undo the skin changes.");
            }

            ApplyMaterialReplacements(characterModel);
            appliedMeshReplacements = ApplyMeshReplacements(characterModel);
            ApplyLightReplacements(characterModel);
            ApplyPrefabInstantiationDatas(characterModel);

            characterModel.materialsDirty = true;
            return new ReapplySkinOnDisposed(characterBodyVariantController, mdlSkinController);
        }

        private void ApplyMaterialReplacements(CharacterModel characterModel)
        {
            for(int i = 0; i < materialReplacements.Length; i++)
            {
                ref RendererTargetedReplacement<Material> rendererTargetedReplacement = ref materialReplacements[i];

                if(!rendererTargetedReplacement.replacement)
                {
                    VAPILog.Warning($"Material Replacement at index {i} of {this} lacks a Material. Not applying material replacement");
                    continue;
                }

                rendererTargetedReplacement.UpgradeTransformPathToRendererIndex(characterModel);
                if(rendererTargetedReplacement.useIndex && HG.ArrayUtils.IsInBounds(characterModel.baseRendererInfos, rendererTargetedReplacement.rendererIndex))
                {
                    ref CharacterModel.RendererInfo rendererInfo = ref characterModel.baseRendererInfos[rendererTargetedReplacement.rendererIndex];
                    rendererInfo.defaultMaterial = rendererTargetedReplacement.replacement;
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
        }

        private bool ApplyMeshReplacements(CharacterModel characterModel)
        {
            bool success = false;
            if(!VAPIConfig._activateMeshReplacementSystem)
            {
                return success;
            }

            void SetMesh(Renderer renderer, Mesh mesh)
            {
                if (renderer.TryGetComponent<MeshFilter>(out var meshFilter))
                {
                    meshFilter.sharedMesh = mesh;
                }
                else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    skinnedMeshRenderer.sharedMesh = mesh;
                }
                else
                {
                    VAPILog.Warning($"Cannot apply mesh {mesh} to renderer {renderer} as the renderer is not a SkinnedMeshRenderer or lacks a MeshFilter component.");
                }
            }

            for (int i = 0; i < meshReplacements.Length; i++)
            {
                ref RendererTargetedReplacement<Mesh> rendererTargetedReplacement = ref meshReplacements[i];
                if(!rendererTargetedReplacement.replacement)
                {
                    VAPILog.Warning($"Mesh Replacement at index {i} of {this} lacks a Mesh. Not applying mesh replacement.");
                    continue;
                }

                rendererTargetedReplacement.UpgradeTransformPathToRendererIndex(characterModel);
                if(rendererTargetedReplacement.useIndex && HG.ArrayUtils.IsInBounds(characterModel.baseRendererInfos, rendererTargetedReplacement.rendererIndex))
                {
                    ref CharacterModel.RendererInfo rendererInfo = ref characterModel.baseRendererInfos[rendererTargetedReplacement.rendererIndex];
                    SetMesh(rendererInfo.renderer, rendererTargetedReplacement.replacement!);
                    success = true;
                }
                else if(rendererTargetedReplacement.TryGetRenderer(characterModel, out var renderer))
                {
                    SetMesh(renderer!, rendererTargetedReplacement.replacement!);
                    success = true;
                }
                else
                {
                    VAPILog.Error($"Failed to apply Mesh Replacement at index {i} of {this}. The renderer could not be obtained via it's RendererInfo index or via the Transform Path.");
                }
            }
            return success;
        }

        private void ApplyLightReplacements(CharacterModel characterModel)
        {
            for(int i = 0; i < lightReplacements.Length; i++)
            {
                ref LightReplacement lightReplacement = ref lightReplacements[i];
                lightReplacement.UpgradeTransformPathToLightIndex(characterModel);

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
        }

        private void ApplyPrefabInstantiationDatas(CharacterModel characterModel)
        {
            GameObject InstantiatePrefabOnTransform(GameObject prefab, Transform parentTransform, Vector3 localPos, Vector3 localRot, Vector3 localScale)
            {
                var instance = Instantiate(prefab, parentTransform);
                var instanceTransform = instance.transform;
                instanceTransform.SetLocalPositionAndRotation(localPos, Quaternion.Euler(localRot));
                instanceTransform.localScale = localScale;
                return instance;
            }

            for (int i = 0; i < prefabInstantiationDatas.Length; i++)
            {
                ref PrefabInstantiationData prefabInstantiationData = ref prefabInstantiationDatas[i];

                GameObject? prefab = prefabInstantiationData.GetPrefabToInstantiate(out var handle);
                if(!prefab)
                {
                    VAPILog.Error($"Failed to Instantiate Prefab at index {i} of {this}. There's no GameObject to spawn.");
                    if (handle.HasValue)
                        AssetAsyncReferenceManager<GameObject>.UnloadAsset(prefabInstantiationData.prefabAddress);

                    continue;
                }

                Transform? parentTransform = null;
                GameObject? instantiatedPrefab = null;
                if (prefabInstantiationData.TryGetChildLocatorTransform(characterModel, out parentTransform))
                {
                    instantiatedPrefab = InstantiatePrefabOnTransform(prefab!, parentTransform!, prefabInstantiationData.localPosition, prefabInstantiationData.localRotation, prefabInstantiationData.localScale);
                    characterModel.customGameObjectActivationTransforms.Add(instantiatedPrefab.transform);
                }
                else if (prefabInstantiationData.TryGetTransformFromPath(characterModel, out parentTransform))
                {
                    instantiatedPrefab = InstantiatePrefabOnTransform(prefab!, parentTransform!, prefabInstantiationData.localPosition, prefabInstantiationData.localRotation, prefabInstantiationData.localScale);
                    characterModel.customGameObjectActivationTransforms.Add(instantiatedPrefab.transform);
                }
                else
                {
                    VAPILog.Error($"Failed to instantiate Prefab at index {i} of {this}. The Target Transform could not be obtained via it's ChildLocator entry or via the Transform Path.");
                    if (handle.HasValue)
                        AssetAsyncReferenceManager<GameObject>.UnloadAsset(prefabInstantiationData.prefabAddress);

                    return;
                }

                //If the handle is not null, add the addressable prefab instance component so it gets released when destroyed
                if(handle.HasValue)
                {
                    instantiatedPrefab!.AddComponent<AddressablePrefabInstance>()
                        .assetReference = prefabInstantiationData.prefabAddress;
                }

                if(instantiatedPrefab != null && instantiatedPrefab.TryGetComponent<CustomGameObjectActivationInfoContainer>(out var infoContainer))
                {
                    if(infoContainer.rendererInfos.Length > 0)
                    {
                        characterModel.baseRendererInfos = HG.ArrayUtils.Join(characterModel.baseRendererInfos, infoContainer.rendererInfos);
                    }
                    if(infoContainer.lightInfos.Length > 0)
                    {
                        characterModel.baseLightInfos = HG.ArrayUtils.Join(characterModel.baseLightInfos, infoContainer.lightInfos);
                    }
                }
            }
        }

        /// <summary>
        /// Struct for creating an instance of <see cref="CharacterVariantVisualModifier"/>
        /// </summary>
        public struct CreateInstanceArgs
        {
            /// <summary>
            /// The name for the scriptable object, cannot be null.
            /// </summary>
            public string name;

            /// <summary>
            /// A list of material replacements, can be null.
            /// </summary>
            public List<RendererTargetedReplacement<Material>>? materialReplacements;

            /// <summary>
            /// A list of mesh replacements, can be null
            /// </summary>
            public List<RendererTargetedReplacement<Mesh>>? meshReplacements;

            /// <summary>
            /// A list of light replacements, can be null
            /// </summary>
            public List<LightReplacement>? lightReplacements;

            /// <summary>
            /// A list of prefabs to instantiate, can be null.
            /// </summary>
            public List<PrefabInstantiationData>? prefabInstantiationDatas;

            /// <summary>Chainable method. Sets <see cref="name"/> to <paramref name="_name"/></summary>
            public CreateInstanceArgs SetName(string _name)
            {
                name = _name ?? throw new ArgumentNullException(nameof(_name));
                return this;
            }

            /// <summary>Chainable method. Adds <paramref name="replacement"/> to <see cref="materialReplacements"/></summary>
            public CreateInstanceArgs AddMaterialReplacement(RendererTargetedReplacement<Material> replacement)
            {
                materialReplacements ??= new List<RendererTargetedReplacement<Material>>();
                materialReplacements.Add(replacement);
                return this;
            }

            /// <summary>Chainable method. Adds <paramref name="replacements"/> to <see cref="materialReplacements"/></summary>
            public CreateInstanceArgs AddMaterialReplacement(IEnumerable<RendererTargetedReplacement<Material>> replacements)
            {
                materialReplacements ??= new List<RendererTargetedReplacement<Material>>();
                materialReplacements.AddRange(replacements);
                return this;
            }

            /// <summary>Chainable method. Adds <paramref name="replacement"/> to <see cref="meshReplacements"/></summary>
            public CreateInstanceArgs AddMeshReplacement(RendererTargetedReplacement<Mesh> replacement)
            {
                meshReplacements ??= new List<RendererTargetedReplacement<Mesh>>();
                meshReplacements.Add(replacement);
                return this;
            }

            /// <summary>Chainable method. Adds <paramref name="replacements"/> to <see cref="meshReplacements"/></summary>
            public CreateInstanceArgs AddMeshReplacement(IEnumerable<RendererTargetedReplacement<Mesh>> replacements)
            {
                meshReplacements ??= new List<RendererTargetedReplacement<Mesh>>();
                meshReplacements.AddRange(replacements);
                return this;
            }

            /// <summary>Chainable method. Adds <paramref name="replacement"/> to <see cref="lightReplacements"/></summary>
            public CreateInstanceArgs AddLightReplacement(LightReplacement replacement)
            {
                lightReplacements ??= new List<LightReplacement>();
                lightReplacements.Add(replacement);
                return this;
            }

            /// <summary>Chainable method. Adds <paramref name="replacements"/> to <see cref="lightReplacements"/></summary>
            public CreateInstanceArgs AddLightReplacement(IEnumerable<LightReplacement> replacements)
            {
                lightReplacements ??= new List<LightReplacement>();
                lightReplacements.AddRange(replacements);
                return this;
            }

            /// <summary>Chainable method. Adds <paramref name="data"/> to <see cref="prefabInstantiationDatas"/></summary>
            public CreateInstanceArgs AddPrefabInstantiationData(PrefabInstantiationData data)
            {
                prefabInstantiationDatas ??= new List<PrefabInstantiationData>();
                prefabInstantiationDatas.Add(data);
                return this;
            }

            /// <summary>Chainable method. Adds <paramref name="datas"/> to <see cref="prefabInstantiationDatas"/></summary>
            public CreateInstanceArgs AddPrefabInstantiationData(IEnumerable<PrefabInstantiationData> datas)
            {
                prefabInstantiationDatas ??= new List<PrefabInstantiationData>();
                prefabInstantiationDatas.AddRange(datas);
                return this;
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="CharacterVariantVisualModifier"/> based on <paramref name="args"/>'s values.
        /// <para></para>
        /// See <see cref="CreateInstanceArgs"/> for more info
        /// </summary>
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

        /// <summary>
        /// Create an instance of <see cref="CharacterVariantVisualModifier"/> by cloning <paramref name="other"/> and assigning its name to <paramref name="newName"/>
        /// </summary>
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