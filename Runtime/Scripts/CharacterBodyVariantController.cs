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
    public class CharacterBodyVariantController : NetworkBehaviour, IBodyStatArgModifier
    {

        //So, VAPI 3.0 has the ability to store the variants on a master, however, we want to allow the ability for masterless variants to be a thing.
        //As a result, the main variants for the body _are_ the ones found on the master, if said master storage is not found then it must utilize it's internal storage.
        //In terms of networking, the "Source of Truth" is the Master, if no master, then its this component.
        public ReadOnlyArray<CharacterVariantDef> characterVariantDefs
        {
            get
            {
                if(characterMasterVariantStorage)
                {
                    return characterMasterVariantStorage!.characterVariantDefs;
                }

                return _fallbackCharacterVariantDefs;
            }
        }
        private CharacterVariantDef[] _fallbackCharacterVariantDefs = Array.Empty<CharacterVariantDef>();
        private SyncListCharacterVariantIndex _fallbackCharacterVariantIndices = new SyncListCharacterVariantIndex();
        public CharacterMasterVariantStorage? characterMasterVariantStorage { get; private set; }
        public CharacterBody characterBody { get; private set; }
        public CharacterDeathBehavior? characterDeathBehavior { get; private set; }
        public CharacterModel? characterModel { get; private set; }

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

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();

            if(characterBody.modelLocator && characterBody.modelLocator.modelTransform)
            {
                characterModel = characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            }

            if(characterBody.TryGetComponent<CharacterDeathBehavior>(out var deathBehavior))
            {
                characterDeathBehavior = deathBehavior;
            }

            if(characterModel && characterModel!.TryGetComponent<ModelSkinController>(out var mdlSkinController))
            {
                mdlSkinController.onSkinApplied += OnSkinApplied;
            }
            else
            {
                _skinHasBeenApplied = true;
            }
        }

        private bool _skinHasBeenApplied = false;
        private void OnSkinApplied(int obj)
        {
            _skinHasBeenApplied = true;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _fallbackCharacterVariantIndices.Callback = SyncListCallback;
        }

        private void Start()
        {
            TryLinkCharacterMasterVariantStorage();
        }

        public bool TryLinkCharacterMasterVariantStorage()
        {
            //Shortcircuit to true if we already have the master's variant storage.
            if(characterMasterVariantStorage)
            {
                return true;
            }

            if(!characterBody.master)
            {
                return false;
            }

            if(!characterBody.master.TryGetComponent<CharacterMasterVariantStorage>(out var masterVariantStorage))
            {
                return false;
            }

            if(masterVariantStorage.doNotRollForVariants)
            {
                //Should "cannotBeVariant" be inherited from the master's variant storage?...
                if(NetworkServer.active)
                {
                    doNotRollForVariants = masterVariantStorage.doNotRollForVariants;
                }
                return false;
            }

            //We've obtained our master's storage, so assign it here.
            characterMasterVariantStorage = masterVariantStorage;

            //Make it so we apply and unapply modifications whenever the master applies/unapplies modifications
            characterMasterVariantStorage.onCharacterMasterVariantStorageApply += ApplyBodyModifications;
            characterMasterVariantStorage.onCharacterMasterVariantStorageUnapply += UnapplyBodyModifications;
            return true;
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
            //First, unapply the body modifications.
            UnapplyBodyModifications(characterVariantDefs);

            //Second, create new array and populate
            _fallbackCharacterVariantDefs = new CharacterVariantDef[_fallbackCharacterVariantIndices.Count];
            for (int i = 0; i < _fallbackCharacterVariantIndices.Count; i++)
            {
                _fallbackCharacterVariantDefs[i] = CharacterVariantManager.GetCharacterVariantDef(_fallbackCharacterVariantIndices[i])!;
            }

            //Thirdy, apply body modifications
            ApplyBodyModifications(characterVariantDefs);
        }

        [Server]
        public void SetFallbackCharacterVariantDefs(CharacterVariantDef[] characterVariantDefs)
        {
            if(characterMasterVariantStorage)
            {
                //TODO: Log here, you shan't set fallback variant defs if the master storage exists.
                return;
            }
            //First, clear the syncList;
            _fallbackCharacterVariantIndices.Clear();
            //Then, add the indices to the sync list.
            for (int i = 0; i < characterVariantDefs.Length; i++)
            {
                _fallbackCharacterVariantIndices.Add(characterVariantDefs[i].characterVariantIndex);
            }

            //Call the OnSyncListDirty(), which will properly update our variants.
            OnSyncListDirty();
        }

        private DisposableCollectionHelper _disposableCollectionHelper = new DisposableCollectionHelper(disposeInReverseOrder: true);
        private void ApplyBodyModifications(ReadOnlyArray<CharacterVariantDef> characterVariants)
        {
            List<VariantVisualModifier> visualModifiers = new List<VariantVisualModifier>();
            /*
             * TODO:
             * 2. Apply the Scale Mutliplier
             */
            for(int i = 0; i < characterVariants.Length; i++)
            {
                CharacterVariantDef characterVariantDef = characterVariants[i];

                //Apply tier body modifiers
                if(characterVariantDef.variantTier)
                {
                    CharacterVariantTierDef variantTierDef = characterVariantDef.variantTier!;
                    _disposableCollectionHelper.AddDisposable(variantTierDef.ModifyBody(characterBody));
                }

                //Apply buffs
                _disposableCollectionHelper.AddDisposable(characterVariantDef.variantBuffs.ApplyBuffs(characterBody));

                //Apply skills
                if(characterBody.skillLocator)
                {
                    foreach(var skillReplacement in characterVariantDef.skillReplacements)
                    {
                        _disposableCollectionHelper.AddDisposable(skillReplacement.ApplySkillReplacement(characterBody.skillLocator));
                    }
                }

                //Apply death state override
                if(characterDeathBehavior)
                {
                    _disposableCollectionHelper.AddDisposable(characterVariantDef.deathStateOverride.ApplyDeathStateOverride(characterDeathBehavior));
                }

                //Add Body Components
                _disposableCollectionHelper.AddDisposable(characterVariantDef.additionalVariantComponents.ApplyComponents(characterBody));

                if(characterModel)
                {
                    //Add CharacterModel Components
                    _disposableCollectionHelper.AddDisposable(characterVariantDef.additionalVariantComponents.ApplyComponents(characterModel!));

                    //Store visual modifiers in the list, we will apply these in a coroutine since we need to await for the Skin to be applied.
                    if(characterVariantDef.visualModifier)
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

            if(_applyVisualModifiersAfterSkinCoroutine != null)
            {
                StopCoroutine(_applyVisualModifiersAfterSkinCoroutine);
            }
            if(_mdlSkinControllerApplySkinCoroutine != null)
            {
                StopCoroutine(_mdlSkinControllerApplySkinCoroutine);
            }
        }

        public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
        {
            for(int i = 0; i < characterVariantDefs.Length; i++)
            {
                var characterVariantDef = characterVariantDefs[i];
                characterVariantDef.statModifier?.ApplyStatModifiers(args, characterBody);
            }
        }

        private IEnumerator? _applyVisualModifiersAfterSkinCoroutine;
        private IEnumerator ApplyVisualModifiersAfterSkin(List<VariantVisualModifier> visualModifiers)
        {
            var waitForEndOfFrame = new WaitForEndOfFrame();
            while(_skinHasBeenApplied == false)
            {
                yield return waitForEndOfFrame;
            }

            foreach(var visualModifier in visualModifiers)
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
                while(applySkinCoroutine.MoveNext())
                {
                    yield return waitForEndOfFrame;
                }
                _mdlSkinControllerApplySkinCoroutine = null;
                yield break;
            }

            if(_mdlSkinControllerApplySkinCoroutine == null)
            {
                _mdlSkinControllerApplySkinCoroutine = InternalCoroutine(subroutine);
                StartCoroutine(_mdlSkinControllerApplySkinCoroutine);
            }
        }
    }
}