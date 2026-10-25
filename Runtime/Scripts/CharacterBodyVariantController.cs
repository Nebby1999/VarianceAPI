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

namespace VAPI
{
    /// <summary>
    /// A <see cref="NetworkBehaviour"/> used to apply variants at a Body level.
    /// <para></para>
    /// When the <see cref="CharacterBody"/> spawns, if it has a master it will try to sync itself with the master's <see cref="CharacterMasterVariantStorage"/>. If this succeeds it'll utilize the variants stored in it.
    /// <br></br>
    /// If this linking process fails, it'll roll it's own variants and store it in this component. This allows for the creation of masterless variants (For example, a goolake pot that leaves a tar pool on explosion)
    /// </summary>
    public sealed class CharacterBodyVariantController : NetworkBehaviour, IBodyStatArgModifier
    {
        public CharacterMasterVariantStorage characterMasterVariantStorage { get; private set; }
        /*public CharacterMasterVariantStorage? characterMasterVariantStorage
        {
            get
            {
                //Return it if we already have it
                if (_characterMasterVariantStorage)
                    return _characterMasterVariantStorage;

                //Search it up.

                //If the body somehow doesnt exist, or the body is flagged as masterless, return null.
                if(!characterBody || !characterBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
                {
                    return null;
                }

                var master = characterBody.master;

                //If there is no master, return null
                if (!master)
                    return null;

                var storage = master.EnsureComponent<CharacterMasterVariantStorage>();
                storage.AssignController(this);
                return storage;
            }
        }
        private CharacterMasterVariantStorage? _characterMasterVariantStorage;*/
        public ReadOnlyArray<CharacterVariantDef> characterVariantDefs
        {
            get
            {
                //If the master storage is not null, and it has stored variants, utilize it's stored array.
                if (characterMasterVariantStorage != null && characterMasterVariantStorage.hasStoredVariants)
                {
                    return characterMasterVariantStorage.storedVariants;
                }

                //Storage null, return our array
                return _fallbackCharacterVariantDefs;
            }
        }
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

        public event Action? onVariantsUnappliedBody;
        public event Action<CharacterVariantDef[]>? onVariantsAppliedBody;

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
        }

        private bool _skinHasBeenApplied = false;
        private void OnSkinApplied(int obj) => _skinHasBeenApplied = true;

        //At this point the link between master and body should be set
        private void Start()
        {
            if(characterMasterVariantStorage == null)
            {
                //Do not retrieve or try to create the storage if flagged as masterless
                if (!characterBody || characterBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless))
                    return;

                //Do not retrieve or try to create the storage if master doesnt exist
                var master = characterBody.master;
                if (!master)
                    return;

                //Ensure the component itself, and try to assign ourselves to the storage.
                characterMasterVariantStorage = master.EnsureComponent<CharacterMasterVariantStorage>();
                characterMasterVariantStorage.TryAssignController(this);
            }
        }

        [Server]
        public void SetCharacterVariantDefsServer(CharacterVariantDef[] variantDefs)
        {
            if (!characterBody)
                return;

            if(characterMasterVariantStorage != null && characterMasterVariantStorage.hasStoredVariants)
            {
                VAPILog.Warning($"Tried to set {this}'s variant defs, but it has a master storage and the master storage's stored variants are valid! this is not allowed!");
                return;
            }

            //Clear the synclist, then add the new variants
            _characterVariantIndices.Clear();
            for(int i = 0; i < variantDefs.Length; i++)
            {
                _characterVariantIndices.Add(variantDefs[i].characterVariantIndex);
            }

            //Call the OnSyncListDirty(), which will properly update our variants.
            OnSyncListDirty();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _characterVariantIndices.Callback = SyncListCallback; //Apply callback
        }

        //Callback is only called if the server is a client. So i _think_ a dedicated server will never get this callback, which isnt good.
        //Because of that, we will use this callback as a "Client only" callback.
        //From HLAPI source code:
        //
        //if (m_Behaviour.isServer && m_Behaviour.isClient && m_Callback != null)
        //{
        //  m_Callback.Invoke(op, itemIndex);
        //}
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
            UnapplyBodyModifications(characterVariantDefs);

            //Invoke the event, the master storage listens this to unapply it's master modifiers
            onVariantsUnappliedBody?.Invoke();

            //Second, create new array and populate.
            _fallbackCharacterVariantDefs = new CharacterVariantDef[_characterVariantIndices.Count];
            for(int i = 0; i < _characterVariantIndices.Count; i++)
            {
                _fallbackCharacterVariantDefs[i] = CharacterVariantCatalog.GetCharacterVariantDef(_characterVariantIndices[i])!;
            }

            //Thirdly, apply body modifications
            ApplyBodyModifications(_fallbackCharacterVariantDefs);

            //Invoke the event, the master storage listens this to apply it's master modifications and store the variants
            onVariantsAppliedBody?.Invoke(_fallbackCharacterVariantDefs);
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
            var scaleModifier = new VariantSizeModifierApplicator(characterBody, characterVariantDefs);
            scaleModifier.Apply();
            _disposableCollectionHelper.AddDisposable(scaleModifier);

            if (visualModifiers.Count > 0)
            {
                _applyVisualModifiersAfterSkinCoroutine = ApplyVisualModifiersAfterSkin(visualModifiers);
                StartCoroutine(_applyVisualModifiersAfterSkinCoroutine);
            }
        }

        private void UnapplyBodyModifications(ReadOnlyArray<CharacterVariantDef> characterVariantDefs)
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
            for (int i = 0; i < characterVariantDefs.Length; i++)
            {
                var characterVariantDef = characterVariantDefs[i];
                characterVariantDef.statModifier?.ApplyStatModifiers(args, characterBody);
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
            onVariantsAppliedBody = null;
            onVariantsUnappliedBody = null;
        }

        /*//So, VAPI 3.0 has the ability to store the variants on a master, however, we want to allow the ability for masterless variants to be a thing.
//As a result, the main variants for the body _are_ the ones found on the master, if said master storage is not found then it must utilize it's internal storage.
//In terms of networking, the "Source of Truth" is the Master, if no master, then its this component.




*/
    }
}