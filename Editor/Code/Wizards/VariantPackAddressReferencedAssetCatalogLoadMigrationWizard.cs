using NUnit.Framework;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.Editor;
using RoR2.ExpansionManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VAPI.AddressReferencedAssets;

namespace VAPI.Editor.Windows
{
    public class VariantPackAddressReferencedAssetCatalogLoadMigrationWizard : EditorWizardWindow, IProgress<float>
    {
        [MenuItem("Tools/VAPI/Migrate AddressReferencedAsset catalog loads to GUID loads")]
        public static void Open() => GetWindow<VariantPackAddressReferencedAssetCatalogLoadMigrationWizard>();
        protected override string GetHelpTooltip()
        {
            return
@"The VariantPack AddressReferencedAsset Catalog Load Migration Wizard upgrades any AddressReferencedAsset fields that are loading a vanilla item thru the catalog to load them via the Addressables system.
This method of loading is more foolproof as item names may change but guids do not.";
        }

        public CharacterVariantDef[] variantDefs = Array.Empty<CharacterVariantDef>();
        public CharacterVariantTierDef[] variantTierDefs = Array.Empty<CharacterVariantTierDef>();

        private List<string> _addressableBodyPaths = new();
        private List<string> _addressableBuffPaths = new();
        private List<string> _addressableItemPaths = new();
        private List<string> _addressableEquipmentPaths = new();
        private List<string> _addressableUnlockablePaths = new();
        private List<string> _addressableExpansionPaths = new();
        private PropertyInfo _pInfo;

        protected override void OnIMGUIContainerAdded()
        {
            AddFooterButton("Find all Main Scriptable Objects", "Finds all CharacterVariantTierDefs and CharacterVariantDefs and adds them to the upgrade list.", () => BeginCoroutine(FindAllScriptableObjects(), "Finding all CharacterVariantDefs and CharacterVariantTierDefs"));
        }

