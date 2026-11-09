#nullable enable
using HG;
using MSU;
using R2API;
using RoR2;
using RoR2.ConVar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Networking;

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
        private static readonly ConVarFlags _flags =
#if DEBUG
            ConVarFlags.None
#else
            ConVarFlags.Cheat
#endif
            ;

        private static BoolConVar _logBodyController = new BoolConVar("vapi_cv_logbodycontroller", _flags, "0", "Enables logging for the CharacterBodyVariantController");

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
                    Log($"Setting doNotRollForVariants to {value}");
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
                if (_variantsApplied != value)
                {
                    _variantsApplied = value;
                }
            }
        }
        [SyncVar]
        private bool _variantsApplied;

        internal PreAssignedSpawnData ConsumePreAssignedSpawnData()
        {
            if (!HasPreAssignedSpawnData())
            {
                return default;
            }

            Log($"Consuming preassigned spawn data");
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
                Log("Setting Preassigned spawn data");
                _preAssignedSpawnDataServer = value;
            }
        }
        private PreAssignedSpawnData? _preAssignedSpawnDataServer;

        private CharacterVariantMask _appliedVariantMask;

        /// <summary>
        /// Event fired when variants are unapplied for this character.
        /// <para></para>
        /// Fired on both Server and Client
        /// <br></br>
        /// This gets called twice, once when the body first spawns, and another time when the <see cref="CharacterVariantDef"/> array has been set.
        /// </summary>
        public event Action? onVariantsUnapplied;

        /// <summary>
        /// Event fired when variants are applied for this character.
        /// <para></para>
        /// Fired on both Server and Client
        /// <br></br>
        /// This gets called twice, once when the body first spawns, and another time when the <see cref="CharacterVariantDef"/> array has been set.
        /// </summary>
        public event Action<CharacterVariantDef[]>? onVariantsApplied;

        private IVariantBodyApplicator _bodyApplicator;
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

            if(TryGetComponent<IVariantBodyApplicator>(out var bodyApplicator))
            {
                Log($"Utilizing custom body applicator");
                _bodyApplicator = bodyApplicator;
            }
            else
            {
                Log($"Utilizing default body applicator");
                _bodyApplicator = new DefaultVariantBodyApplicator();
            }

            Log("Awake");
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
        public bool IsVariantApplied(CharacterVariantIndex variantIndex)
        {
            bool result = _appliedVariantMask.Contains(variantIndex);
            if (!result)
            {
                Log($"{variantIndex} is not applied");
            }
            return result;
        }

        /// <summary>
        /// Do not call this method, left for networking purposes.
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();

            Log("On Start Client");
            //When the client starts, the indices should already be set on the server.
            //If the indices' count doesnt match our fallback array, we need to trigger a sync list dirty.
            //this can happen when going to a new stage while a variant's master is set to "DontDestroyOnLoad", without this call they'll be variants in the host but look normal on the clients.
            if (_characterVariantIndices.Count != _fallbackCharacterVariantDefs.Length)
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
                Log($"Retrieving CharacterVariantDefs from MasterStorage");
                return variantDefs!;
            }

            Log($"Retrieving CharacterVariantDefs from internal array");
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
            if (variantsApplied == true)
            {
                VAPILog.Warning($"Tried to set {this}'s variant defs, but there are already variants applied! Either set the array to an empty array first or do not try to override variants!.");
                return;
            }

            SetCharacterVariantDefsServerInternal(variantDefs);
        }

        [Server]
        private void SetCharacterVariantDefsServerInternal(CharacterVariantDef[] variantDefs)
        {
            if (variantDefs.Length > 0)
            {
                Log($"This is becoming a variant. with {variantDefs.Length} variants applied");
            }

            _characterVariantIndices.Clear();
            for (int i = 0; i < variantDefs.Length; i++)
            {
                _characterVariantIndices.Add(variantDefs[i].characterVariantIndex);
            }
            if (_characterVariantIndices.Count > 0)
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
            switch (op)
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
            if (storage != null)
            {
                Log("Unapplying variants on master");
                storage.UnapplyVariants(false);
            }

            onVariantsUnapplied?.Invoke();

            Log($"Clearing applied variant mask and retrieving new variants from synclist");
            //Clear the applied variant mask
            _appliedVariantMask.Clear();

            //Second, create new array and populate.
            _fallbackCharacterVariantDefs = new CharacterVariantDef[_characterVariantIndices.Count];
            for (int i = 0; i < _characterVariantIndices.Count; i++)
            {
                _fallbackCharacterVariantDefs[i] = CharacterVariantCatalog.GetCharacterVariantDef(_characterVariantIndices[i])!;
                _appliedVariantMask.Add(_characterVariantIndices[i]); //set the mask's value
            }

            //Thirdly, apply body modifications
            ApplyBodyModifications(_fallbackCharacterVariantDefs);

            if (storage != null)
            {
                Log($"Applying variants on master");
                storage.ApplyVariants(characterBody.bodyIndex, _fallbackCharacterVariantDefs);
            }

            onVariantsApplied?.Invoke(_fallbackCharacterVariantDefs);

            //If we're not supressing the arrival, and the config lets us send them, send it.
            if (NetworkServer.active && supressArrivalMessageServer == false && VAPIConfig._sendArrivalMessages)
            {
                AnnounceArrivalServer();
            }

            StartCoroutine(MarkStatsDirty());
        }

        private IEnumerator MarkStatsDirty()
        {
            //Call get stats interfaces from msu
            if (_msuContentBehaviour != null)
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
            if (!NetworkServer.active)
            {
                return;
            }

            Log($"Announcing Arrival");

            _possibleArrivalTokensShared.Clear();
            _possibleSpawnSoundsShared.Clear();

            bool shouldAnnounceArrival = false;
            bool shouldPlaySFX = false;

            for (int i = 0; i < _fallbackCharacterVariantDefs.Length; i++)
            {
                var def = _fallbackCharacterVariantDefs[i];
                if (!def || def.variantTier == null)
                    continue;

                if (def.variantTier.announceArrivalInChat)
                {
                    shouldAnnounceArrival = true;
                    if (!string.IsNullOrWhiteSpace(def.arrivalToken))
                    {
                        _possibleArrivalTokensShared.Add(def.arrivalToken);
                    }
                }

                if (def.variantTier.spawnedSoundEvent)
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

            if (shouldPlaySFX && _possibleSpawnSoundsShared.Count > 0)
            {
                var spawnSFX = HG.ArrayUtils.GetRandom(_possibleSpawnSoundsShared);
                EffectManager.SimpleSoundEffect(spawnSFX.index, characterBody.corePosition, true);
            }
        }

        private DisposableCollectionHelper _disposableCollectionHelper = new DisposableCollectionHelper(disposeInReverseOrder: true);
        private void ApplyBodyModifications(ReadOnlyArray<CharacterVariantDef> characterVariants)
        {
            _bodyApplicator.ApplyVariants(this, characterVariants);
        }

        [Obsolete("This property is no longer used")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string cachedVariantName { get; set; } = "";

        public bool nameModifiersActive { get; set; }

        private void UnapplyBodyModifications()
        {
            _bodyApplicator.UnapplyVariants(this);

            if (_applyVisualModifiersAfterSkinCoroutine != null)
            {
                StopCoroutine(_applyVisualModifiersAfterSkinCoroutine);
            }
            if (_mdlSkinControllerApplySkinCoroutine != null)
            {
                StopCoroutine(_mdlSkinControllerApplySkinCoroutine);
            }
            Log($"Body modifications disposed");
        }

        /// <summary>
        /// Method used to apply stat modifiers, you shouldn't call this directly as it's called automatically by MSU's systems.
        /// </summary>
        public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
        {
            var variants = GetCharacterVariantDefs();
            for (int i = 0; i < variants.Length; i++)
            {
                var variant = variants[i];
                variant.statModifier?.ApplyStatModifiers(args, characterBody);
            }
        }


        private IEnumerator? _applyVisualModifiersAfterSkinCoroutine;
        public void StartVisualModifierApplicationCoroutine(IEnumerator coroutine)
        {
            IEnumerator Wrapper(IEnumerator coroutine)
            {
                while(coroutine.MoveNext())
                {
                    if(coroutine.Current is bool value && value)
                    {
                        _applyVisualModifiersAfterSkinCoroutine = null;
                        yield break;
                    }
                    yield return coroutine.Current;
                }
            }

            _applyVisualModifiersAfterSkinCoroutine = Wrapper(coroutine);
            StartCoroutine(_applyVisualModifiersAfterSkinCoroutine);
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

        public void Log(object data)
        {
            if (_logBodyController.value)
            {
                VAPILog.Message($"[{gameObject.name}.CharacterBodyVariantController]: {data}");
            }
        }
    }
}