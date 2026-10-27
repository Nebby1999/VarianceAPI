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
    /// A <see cref="NetworkBehaviour"/> used to apply variants at a Body level.
    /// <para></para>
    /// When the <see cref="CharacterBody"/> spawns, if it has a master it will try to sync itself with the master's <see cref="CharacterMasterVariantStorage"/>. If this succeeds it'll utilize the variants stored in it.
    /// <br></br>
    /// If this linking process fails, it'll roll it's own variants and store it in this component. This allows for the creation of masterless variants (For example, a goolake pot that leaves a tar pool on explosion)
    /// </summary>
    [AddComponentMenu("")]
    public sealed class CharacterBodyVariantController : NetworkBehaviour, IBodyStatArgModifier
    {
        private CharacterVariantDef[] _fallbackCharacterVariantDefs = Array.Empty<CharacterVariantDef>();
        private SyncListCharacterVariantIndex _characterVariantIndices = new SyncListCharacterVariantIndex();

        public CharacterBody characterBody { get; private set; }
        public CharacterDeathBehavior? characterDeathBehavior { get; private set; }
        public CharacterModel? characterModel { get; private set; }

        /// <summary>
        /// The VariantSpawnManager will not roll for variants for this controller if this is true.
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
        public bool supressArrivalMessage
        {
            get => _supressArrivalMessage;
            [Server]
            set
            {
                if(_supressArrivalMessage != value)
                {
                    _supressArrivalMessage = value;
                }
            }
        }
        [SyncVar]
        private bool _supressArrivalMessage;

        [SyncVar]
        private bool _isBeingForcefullySet;

        public event Action? onVariantsUnapplied;
        public event Action<CharacterVariantDef[]>? onVariantsApplied;

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();

            if (characterBody.modelLocator && characterBody.modelLocator.modelTransform)
            {
                characterModel = characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            }

            if (characterBody.TryGetComponent<CharacterDeathBehavior>(out var deathBehavior))
            {
                characterDeathBehavior = deathBehavior;
            }

            if (characterModel && characterModel!.TryGetComponent<ModelSkinController>(out var mdlSkinController))
            {
                mdlSkinController.onSkinApplied += OnSkinApplied;
            }
            else
            {
                _skinHasBeenApplied = true;
            }

            _characterVariantIndices.Callback = SyncListCallback; //Apply callback
        }

        public ReadOnlyArray<CharacterVariantDef> GetCharacterVariantDefs()
        {
            CharacterMasterVariantStorage? storage = GetCharacterMasterVariantStorage();
            //If we have the storage, try getting the stored variants.
            if (storage != null && storage.TryGetVariantsStoredForCharacter(characterBody.bodyIndex, out var variantDefs))
            {
                return variantDefs!;
            }

            //Storage null, return our array
            return _fallbackCharacterVariantDefs;
        }

        public CharacterMasterVariantStorage? GetCharacterMasterVariantStorage()
        {
            //If the backing field is not null, and the storage's master's body points to our body, return the backing field.
            if (_characterMasterVariantStorage != null && _characterMasterVariantStorage.characterMaster && _characterMasterVariantStorage.characterMaster.GetBody() == characterBody)
            {
                return _characterMasterVariantStorage;
            }
            //Search our current storage and assign to the backing field

            //If there's no body, or the body is flagged as materless, return
            if (!characterBody || characterBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
                return null;

            //Retrieve the storage from the master, cache the result
            if (characterBody.master && characterBody.master.TryGetComponent<CharacterMasterVariantStorage>(out var storageInMaster))
            {
                _characterMasterVariantStorage = storageInMaster;
            }
            return _characterMasterVariantStorage;
        }
        private CharacterMasterVariantStorage? _characterMasterVariantStorage;

        private bool _skinHasBeenApplied = false;
        private void OnSkinApplied(int obj) => _skinHasBeenApplied = true;

        public override void OnStartServer()
        {
            base.OnStartServer();

            //When the controller starts on the server, check if we have a storage.
            //If the storage has a variants array, set them on the controller.

            //On the server when the body spawns it'll apply the ones on the storage.
            //If the body respawns (via an extra life or via polymorph), it'll set the variants

            //If a client disconnects, when reconnecting the server should have the proper array of variants to send to the client.

            CharacterMasterVariantStorage? storage = GetCharacterMasterVariantStorage();
            if(storage != null && storage.TryGetVariantsStoredForCharacter(characterBody.bodyIndex, out CharacterVariantDef[]? storedVariants))
            {
                //We want to call the internal method directly, as we're using the variants in the storage rather than a foreign array
                SetCharacterVariantDefsServerInternal(storedVariants!);
            }
        }

        //public facing method, in VAPI it's called by the spawn manager and the Util method to turn something into a variant.
        //Unless force set is set to true, it'll reject the operation if the storage has variants for the body
        [Server]
        public void SetCharacterVariantDefsServer(CharacterVariantDef[] variantDefs, bool forceSet = false)
        {
            if (!characterBody)
                return;

            if (variantDefs == null)
                return;

            //If we have a storage, and the storage already has variants set for our body, reject. unless we're forcing it
            CharacterMasterVariantStorage? storage = GetCharacterMasterVariantStorage();
            if(storage != null && storage.HasVariantsForBody(characterBody.bodyIndex) && forceSet == false)
            {
                VAPILog.Warning($"Tried to set {this}'s variant defs, but the master storage has variants for {BodyCatalog.GetBodyName(characterBody.bodyIndex)}! This is not allowed!");
                return;
            }

            if(_isBeingForcefullySet != forceSet)
            {
                _isBeingForcefullySet = forceSet;
            }
            SetCharacterVariantDefsServerInternal(variantDefs);
        }

        [Server]
        private void SetCharacterVariantDefsServerInternal(CharacterVariantDef[] variantDefs)
        {
            //Clear the synclist, then add the new variants
            _characterVariantIndices.Clear();
            for (int i = 0; i < variantDefs.Length; i++)
            {
                _characterVariantIndices.Add(variantDefs[i].characterVariantIndex);
            }

            //Call the OnSyncListDirty(), which will properly update our variants.
            OnSyncListDirty();
        }

        private void SyncListCallback(SyncList<NetworkCharacterVariantIndex>.Operation op, int itemIndex)
        {
            //GTFO if server.
            if (NetworkServer.active)
                return;

            OnSyncListDirty();
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

            //Second, create new array and populate.
            _fallbackCharacterVariantDefs = new CharacterVariantDef[_characterVariantIndices.Count];
            for(int i = 0; i < _characterVariantIndices.Count; i++)
            {
                _fallbackCharacterVariantDefs[i] = CharacterVariantCatalog.GetCharacterVariantDef(_characterVariantIndices[i])!;
            }

            //Thirdly, apply body modifications
            ApplyBodyModifications(_fallbackCharacterVariantDefs);

            if(storage != null)
            {
                storage.ApplyVariants(characterBody.bodyIndex, _fallbackCharacterVariantDefs, _isBeingForcefullySet);
            }

            onVariantsApplied?.Invoke(_fallbackCharacterVariantDefs);

            //If we're not supressing the arrival, and the config lets us send them, send it.
            if(!supressArrivalMessage && VAPIConfig._sendArrivalMessages)
            {
                AnnounceArrival();
            }
        }

        private static readonly List<string> _possibleArrivalTokensShared = new List<string>();
        private static readonly List<NetworkSoundEventDef> _possibleSpawnSoundsShared = new List<NetworkSoundEventDef>();
        private void AnnounceArrival()
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
                    _possibleSpawnSoundsShared.Add(def.variantTier.spawnedSoundEvent);
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

            //Set stats to dirty
            characterBody.MarkAllStatsDirty();
        }

        private void UnapplyBodyModifications()
        {
            _disposableCollectionHelper.Dispose();
            _skinHasBeenApplied = false;

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
            while (_skinHasBeenApplied == false)
            {
                yield return waitForEndOfFrame;
            }

            foreach (var visualModifier in visualModifiers)
            {
                _disposableCollectionHelper.AddDisposable(visualModifier.ApplyVisualModifiers(characterModel!, this));
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
        }
    }
}