        protected override void OnIMGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(variantDefs)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(variantTierDefs)));
            serializedObject.ApplyModifiedProperties();
        }

        private IEnumerator FindAllScriptableObjects()
        {
            yield return 0;
            using var _ = new AssetDatabaseUtil.AssetEditingScope();
            string[] guids = AssetDatabase.FindAssets("t:CharacterVariantDef");
            variantDefs = new CharacterVariantDef[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                yield return R2EKMath.Remap(i, 0, guids.Length, 0, 0.5f);
                variantDefs[i] = AssetDatabaseUtil.LoadAssetFromGUID<CharacterVariantDef>(guids[i]);
            }

            yield return 0.5f;

            guids = AssetDatabase.FindAssets("t:CharacterVariantTierDef");
            variantTierDefs = new CharacterVariantTierDef[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                yield return R2EKMath.Remap(i, 0, guids.Length, 0.5f, 1f);
                variantTierDefs[i] = AssetDatabaseUtil.LoadAssetFromGUID<CharacterVariantTierDef>(guids[i]);
            }

            yield return 1;
        }

        public void Report(float value)
        {
            UpdateProgress(value);
        }

        protected override IEnumerator RunWizardCoroutine()
        {
            WizardCoroutineHelper helper = new WizardCoroutineHelper(this);
            int stepCount = 0;

            Log("Began AddressReferencedAsset Catalog Load Migration Wizard.");
            Log("Adding 4 steps.", true);
            stepCount += 4;

            helper.AddStep(LoadAddressableEntries<CharacterBody>(helper, _addressableBodyPaths), "Loading Addressable Bodies, this may take a bit.");
            helper.AddStep(LoadAddressableEntries<ItemDef>(helper, _addressableItemPaths), "Loading Addressable Items, this may take a bit.");
            helper.AddStep(LoadAddressableEntries<EquipmentDef>(helper, _addressableEquipmentPaths), "Loading Addressable Equipments, this may take a bit.");
            helper.AddStep(LoadAddressableEntries<BuffDef>(helper, _addressableBuffPaths), "Loading Addressable Buffs, this may take a bit.");
            helper.AddStep(LoadAddressableEntries<UnlockableDef>(helper, _addressableUnlockablePaths), "Loading Addressable Unlockables, this may take a bit.");
            helper.AddStep(LoadAddressableEntries<ExpansionDef>(helper, _addressableExpansionPaths), "Loading Addressable Expansions, this may take a bit.");

            if (variantTierDefs.Length > 0)
            {
                Log("Tiers detected, adding 1 step.", true);
                stepCount++;
                helper.AddStep(MigrateAddressReferencedTiers(), "Migrating Catalog Loads from CharacterVariantTierDefs");
            }
            if (variantDefs.Length > 0)
            {
                Log("Variants detected, adding 1 step", true);
                stepCount++;
                helper.AddStep(MigrateAddressReferencedVariants(), "Migrating Catalog Loads from CharacterVariantDefs");
            }

            Log("Steps to Execute: " + stepCount, true);
            return helper;
        }

        protected override bool ValidateData(string coroutineName)
        {
            if (coroutineName == "Run")
            {
                return variantDefs.Length > 0 || variantTierDefs.Length > 0;
            }
            return true;
        }

        protected override void Cleanup(string coroutineName)
        {
            base.Cleanup(coroutineName);
            if(coroutineName == "Run")
            {
                _addressableBodyPaths.Clear();
                _addressableItemPaths.Clear();
                _addressableEquipmentPaths.Clear();
                _addressableBuffPaths.Clear();
                _addressableUnlockablePaths.Clear();
                _addressableExpansionPaths.Clear();
            }
        }

        #region AddressableLoad Coroutines
        private IEnumerator LoadAddressableEntries<T>(IProgress<float> progress, List<string> target)
        {
            yield return 0;

            Log($"Loading Addressable {typeof(T).Name}, this may take a while.");

            using (var entryLookup = new AddressablesPathDictionary.EntryLookup())
            {
                if(typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
                {
                    entryLookup.WithComponentRequirement(typeof(T), false)
                        .WithTypeRestriction(typeof(GameObject));
                }
                else
                {
                    entryLookup.WithTypeRestriction(typeof(T));
                }

                var subroutine = entryLookup.WithProgressReport(progress)
                    .WithLookupType(AddressablesPathDictionary.EntryType.Path)
                    .PerformLookupAsync();

                while(subroutine.MoveNext())
                {
                    yield return null;
                }

                target.AddRange(entryLookup.results);
            }

            yield return 1;
        }
        #endregion

        #region Migration Steps
        private IEnumerator MigrateAddressReferencedTiers()
        {
            using (new AssetDatabaseUtil.AssetEditingSaveScope())
            {
                yield return 0;
                for (int i = 0; i < variantTierDefs.Length; i++)
                {
                    bool modified = false;
                    CharacterVariantTierDef variantTierDef = variantTierDefs[i];
                    yield return R2EKMath.Remap(i, 0, variantTierDefs.Length, 0, 1);

                    if (variantTierDef.tierItems.Length > 0)
                    {
                        for (int tierItemIndex = 0; tierItemIndex < variantTierDef.tierItems.Length; tierItemIndex++)
                        {
                            AddressableItemCountPair itemCountPair = variantTierDef.tierItems[tierItemIndex];

                            if (!ShouldTryToUpgradeAddressReferencedAsset(itemCountPair.itemDef))
                            {
                                continue;
                            }

                            if (TryGetMatchingAssetFromAddressablePath(_addressableItemPaths, itemCountPair.itemDef.Address, out string itemGuid) == false)
                            {
                                continue;
                            }

                            var newReferencedItem = new AddressReferencedItemDef(itemGuid);
                            itemCountPair.itemDef = newReferencedItem;

                            variantTierDef.tierItems[tierItemIndex] = itemCountPair;
                            modified = true;
                        }
                    }

                    //If the buff def isnt a direct asset, the address is not a valid guid, try to get the matching guid for the asset.
                    if (ShouldTryToUpgradeAddressReferencedAsset(variantTierDef.tierBuffDef) && TryGetMatchingAssetFromAddressablePath(_addressableBuffPaths, variantTierDef.tierBuffDef.Address, out string buffGuid))
                    {
                        var newReferencedBuff = new AddressReferencedBuffDef(buffGuid);
                        variantTierDef.tierBuffDef = newReferencedBuff;
                        modified = true;
                    }

                    if (modified)
                    {
                        EditorUtility.SetDirty(variantTierDef);
                        AssetDatabase.SaveAssetIfDirty(variantTierDef);
                    }
                }
                yield return 1;
            }
        }

        private IEnumerator MigrateAddressReferencedVariants()
        {
            using(new AssetDatabaseUtil.AssetEditingSaveScope())
            {
                yield return 0;
                for (int i = 0; i < variantDefs.Length; i++)
                {
                    bool modified = false;
                    CharacterVariantDef variantDef = variantDefs[i];
                    yield return R2EKMath.Remap(i, 0, variantDefs.Length, 0, 1);

                    CharacterVariantTarget target = variantDef.targetBody;
                    if(ShouldTryToUpgradeAddressReferencedAsset(target.targetBody) && TryGetMatchingAssetFromAddressablePath(_addressableBodyPaths, target.targetBody.Address, out string bodyGUID))
                    {
                        target.targetBody = new AddressReferencedBody(bodyGUID);
                        modified = true;
                    }

                    if(variantDef.spawnCondition is BasicSpawnCondition basicSpawnCondition)
                    {
                        UpgradeBasicSpawnCondition(basicSpawnCondition);
                        modified = true;
                    }

                    modified |= UpgradeInventoryDefinition(variantDef.inventoryDefinition);
                    modified |= UpgradeVariantBuffStorage(variantDef.variantBuffs);

                    if (modified)
                    {
                        EditorUtility.SetDirty(variantDef);
                        AssetDatabase.SaveAssetIfDirty(variantDef);
                    }
                }
                yield return 1;
            }
        }

        private bool UpgradeBasicSpawnCondition(BasicSpawnCondition basicSpawnCondition)
        {
            bool result = false;

            if(ShouldTryToUpgradeAddressReferencedAsset(basicSpawnCondition.requiredUnlock) && TryGetMatchingAssetFromAddressablePath(_addressableUnlockablePaths, basicSpawnCondition.requiredUnlock.Address, out string guid))
            {
                basicSpawnCondition.requiredUnlock = new AddressReferencedUnlockableDef(guid);
                result = true;
            }

            if (ShouldTryToUpgradeAddressReferencedAsset(basicSpawnCondition.forbiddenUnlock) && TryGetMatchingAssetFromAddressablePath(_addressableUnlockablePaths, basicSpawnCondition.forbiddenUnlock.Address, out guid))
            {
                basicSpawnCondition.forbiddenUnlock = new AddressReferencedUnlockableDef(guid);
                result = true;
            }

            for(int i = 0; i < basicSpawnCondition.requiredExpansionDefs.Length; i++)
            {
                if (ShouldTryToUpgradeAddressReferencedAsset(basicSpawnCondition.requiredExpansionDefs[i]) && TryGetMatchingAssetFromAddressablePath(_addressableExpansionPaths, basicSpawnCondition.requiredExpansionDefs[i].Address, out guid))
                {
                    basicSpawnCondition.requiredExpansionDefs[i] = new AddressReferencedExpansionDef(guid);
                    result = true;
                }
            }

            return result;
        }

        private bool UpgradeInventoryDefinition(VariantInventoryDefinition inventoryDefinition)
        {
            bool result = false;
            string guid = "";
            for(int i = 0; i < inventoryDefinition.itemsToGrant.Length; i++)
            {
                var itemCountPair = inventoryDefinition.itemsToGrant[i];

                if(ShouldTryToUpgradeAddressReferencedAsset(itemCountPair.itemDef) && TryGetMatchingAssetFromAddressablePath(_addressableItemPaths, itemCountPair.itemDef.Address, out guid))
                {
                    itemCountPair.itemDef = new AddressReferencedItemDef(guid);
                    result = true;
                }

                inventoryDefinition.itemsToGrant[i] = itemCountPair;
            }

            if(ShouldTryToUpgradeAddressReferencedAsset(inventoryDefinition.equipmentInfo.equipmentDef) && TryGetMatchingAssetFromAddressablePath(_addressableEquipmentPaths, inventoryDefinition.equipmentInfo.equipmentDef.Address, out guid))
            {
                var eqpInfo = inventoryDefinition.equipmentInfo;

                eqpInfo.equipmentDef = new AddressReferencedEquipmentDef(guid);

                inventoryDefinition.equipmentInfo = eqpInfo;
                return true;
            }
            return result;
        }

        private bool UpgradeVariantBuffStorage(VariantBuffStorage buffStorage)
        {
            bool result = false;
            for(int i = 0; i < buffStorage.buffInfos.Length; i++)
            {
                var buffCountPair = buffStorage.buffInfos[i];

                if(ShouldTryToUpgradeAddressReferencedAsset(buffCountPair.buffDef) && TryGetMatchingAssetFromAddressablePath(_addressableBuffPaths, buffCountPair.buffDef.Address, out string guid))
                {
                    buffCountPair.buffDef = new AddressReferencedBuffDef(guid);
                    result = true;
                }

                buffStorage.buffInfos[i] = buffCountPair;
            }

            return result;
        }
        #endregion

        private bool ShouldTryToUpgradeAddressReferencedAsset<T>(AddressReferencedAsset<T> addressReferencedAsset) where T : UnityEngine.Object
        {
            if (addressReferencedAsset.AssetExists)
                return false;

            var address = addressReferencedAsset.Address;
            if (AddressablesPathDictionary.instance.IsGUIDValid(address))
                return false;

            return true;
        }
        private bool TryGetMatchingAssetFromAddressablePath(List<string> paths, string toMatch, out string guid)
        {
            guid = string.Empty;
            foreach(string path in paths)
            {
                var assetNameWithExtension = path.Substring(path.LastIndexOf('/') + 1);

                if(assetNameWithExtension.StartsWith(toMatch) && AddressablesPathDictionary.instance.TryGetGUIDFromPath(path, out guid))
                {
                    return true;
                }
            }
            return false;
        }
    }
}