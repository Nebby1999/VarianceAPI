#nullable enable
using RoR2;
using RoR2.ConVar;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    //This class is added dynamically by the CharacterBodyVariantController if the body has a master. it's added to CharacterMasters.
    /// <summary>
    /// The <see cref="CharacterMasterVariantStorage"/> is a MonoBehaviour that manages the application and storage of <see cref="CharacterVariantDef"/> at the master level.
    /// <para></para>
    /// This component is added automatically to any CharacterMaster prefab whose body prefab has the <see cref="CharacterBodyVariantController"/> component, it is not necessary to add it manually.
    /// <br></br>
    /// As the name implies, it's main purpose is to store the <see cref="CharacterVariantDef"/>s that are applied to the body. This way the variant is preserved when the body respawns or when being taken to a new stage.    
    /// <para></para>
    /// The application of <see cref="CharacterVariantDef"/>s on the Master are handled via an implementation of <see cref="IVariantMasterApplicator"/>. The default implementation of this interface is via the <see cref="DefaultVariantMasterApplicator"/>. See <see cref="IVariantMasterApplicator"/> for more information.
    /// <para></para>
    /// Only one set of variant defs may be applied to the master at a time, if the Master's body index changes, the variants are unapplied.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class CharacterMasterVariantStorage : MonoBehaviour
    {
        private static readonly ConVarFlags _flags =
#if DEBUG
            ConVarFlags.None
#else
            ConVarFlags.Cheat
#endif
            ;

        private static BoolConVar _logMasterStorage = new BoolConVar("vapi_cv_logmasterstorage", _flags, "0", "Enables logging for the CharacterMasterVariantStorage");
        private struct StorageMetadataEntry
        {
            public BodyIndex relatedBodyIndex;
            public CharacterVariantDef[] storedVariants;
        }

        /// <summary>
        /// Access to the <see cref="CharacterMaster"/> sibling component
        /// </summary>
#pragma warning disable CS8618
        public CharacterMaster characterMaster { get; private set; }
#pragma warning restore CS8618

        /// <summary>
        /// Event fired when variants are unapplied for this master.
        /// <para></para>
        /// Fired on both Server and Client
        /// </summary>
        public event Action? onVariantsUnapplied;

        /// <summary>
        /// Event fired when variants are applied for this master.
        /// <para></para>
        /// Fired on both Server and Client
        /// </summary>
        public event Action<CharacterVariantDef[]>? onVariantsApplied;

        private IVariantMasterApplicator _masterApplicator;
        private List<StorageMetadataEntry> _storageMetadataList = new List<StorageMetadataEntry>();
        private BodyIndex _bodyIndexOfAppliedVariants = BodyIndex.None;

        private void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
            characterMaster.onBodyStart += OnBodyStart;

            if(TryGetComponent<IVariantMasterApplicator>(out var masterApplicator))
            {
                _masterApplicator = masterApplicator;
            }
            else
            {
                _masterApplicator = new DefaultVariantMasterApplicator();
            }
            Log("Awake");
        }

        //If the index were using is different from the index of applied variants, unapply them.
        //This ensures stuff like inventory is properly disposed of
        private void OnBodyStart(CharacterBody obj)
        {
            if (obj.bodyIndex != _bodyIndexOfAppliedVariants)
            {
                if (_bodyIndexOfAppliedVariants != BodyIndex.None)
                {
                    VAPILog.Info($"{this} switched from {BodyCatalog.GetBodyName(_bodyIndexOfAppliedVariants) ?? "None"} to {BodyCatalog.GetBodyName(obj.bodyIndex)}. Unapplying variants.");
                }
                Log($"Unapplying Variants as {obj}'s body index doesnt match {BodyCatalog.GetBodyName(_bodyIndexOfAppliedVariants)}");
                UnapplyVariants(true);
            }
        }

        /// <summary>
        /// Removes the Variant array storage for the character that <paramref name="bodyIndex"/> represents
        /// </summary>
        public void RemoveVariantStorageForCharacter(in BodyIndex bodyIndex)
        {
            if (bodyIndex == BodyIndex.None)
                return;

            var index = GetVariantStorageIndex(bodyIndex);
            if (index == -1)
                return;

            Log($"Removing the stored variants for body {BodyCatalog.GetBodyName(bodyIndex)}");
            _storageMetadataList.RemoveAt(index);
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="CharacterVariantDef"/> that are used for the body that <paramref name="bodyIndex"/> represents.
        /// <para></para>
        /// <paramref name="variantDefs"/> is null if this method returns false, if this method returns true then the array is not null, but may be empty.
        /// </summary>
        public bool TryGetVariantsStoredForCharacter(in BodyIndex bodyIndex, out CharacterVariantDef[]? variantDefs)
        {
            variantDefs = null;

            if (bodyIndex == BodyIndex.None)
                return false;

            int variantStorageListIndex = GetVariantStorageIndex(bodyIndex);
            if (variantStorageListIndex == -1)
            {
                Log($"There are no variants stored for body {BodyCatalog.GetBodyName(bodyIndex)}");
                return false;
            }

            variantDefs = _storageMetadataList[variantStorageListIndex].storedVariants;
            Log($"Retrieved a total of {variantDefs.Length} for body {BodyCatalog.GetBodyName(bodyIndex)}");
            return true;
        }

        /// <summary>
        /// Returns true if this Master Storage has variants for the body that <paramref name="bodyIndex"/> represents.
        /// </summary>
        public bool HasVariantsForBody(BodyIndex bodyIndex)
        {
            bool result = GetVariantStorageIndex(bodyIndex) != -1;
            if (!result)
            {
                Log($"There's no Variants for body {BodyCatalog.GetBodyName(bodyIndex)}");
            }
            return result;
        }

        private void PushVariantsForCharacter(in BodyIndex bodyIndex, in CharacterVariantDef[] variantDefs)
        {
            if (bodyIndex == BodyIndex.None || variantDefs == null)
            {
                return;
            }

            int variantStorageListIndex = GetVariantStorageIndex(bodyIndex);

            //Add to the storage as is
            if (variantStorageListIndex == -1)
            {
                Log($"Adding a new storage entry for {BodyCatalog.GetBodyName(bodyIndex)} with {variantDefs.Length} variants.");
                _storageMetadataList.Add(new StorageMetadataEntry { relatedBodyIndex = bodyIndex, storedVariants = variantDefs });
                return;
            }

            UnapplyVariants(true);
            //Override list
            Log($"Overriding existing storage for {BodyCatalog.GetBodyName(bodyIndex)} with {variantDefs.Length} variants.");
            _storageMetadataList[variantStorageListIndex] = new StorageMetadataEntry { relatedBodyIndex = bodyIndex, storedVariants = variantDefs };
        }

        private int GetVariantStorageIndex(BodyIndex bodyIndex)
        {
            for (int i = 0; i < _storageMetadataList.Count; i++)
            {
                if (_storageMetadataList[i].relatedBodyIndex == bodyIndex)
                    return i;
            }

            return -1;
        }

        internal void ApplyVariants(BodyIndex bodyIndex, CharacterVariantDef[] fallbackCharacterVariantDefs)
        {
            //If the body index of applied variants is the one that's incoming, return.
            //We definetly don't want to apply the master modifications twice.
            if (_bodyIndexOfAppliedVariants == bodyIndex)
            {
                return;
            }

            PushVariantsForCharacter(bodyIndex, fallbackCharacterVariantDefs);

            _masterApplicator.ApplyVariants(this, fallbackCharacterVariantDefs);

            _bodyIndexOfAppliedVariants = bodyIndex;
            Log($"Finished applying variants");
            onVariantsApplied?.Invoke(fallbackCharacterVariantDefs);
        }

        internal void UnapplyVariants(bool bodyIndexChanged = false)
        {
            _masterApplicator.UnapplyVariants(this, bodyIndexChanged);
            //Set the body index of applied variants to none
            _bodyIndexOfAppliedVariants = BodyIndex.None;

            Log($"All Variants have been unapplied");
            onVariantsUnapplied?.Invoke();
        }

        public void Log(object data)
        {
            if (_logMasterStorage.value)
            {
                VAPILog.Message($"[{gameObject.name}.CharacterMasterVariantStorage]: {data}");
            }
        }
    }
}