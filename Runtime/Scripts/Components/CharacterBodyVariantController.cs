#nullable enable
using HG;
using MSU;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VAPI
{
    /// <summary>
    /// A struct that represents metadata assigned before spawning.
    /// <br></br>
    /// This is mainly utilized when a variant is spawned via <see cref="VariantMasterSummon"/>, <see cref="VariantDirectorSpawnRequest"/> or by using <see cref="VAPIUtils.TransformSpawnedInstanceIntoVariant(VAPIUtils.TransformSpawnedInstanceIntoVariantArgs)"/>.
    /// </summary>
    public struct PreAssignedSpawnData
    {
        /// <summary>
        /// An array of CharacterVariantDefs to add to the final array of variants.
        /// <br></br>
        /// If not null and not empty, it is guaranteed that these variants will be applied to the body
        /// </summary>
        public CharacterVariantDef[]? additiveVariantDefs;

        /// <summary>
        /// A spawn rate multiplier utilized by the <see cref="VariantSpawnManager"/>'s systems for rolling variants.
        /// </summary>
        public float? spawnRateMultiplier;
    }

    /// <summary>
    /// The <see cref="CharacterBodyVariantController"/> is a NetworkBehaviour that manages the application and storage of <see cref="CharacterVariantDef"/> at the body level.
    /// <para></para>
    /// This component is added automatically to any CharacterBody prefab that has CharacterVariantDefs, it is not necessary to add it manually.
    /// <para></para>
    /// Variants are mainly spawned via the <see cref="VariantSpawnManager"/>, done when the Body calls "Start"
    /// <br></br>
    /// When an array of <see cref="CharacterVariantDef"/> is assigned via <see cref="SetCharacterVariantDefsServer(CharacterVariantDef[])"/>, the controller will try and store this array in the <see cref="CharacterMasterVariantStorage"/> on the master. This in turn will apply any master modifications, and store the variants in case the body respawns.
    /// <para></para>
    /// If you want to "Script" a variant spawn, it is recommended to utilize <see cref="VariantMasterSummon"/> and <see cref="VariantDirectorSpawnRequest"/>. Alternatively you can use <see cref="VAPIUtils.TransformSpawnedInstanceIntoVariant(VAPIUtils.TransformSpawnedInstanceIntoVariantArgs)"/>.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class CharacterBodyVariantController : NetworkBehaviour, IBodyStatArgModifier
    {
        private CharacterVariantDef[] _fallbackCharacterVariantDefs = Array.Empty<CharacterVariantDef>();
        private SyncListCharacterVariantIndex _characterVariantIndices = new SyncListCharacterVariantIndex();

        /// <summary>
        /// Access to the <see cref="CharacterBody"/> sibling component
        /// </summary>
        public CharacterBody characterBody { get; private set; }

        /// <summary>
        /// Access to the <see cref="CharacterDeathBehavior"/> sibling component, may be null
        /// </summary>
        public CharacterDeathBehavior? characterDeathBehavior { get; private set; }

        /// <summary>
        /// Access to the <see cref="CharacterModel"/> for the body, may be null
        /// </summary>
        public CharacterModel? characterModel { get; private set; }

        /// <summary>
        /// If true, the <see cref="VariantSpawnManager"/> will not roll for variants for this body.
        /// <para></para>
        /// <see cref="VariantMasterSummon"/>, <see cref="VariantDirectorSpawnRequest"/> and <see cref="VAPIUtils.TransformSpawnedInstanceIntoVariant(VAPIUtils.TransformSpawnedInstanceIntoVariantArgs)"/> have methods for setting this value.
        /// <br></br>
        /// The setter is server only.
        /// </summary>
        public bool doNotRollForVariants
        {
            get => _doNotRollForVariants;
            [Server]
            set
            {
                if (_doNotRollForVariants != value)
                {
                    _doNotRollForVariants = value;
                }
            }
        }
        [SyncVar]
        private bool _doNotRollForVariants;

        /// <summary>
        /// Server only boolean.
        /// <br></br>
        /// If true, when variants are applied to this body there will be no arrival message or sound effect transmitted.
        /// <para></para>
        /// <see cref="VariantMasterSummon"/>, <see cref="VariantDirectorSpawnRequest"/> and <see cref="VAPIUtils.TransformSpawnedInstanceIntoVariant(VAPIUtils.TransformSpawnedInstanceIntoVariantArgs)"/> have methods for setting this value.
        /// </summary>
        public bool supressArrivalMessageServer { get; set; }

        /// <summary>
        /// Returns true if _any_ variant is applied to this body.
        /// <br></br>
        /// This strictly means that the underlying <see cref="CharacterVariantDef"/> array is not null, and not empty. If the underlying array is not null, but empty, this will return false.
        /// </summary>
        public bool variantsApplied
        {
            get => _variantsApplied;
            [Server]
            set
            {
                if(_variantsApplied != value)
                {
                    _variantsApplied = value;
                }
            }
        }
        [SyncVar]
        private bool _variantsApplied;

        internal PreAssignedSpawnData ConsumePreAssignedSpawnData()
        {
            if(!HasPreAssignedSpawnData())
            {
                return default;
            }

            var value = _preAssignedSpawnDataServer!.Value;
            _preAssignedSpawnDataServer = null;
            return value;
        }
        internal bool HasPreAssignedSpawnData() => _preAssignedSpawnDataServer.HasValue;

        /// <summary>
        /// See <see cref="PreAssignedSpawnData"/> for more info.
        /// <br></br>
        /// The setter is server only
        /// </summary>
        public PreAssignedSpawnData? preAssignedSpawnDataServer
        {
            [Server]
            set
            {
                _preAssignedSpawnDataServer = value;
            }
        }
        private PreAssignedSpawnData? _preAssignedSpawnDataServer;

        private CharacterVariantMask _appliedVariantMask;

        /// <summary>
        /// Event fired when variants are unapplied for this character.
        /// <para</para>
        /// Fired on both Server and Client
        /// <br></br>
        /// This gets called twice, once when the body first spawns, and another time when the <see cref="CharacterVariantDef"/> array has been set.
        /// </summary>
        public event Action? onVariantsUnapplied;

        /// <summary>
        /// Event fired when variants are applied for this character.
        /// <para</para>
        /// Fired on both Server and Client
        /// <br></br>
        /// This gets called twice, once when the body first spawns, and another time when the <see cref="CharacterVariantDef"/> array has been set.
        /// </summary>
        public event Action<CharacterVariantDef[]>? onVariantsApplied;
        private MSUContentBehaviour? _msuContentBehaviour;
        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            _msuContentBehaviour = GetComponent<MSUContentBehaviour>();

            if (characterBody.modelLocator && characterBody.modelLocator.modelTransform)
            {
                characterModel = characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            }

            if (characterBody.TryGetComponent<CharacterDeathBehavior>(out var deathBehavior))
            {
                characterDeathBehavior = deathBehavior;
            }

            _characterVariantIndices.Callback = SyncListCallback;
            _appliedVariantMask = CharacterVariantMask.Rent();
        }

        /// <summary>
        /// Returns true if <paramref name="characterVariantDef"/> is inside the Controller's internal <see cref="CharacterVariantMask"/>.
        /// <br></br>
        /// If <paramref name="characterVariantDef"/> is null, it returns false.
        /// </summary>
        public bool IsVariantApplied(CharacterVariantDef? characterVariantDef) => IsVariantApplied(characterVariantDef ? characterVariantDef!.characterVariantIndex : CharacterVariantIndex.none);

        /// <summary>
        /// Returns true if <paramref name="variantIndex"/> is inside the Controller's internal <see cref="CharacterVariantMask"/>
        /// <br></br>
        /// If <paramref name="variantIndex"/> is out of bounds of the <see cref="CharacterVariantCatalog"/>, it returns false
        /// </summary>
        public bool IsVariantApplied(CharacterVariantIndex variantIndex) => _appliedVariantMask.Contains(variantIndex);

        /// <summary>
        /// Do not call this method, left for networking purposes.
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();

            //When the client starts, the indices should already be set on the server.
            //If the indices' count doesnt match our fallback array, we need to trigger a sync list dirty.
            //this can happen when going to a new stage while a variant's master is set to "DontDestroyOnLoad", without this call they'll be variants in the host but look normal on the clients.
            if(_characterVariantIndices.Count != _fallbackCharacterVariantDefs.Length)
            {
                OnSyncListDirty();
            }
        }

        /// <summary>
        /// Retrieves a <see cref="ReadOnlyArray{T}"/> of the <see cref="CharacterVariantDef"/> currently applied to this character.
        /// <br></br>
        /// If the character has a master, the retrieved value is from the <see cref="CharacterMasterVariantStorage"/>. Otherwise it's from the internal array stored in this controller.
        /// <para></para>
        /// This array is never null, keep in mind that it may be empty, an empty array is a valid state for the controller as it represents the abscense of variants.
        /// </summary>
        public ReadOnlyArray<CharacterVariantDef> GetCharacterVariantDefs()
        {
            CharacterMasterVariantStorage? storage = GetCharacterMasterVariantStorage();
            if (storage != null && storage.TryGetVariantsStoredForCharacter(characterBody.bodyIndex, out var variantDefs))
            {
                return variantDefs!;
            }

            //Storage null, return our array
            return _fallbackCharacterVariantDefs;
        }

        /// <summary>
        /// Retrieves the <see cref="CharacterMasterVariantStorage"/> for this body.
        /// <para></para>
        /// Returns null if the linking process fails, or if the body is flagged as <see cref="CharacterBody.BodyFlags.Masterless"/>
        /// </summary>
        public CharacterMasterVariantStorage? GetCharacterMasterVariantStorage()
        {
            if (_characterMasterVariantStorage != null && _characterMasterVariantStorage.characterMaster && _characterMasterVariantStorage.characterMaster.GetBody() == characterBody)
            {

                return _characterMasterVariantStorage;
            }

            //Search our current storage and assign to the backing field

            //If there's no body, or the body is flagged as materless, return
            if (!characterBody || characterBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
            {
                return null;
            }

            //Retrieve the storage from the master, cache the result
            if (characterBody.master && characterBody.master.TryGetComponent<CharacterMasterVariantStorage>(out var storageInMaster))
            {
                _characterMasterVariantStorage = storageInMaster;
            }

            return _characterMasterVariantStorage;
        }
        private CharacterMasterVariantStorage? _characterMasterVariantStorage;


        /// <summary>
        /// Method used to set the <see cref="CharacterVariantDef"/> for this character.
        /// <para></para>
        /// It is recommended to use <see cref="VariantMasterSummon"/>, <see cref="VariantDirectorSpawnRequest"/> or <see cref="VAPIUtils.TransformSpawnedInstanceIntoVariant(VAPIUtils.TransformSpawnedInstanceIntoVariantArgs)"/> instead of utilizing this method directly.
        /// <br></br>
        /// The array is rejected if <see cref="variantsApplied"/> is true.
        /// </summary>
        [Server]
        public void SetCharacterVariantDefsServer(CharacterVariantDef[] variantDefs)
        {
            if (!characterBody)
                return;

            if (variantDefs == null)
                return;

            //Only accept the incoming array if there are no variants applied.
            if(variantsApplied == true)
            {
                VAPILog.Warning($"Tried to set {this}'s variant defs, but there are already variants applied! Either set the array to an empty array first or do not try to override variants!.");
                return;
            }

            SetCharacterVariantDefsServerInternal(variantDefs);
        }

        [Server]
        private void SetCharacterVariantDefsServerInternal(CharacterVariantDef[] variantDefs)
        {
            if(variantDefs.Length > 0)
            {
                VAPILog.Message($"{this} is becoming a variant!");
            }

            _characterVariantIndices.Clear();
            for (int i = 0; i < variantDefs.Length; i++)
            {
                _characterVariantIndices.Add(variantDefs[i].characterVariantIndex);
            }
            if(_characterVariantIndices.Count > 0)
            {
                _characterVariantIndices.Dirty(0);
                variantsApplied = true;
            }
            else
            {
                variantsApplied = false;
            }
        }

        //Synclists kinda suck, if a character rolls multiple variants this method will be called n + 1 times. where n is the amount of variants being assigned.
        //The +1 is due to the clear operation.
        //So instead, we'll only call OnSyncListDirty if:
        //1. The operation is "CLEAR" (removing variants)
        //2. The operation is "DIRTY" (this will effectively be apply variants)
        private void SyncListCallback(SyncList<NetworkCharacterVariantIndex>.Operation op, int itemIndex)
        {
            switch(op)
            {
                case SyncList<NetworkCharacterVariantIndex>.Operation.OP_CLEAR:
                case SyncList<NetworkCharacterVariantIndex>.Operation.OP_DIRTY:
                    OnSyncListDirty();
                    break;
                default:
                    return;
            }
        }

        private void OnSyncListDirty()
        {
            //First, unapply the modifications, if any
            UnapplyBodyModifications();

            CharacterMasterVariantStorage? storage = GetCharacterMasterVariantStorage();
            if(storage != null) 
            {
                storage.UnapplyVariants();
            }

            onVariantsUnapplied?.Invoke();

            //Clear the applied variant mask
            _appliedVariantMask.Clear();
            
            //Second, create new array and populate.
            _fallbackCharacterVariantDefs = new CharacterVariantDef[_characterVariantIndices.Count];
            for(int i = 0; i < _characterVariantIndices.Count; i++)
            {
                _fallbackCharacterVariantDefs[i] = CharacterVariantCatalog.GetCharacterVariantDef(_characterVariantIndices[i])!;
                _appliedVariantMask.Add(_characterVariantIndices[i]); //set the mask's value
            }

            //Thirdly, apply body modifications
            ApplyBodyModifications(_fallbackCharacterVariantDefs);

            if (storage != null)
            {
                storage.ApplyVariants(characterBody.bodyIndex, _fallbackCharacterVariantDefs);
            }

            onVariantsApplied?.Invoke(_fallbackCharacterVariantDefs);

            //If we're not supressing the arrival, and the config lets us send them, send it.
            if(NetworkServer.active && supressArrivalMessageServer == false && VAPIConfig._sendArrivalMessages)
            {
                AnnounceArrivalServer();
            }

            StartCoroutine(MarkStatsDirty());
        }

        private IEnumerator MarkStatsDirty()
        {
            //Call get stats interfaces from msu
            if(_msuContentBehaviour != null)
            {
                _msuContentBehaviour.StartGetInterfaces();
            }
            //Await once
            yield return null;
            //by the time charbody's recalc stats run, we'll be on MSU's content behaviour system properly.
            characterBody.MarkAllStatsDirty();
        }
        private static readonly List<string> _possibleArrivalTokensShared = new List<string>();
        private static readonly List<NetworkSoundEventDef> _possibleSpawnSoundsShared = new List<NetworkSoundEventDef>();
        private void AnnounceArrivalServer()
        {
            //Get out if not the server
            if(!NetworkServer.active)
            {
                return;
            }

            _possibleArrivalTokensShared.Clear();
            _possibleSpawnSoundsShared.Clear();

            bool shouldAnnounceArrival = false;
            bool shouldPlaySFX = false;

            for(int i = 0; i < _fallbackCharacterVariantDefs.Length; i++)
            {
                var def = _fallbackCharacterVariantDefs[i];
                if (!def || def.variantTier == null)
                    continue;

                if(def.variantTier.announceArrivalInChat)
                {
                    shouldAnnounceArrival = true;
                    if(!string.IsNullOrWhiteSpace(def.arrivalToken))
                    {
                        _possibleArrivalTokensShared.Add(def.arrivalToken);
                    }
                }

                if(def.variantTier.spawnedSoundEvent)
                {
                    shouldPlaySFX = true;
                    _possibleSpawnSoundsShared.Add(def.variantTier.spawnedSoundEvent!);
                }
            }

            if (shouldAnnounceArrival)
            {
                //If somehow all the defs that had the correct tier setting don't have arrival tokens, use the generic arrival.
                bool hasAtLeastOneToken = _possibleArrivalTokensShared.Count > 0;
                string arrivalToken = hasAtLeastOneToken ? HG.ArrayUtils.GetRandom(_possibleArrivalTokensShared) : "VAPI_GENERIC_ARRIVAL";
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = arrivalToken,
                    paramTokens = hasAtLeastOneToken ? new string[] { Language.GetString(characterBody.baseNameToken) } : null,
                });
            }

            if(shouldPlaySFX && _possibleSpawnSoundsShared.Count > 0)
            {
                var spawnSFX = HG.ArrayUtils.GetRandom(_possibleSpawnSoundsShared);
                EffectManager.SimpleSoundEffect(spawnSFX.index, characterBody.corePosition, true);
            }
        }

        private DisposableCollectionHelper _disposableCollectionHelper = new DisposableCollectionHelper(disposeInReverseOrder: true);
        private void ApplyBodyModifications(ReadOnlyArray<CharacterVariantDef> characterVariants)
        {
            List<CharacterVariantVisualModifier> visualModifiers = new List<CharacterVariantVisualModifier>();

            for (int i = 0; i < characterVariants.Length; i++)
            {
                CharacterVariantDef characterVariantDef = characterVariants[i];

                //Apply tier body modifiers
                if (characterVariantDef.variantTier)
                {
                    CharacterVariantTierDef variantTierDef = characterVariantDef.variantTier!;
                    _disposableCollectionHelper.AddDisposable(variantTierDef.ModifyBody(characterBody));
                }

                //Apply buffs
                _disposableCollectionHelper.AddDisposable(characterVariantDef.variantBuffs.ApplyBuffs(characterBody));

                //Apply skills
                if (characterBody.skillLocator)
                {
                    foreach (var skillReplacement in characterVariantDef.skillReplacements)
                    {
                        _disposableCollectionHelper.AddDisposable(skillReplacement.ApplySkillReplacement(characterBody.skillLocator));
                    }
                }

                //Apply death state override
                if (characterDeathBehavior)
                {
                    _disposableCollectionHelper.AddDisposable(characterVariantDef.deathStateOverride.ApplyDeathStateOverride(characterDeathBehavior));
                }

                //Add Body Components
                _disposableCollectionHelper.AddDisposable(characterVariantDef.additionalVariantComponents.ApplyComponents(characterBody));

                if (characterModel)
                {
                    //Add CharacterModel Components
                    _disposableCollectionHelper.AddDisposable(characterVariantDef.additionalVariantComponents.ApplyComponents(characterModel!));

                    //Store visual modifiers in the list, we will apply these in a coroutine since we need to await for the Skin to be applied.
                    if (characterVariantDef.visualModifier)
                    {
                        visualModifiers.Add(characterVariantDef.visualModifier!);
                    }
                }
            }

            //Apply scale modifiers
            var scaleModifier = new VariantSizeModifierApplicator(characterBody, characterVariants);
            scaleModifier.Apply();
            _disposableCollectionHelper.AddDisposable(scaleModifier);

            if (visualModifiers.Count > 0)
            {
                _applyVisualModifiersAfterSkinCoroutine = ApplyVisualModifiersAfterSkin(visualModifiers);
                StartCoroutine(_applyVisualModifiersAfterSkinCoroutine);
            }

            RecalculateBodyName(characterVariants);
        }

        /// <summary>
        /// The current variant name for this character.
        /// <br></br>
        /// This is calculated by running the <see cref="IVariantNameProvider"/> of the applied variants against the <see cref="characterBody"/>'s name token
        /// </summary>
        public string cachedVariantName { get; private set; } = "";
        private void RecalculateBodyName(ReadOnlyArray<CharacterVariantDef> appliedVariants)
        {
            string baseName = Language.GetString(characterBody.baseNameToken);

            if(appliedVariants.Length == 0)
            {
                cachedVariantName = baseName;
                return;
            }

            using var _0 = ListPool<CharacterVariantDef>.RentCollection(out var sortedVariants);
            sortedVariants.AddRange(appliedVariants);
            sortedVariants.Sort((a, b) =>
            {
                bool aIsOverride = a.variantNameProvider is VariantNameOverride;
                bool bIsOverride = b.variantNameProvider is VariantNameOverride;

                return bIsOverride.CompareTo(aIsOverride);
            });

            foreach(var variant in sortedVariants)
            {
                if(variant.variantNameProvider != null)
                {
                    baseName = variant.variantNameProvider.GetVariantName(baseName);
                }
            }
            cachedVariantName = baseName;
        }

        private void UnapplyBodyModifications()
        {
            _disposableCollectionHelper.Dispose();

            if (_applyVisualModifiersAfterSkinCoroutine != null)
            {
                StopCoroutine(_applyVisualModifiersAfterSkinCoroutine);
            }
            if (_mdlSkinControllerApplySkinCoroutine != null)
            {
                StopCoroutine(_mdlSkinControllerApplySkinCoroutine);
            }
        }

        /// <summary>
        /// Method used to apply stat modifiers, you shouldn't call this directly as it's called automatically by MSU's systems.
        /// </summary>
        public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
        {
            var variants = GetCharacterVariantDefs();
            for(int i = 0; i < variants.Length; i++)
            {
                var variant = variants[i];
                variant.statModifier?.ApplyStatModifiers(args, characterBody);
            }
        }


        private IEnumerator? _applyVisualModifiersAfterSkinCoroutine;
        private IEnumerator ApplyVisualModifiersAfterSkin(List<CharacterVariantVisualModifier> visualModifiers)
        {
            var waitForEndOfFrame = new WaitForEndOfFrame();
            while ((characterModel ? characterModel!._hasSkinBeenApplied == false : false))
            {
                yield return waitForEndOfFrame;
            }

            bool shouldUpdateIDRS = false;
            foreach (var visualModifier in visualModifiers)
            {
                _disposableCollectionHelper.AddDisposable(visualModifier.ApplyVisualModifiers(characterModel!, this, out bool appliedMeshReplacements));
                shouldUpdateIDRS |= appliedMeshReplacements;
            }

            if(shouldUpdateIDRS && characterModel != null && characterModel.body != null && characterModel.body.inventory != null)
            {
                characterModel.DisableAllItemDisplays();
                characterModel.UpdateItemDisplay(characterModel.body.inventory);
            }

            _applyVisualModifiersAfterSkinCoroutine = null;
        }

        private IEnumerator? _mdlSkinControllerApplySkinCoroutine;
        internal void StartModelSkinControllerApplySkinCoroutine(IEnumerator subroutine)
        {
            IEnumerator InternalCoroutine(IEnumerator applySkinCoroutine)
            {
                var waitForEndOfFrame = new WaitForEndOfFrame();
                while (applySkinCoroutine.MoveNext())
                {
                    yield return waitForEndOfFrame;
                }
                _mdlSkinControllerApplySkinCoroutine = null;
                yield break;
            }

            if (_mdlSkinControllerApplySkinCoroutine == null)
            {
                _mdlSkinControllerApplySkinCoroutine = InternalCoroutine(subroutine);
                StartCoroutine(_mdlSkinControllerApplySkinCoroutine);
            }
        }

        private void OnDestroy()
        {
            onVariantsApplied = null;
            onVariantsUnapplied = null;
            CharacterVariantMask.Return(_appliedVariantMask);
        }
    }
}