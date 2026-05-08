using System.Collections.Generic;
using RoR2.Editor;
using System.Collections;
using System.Linq;
using UnityEditor;
using VAPI.Legacy;
using System;
using System.Linq.Expressions;
using UnityEngine;
using HG;
using RoR2;
using UnityEngine.AddressableAssets;
using RoR2.Modding;

namespace VAPI.Editor.Windows
{
    public class VariantPackScriptableObjectMigrationWizard : EditorWizardWindow, IProgress<float>
    {
        [MenuItem("Tools/VAPI/VariantPack ScriptableObject Migration Wizard")]
        public static void Open() => GetWindow<VariantPackScriptableObjectMigrationWizard>();

        [MenuItem("test/test")]
        public static void Test()
        {
            new AddressablesPathDictionary.EntryLookup()
                .WithComponentRequirement(typeof(CharacterBody), false)
                .WithTypeRestriction(typeof(GameObject))
                .WithFilter("Body")
                .WithLookupType(AddressablesPathDictionary.EntryType.Path)
                .PerformLookup();
        }
        protected override string GetHelpTooltip()
        {
            return 
@"The VariantPack ScriptableObject Migration Wizard upgrades your VAPI.Legacy ScriptableObjects into their VAPI.Runtime counterparts.
Overall you should see a decrease in the total amount of ScriptableObjects due to VAPI.Runtime utilizing only 3 ScriptableObjects.";
        }

        public VariantTierDef[] variantTierDefs = Array.Empty<VariantTierDef>();
        public VariantDef[] variantDefs = Array.Empty<VariantDef>();

        private List<(VariantTierDef, CharacterVariantTierDef)> _createdRuntimeTiersWithLegacyCounterpartPairs = new List<(VariantTierDef, CharacterVariantTierDef)>();
        private List<(VariantDef, CharacterVariantDef)> _createdRuntimeVariantsWithLegacyCounterpartPairs = new List<(VariantDef, CharacterVariantDef)>();
        private List<(VariantVisuals, CharacterVariantVisualModifier)> _createdRuntimeVisualsWithLegacyCounterpartPairs = new List<(VariantVisuals, CharacterVariantVisualModifier)>();
        private Dictionary<VariantDef, CharacterBody> _legacyVariantToTargetBody = new Dictionary<VariantDef, CharacterBody>();

        protected override void OnIMGUIContainerAdded()
        {
            AddFooterButton("Find All Main Scriptable Objects", "Finds all VariantDefs and VariantTierDefs and adds it to the Upgrade list", () => BeginCoroutine(FindAllVariantDefs(), "Finding all VariantDefs and VariantTierDefs"));
        }
        protected override void OnIMGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(variantDefs)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(variantTierDefs)));
            serializedObject.ApplyModifiedProperties();
        }

        private IEnumerator FindAllVariantDefs()
        {
            using var _ = new AssetDatabaseUtil.AssetEditingScope();
            string[] guids = AssetDatabase.FindAssets("t:VariantDef");
            variantDefs = new VariantDef[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                yield return R2EKMath.Remap(i, 0, guids.Length, 0, 0.5f);
                variantDefs[i] = AssetDatabaseUtil.LoadAssetFromGUID<VariantDef>(guids[i]);
            }

            yield return 0.5f;

            guids = AssetDatabase.FindAssets("t:VariantTierDef");
            variantTierDefs = new VariantTierDef[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                yield return R2EKMath.Remap(i, 0, guids.Length, 0.5f, 1);
            }
        }

        protected override IEnumerator RunWizardCoroutine()
        {
            WizardCoroutineHelper helper = new WizardCoroutineHelper(this);

            if(variantTierDefs.Length > 0)
            {
                helper.AddStep(MigrateTierDefs(), "Migrating TierDefs");
                helper.AddStep(CreateCharacterTierDefAssets(), "Creating CharacterVariantTierDef Assets");
                helper.AddStep(SaveAssets(), "Saving Assets");
            }

            if(variantDefs.Length > 0)
            {
                bool anyVariantHasVisualModifiers = variantDefs.Any(vd => vd.visualModifier);
                if(anyVariantHasVisualModifiers)
                {
                    helper.AddStep(LoadAddressableBodies(helper), "Loading Addressable Bodies, this may take a bit.");
                    helper.AddStep(MigrateVisualModifiers(), "Migrating Visual Modifiers");
                }
                helper.AddStep(MigrateVariantDefs(), "Migrating VariantDefs");
                if(anyVariantHasVisualModifiers)
                {
                    helper.AddStep(CreateCharacterVariantVisualModifierAssets(), "Creating CharacterVariantVisualModifier Assets");
                    helper.AddStep(SaveAssets(), "Saving Assets");
                }
                helper.AddStep(CreateCharacterVairantDefAssets(), "Creating VariantCharacterDef Assets");
                helper.AddStep(SaveAssets(), "Saving Assets");
            }

            return helper;
        }

        protected override void Cleanup(string coroutineName)
        {
            base.Cleanup(coroutineName);
            if(coroutineName == "Run")
            {
                _createdRuntimeTiersWithLegacyCounterpartPairs.Clear();
                _createdRuntimeVariantsWithLegacyCounterpartPairs.Clear();
            }
        }

        public void Report(float progress)
        {
            UpdateProgress(progress);
        }

        private IEnumerator SaveAssets()
        {
            yield return 0;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            yield return 1;
        }

        #region TierMigration Coroutines
        private IEnumerator MigrateTierDefs()
        {
            yield return 0;

            for(int i = 0; i < variantTierDefs.Length; i++)
            {
                VariantTierDef legacyTierDef = variantTierDefs[i];
                yield return R2EKMath.Remap(i, 0, variantTierDefs.Length, 0, 1);

                //Create the instance args for this variant
                CharacterVariantTierDef.CreateInstanceArgs args = new CharacterVariantTierDef.CreateInstanceArgs
                {
                    name = legacyTierDef.name,
                    announceArrivalInChat = legacyTierDef.announcesArrival,
                    bonusArmor = legacyTierDef.armorBonus,
                    expRewardCoefficient = legacyTierDef.experienceMultiplier,
                    goldRewardCoefficient = legacyTierDef.goldMultiplier,
                    commonItemRewardChance = legacyTierDef.whiteItemDropChance,
                    uncommonItemRewardChance = legacyTierDef.greenItemDropChance,
                    legendaryItemRewardChance = legacyTierDef.redItemDropChance,
                    bossItemRewardChance = 0f,
                    tierBuffDef = legacyTierDef.tierBuffDef
                };

                //Add tiers with count of 1
                foreach(var thing in legacyTierDef.tierItemDefs)
                {
                    args.AddTierItem(new AddressableItemCountPair { itemDef = thing, count = 1 });
                }

                //Create instance, store result
                var result = CharacterVariantTierDef.CreateInstance(args);
                _createdRuntimeTiersWithLegacyCounterpartPairs.Add((legacyTierDef, result));
            }
            yield return 1;
        }

        private IEnumerator CreateCharacterTierDefAssets()
        {
            yield return 0;

            for(int i = 0; i < _createdRuntimeTiersWithLegacyCounterpartPairs.Count; i++)
            {
                CharacterVariantTierDef newTierDef = _createdRuntimeTiersWithLegacyCounterpartPairs[i].Item2;
                VariantTierDef oldTierDef = _createdRuntimeTiersWithLegacyCounterpartPairs[i].Item1;

                yield return R2EKMath.Remap(i, 0, _createdRuntimeTiersWithLegacyCounterpartPairs.Count, 0, 1);

                //Get old tier path
                string oldTierPath = AssetDatabase.GetAssetPath(oldTierDef);

                //Rename old tier, put "_Legacy" as the suffix.
                AssetDatabase.RenameAsset(oldTierPath, string.Format("{0}_Legacy", oldTierDef.name));

                //Create new tier asset
                AssetDatabase.CreateAsset(newTierDef, oldTierPath);

                //Import Asset
                AssetDatabase.ImportAsset(oldTierPath);
            }
            yield return 1;
        }

        #endregion

        private IEnumerator CreateTierDictionary()
        {
            yield return 0;

        }

        #region VariantMigration Coroutines
        private IEnumerator LoadAddressableBodies(IProgress<float> progress)
        {
            yield return 0;

            for (int i = 0; i < variantDefs.Length; i++)
            {
                VariantDef legacyVariant = variantDefs[i];
                yield return R2EKMath.Remap(i, 0, variantDefs.Length, 0, 1);

                VariantVisuals legacyVisuals = legacyVariant.visualModifier;
                if (!legacyVisuals)
                    continue;

                using (var entryLookup = new AddressablesPathDictionary.EntryLookup())
                {
                    entryLookup.WithComponentRequirement(typeof(CharacterBody), false)
                        .WithLookupType(AddressablesPathDictionary.EntryType.Path)
                        .WithFilter(legacyVariant.bodyName)
                        .WithTypeRestriction(typeof(GameObject))
                        .WithProgressReport(progress);

                    var subroutine = entryLookup.PerformLookupAsync();
                    while(subroutine.MoveNext())
                    {
                        yield return null;
                    }

                    string result = entryLookup.results.FirstOrDefault();
                    GameObject prefab = Addressables.LoadAssetAsync<GameObject>(result).WaitForCompletion();
                    if(prefab)
                    {
                        _legacyVariantToTargetBody.Add(legacyVariant, prefab.GetComponent<CharacterBody>());
                    }
                }
            }
        }
        private IEnumerator MigrateVisualModifiers()
        {
            yield return 0;
            for(int i = 0; i < variantDefs.Length; i++)
            {
                VariantDef legacyVariant = variantDefs[i];
                yield return R2EKMath.Remap(i, 0, variantDefs.Length, 0, 1);

                if (_legacyVariantToTargetBody.TryGetValue(legacyVariant, out CharacterBody body))
                    continue;

                VariantVisuals legacyVisuals = legacyVariant.visualModifier;
                if(!legacyVisuals)
                {
                    continue;
                }

                SkinDefParams skinDefParams = null;
                GameObject mdlObject = null;
                if(body.modelLocator && body.modelLocator.modelTransform)
                {
                    mdlObject = body.modelLocator.modelTransform.gameObject;
                    if(mdlObject.TryGetComponent<ModelSkinController>(out var mdlSkinController) && HG.ArrayUtils.IsInBounds(mdlSkinController.skins, 0))
                    {
                        var skinDef = mdlSkinController.skins[0];
                        if(skinDef.skinDefParams)
                        {
                            skinDefParams = skinDef.skinDefParams;
                        }
                        else if(skinDef.skinDefParamsAddress.RuntimeKeyIsValid())
                        {
                            skinDefParams = Addressables.LoadAssetAsync<SkinDefParams>(skinDef.skinDefParamsAddress.RuntimeKey).WaitForCompletion();
                        }
                    }
                }

                bool useTransformPath = skinDefParams && mdlObject;

                List<CharacterVariantVisualModifier.RendererTargetedReplacement<Material>> materialReplacements = new List<CharacterVariantVisualModifier.RendererTargetedReplacement<Material>>();

                foreach(var legacyMatReplacement in legacyVisuals.materialReplacements)
                {
                    int legacyRendererIndex = legacyMatReplacement.rendererIndex;
                    CharacterVariantVisualModifier.RendererTargetedReplacement<Material> runtimeMatReplacement = new CharacterVariantVisualModifier.RendererTargetedReplacement<Material>();
                    runtimeMatReplacement.replacement = legacyMatReplacement.material;

                    if(useTransformPath && HG.ArrayUtils.IsInBounds(skinDefParams.rendererInfos, legacyRendererIndex) && skinDefParams.rendererInfos[legacyRendererIndex].renderer)
                    {
                        runtimeMatReplacement.transformPath = Util.BuildPrefabTransformPath(mdlObject.transform, skinDefParams.rendererInfos[legacyRendererIndex].renderer.transform, false, false);
                    }
                    else
                    {
                        runtimeMatReplacement.rendererIndex = legacyRendererIndex;
                        runtimeMatReplacement.useIndex = true;
                    }

                    materialReplacements.Add(runtimeMatReplacement);
                }

                List<CharacterVariantVisualModifier.RendererTargetedReplacement<Mesh>> meshReplacements = new List<CharacterVariantVisualModifier.RendererTargetedReplacement<Mesh>>();
                foreach(var legacyMeshReplacement in legacyVisuals.meshReplacements)
                {
                    int legacyRendererIndex = legacyMeshReplacement.rendererIndex;
                    CharacterVariantVisualModifier.RendererTargetedReplacement<Mesh> runtimeMeshReplacement = new CharacterVariantVisualModifier.RendererTargetedReplacement<Mesh>();
                    runtimeMeshReplacement.replacement = legacyMeshReplacement.mesh;

                    if(useTransformPath && HG.ArrayUtils.IsInBounds(skinDefParams.rendererInfos, legacyRendererIndex) && skinDefParams.rendererInfos[legacyRendererIndex].renderer)
                    {
                        runtimeMeshReplacement.transformPath = Util.BuildPrefabTransformPath(mdlObject.transform, skinDefParams.rendererInfos[legacyRendererIndex].renderer.transform, false, false);
                    }
                    else
                    {
                        runtimeMeshReplacement.rendererIndex = legacyRendererIndex;
                        runtimeMeshReplacement.useIndex = true;
                    }

                    meshReplacements.Add(runtimeMeshReplacement);
                }

                List<CharacterVariantVisualModifier.LightReplacement> lightReplacements = new List<CharacterVariantVisualModifier.LightReplacement>();
                foreach(var legacyLightReplacement in legacyVisuals.lightReplacements)
                {
                    int legacyLightIndex = legacyLightReplacement.rendererIndex;
                    CharacterVariantVisualModifier.LightReplacement runtimeLightReplacement = new CharacterVariantVisualModifier.LightReplacement();
                    runtimeLightReplacement.lightColor = legacyLightReplacement.color;

                    runtimeLightReplacement.useIndex = useTransformPath == false;

                    if(useTransformPath && HG.ArrayUtils.IsInBounds(skinDefParams.lightReplacements, legacyLightIndex) && skinDefParams.lightReplacements[legacyLightIndex].light)
                    {
                        runtimeLightReplacement.transformPath = Util.BuildPrefabTransformPath(mdlObject.transform, skinDefParams.lightReplacements[legacyLightIndex].light.transform, false, false);
                    }
                    else
                    {
                        runtimeLightReplacement.lightIndex = legacyLightIndex;
                        runtimeLightReplacement.useIndex = true;
                    }

                    lightReplacements.Add(runtimeLightReplacement);
                }

                CharacterVariantVisualModifier.CreateInstanceArgs args = new CharacterVariantVisualModifier.CreateInstanceArgs()
                    .SetName(legacyVisuals.name)
                    .AddLightReplacement(lightReplacements)
                    .AddMeshReplacement(meshReplacements)
                    .AddMaterialReplacement(materialReplacements);

                var runtimeVisuals = CharacterVariantVisualModifier.CreateInstance(args);
                _createdRuntimeVisualsWithLegacyCounterpartPairs.Add((legacyVisuals, runtimeVisuals));
            }
        }

        private IEnumerator MigrateVariantDefs()
        {
            yield return 0;
            for(int i = 0; i < variantDefs.Length; i++)
            {
                VariantDef legacyVariant = variantDefs[i];

                yield return R2EKMath.Remap(i, 0, variantDefs.Length, 0, 1);

                CharacterVariantDef result = CachedNameScriptableObject.CreateInstance<CharacterVariantDef>(legacyVariant.name);

                result.targetCharacter = new CharacterVariantTarget(legacyVariant.bodyName, CharacterVariantTarget.TargetType.CharacterBody);
                result.variantTier = GetRuntimeTierFromLegacy(legacyVariant);
                result.isUnique = legacyVariant.isUnique;
                result.spawnRate = legacyVariant.spawnRate;
                result.arrivalToken = legacyVariant.arrivalToken;
                
                result.spawnCondition = GetBasicSpawnConditionFromLegacy(legacyVariant);
                
                result.masterModifiers = GetMasterModifiersFromLegacy(legacyVariant);
                result.inventoryDefinition = GetInventoryDefinitionFromLegacy(legacyVariant);

                result.variantNameProvider = GetVariantNameProviderFromLegacy(legacyVariant);
                result.deathStateOverride = new VariantDeathStateOverride(legacyVariant.deathStateOverride);
                result.skillReplacements = GetSkillReplacementsFromLegacy(legacyVariant);
                result.statModifier = GetBasicStatModifiersFromLegacy(legacyVariant);
                result.variantBuffs = GetVariantBuffsFromLegacy(legacyVariant);
                result.visualModifier = GetVisualModifierFromLegacyOrNull(legacyVariant);
                result.scaleMultiplier = legacyVariant.sizeModifier ? legacyVariant.sizeModifier.sizeCoefficient : 1f;
                result.additionalVariantComponents = GetVariantComponentsFromLegacy(legacyVariant);

                _createdRuntimeVariantsWithLegacyCounterpartPairs.Add((legacyVariant, result));
            }
            yield return 1;
        }

        private IEnumerator CreateCharacterVariantVisualModifierAssets()
        {
            yield return 0;
            for (int i = 0; i < _createdRuntimeVisualsWithLegacyCounterpartPairs.Count; i++)
            {
                CharacterVariantVisualModifier newVisuals = _createdRuntimeVisualsWithLegacyCounterpartPairs[i].Item2;
                VariantVisuals oldVisuals = _createdRuntimeVisualsWithLegacyCounterpartPairs[i].Item1;

                yield return R2EKMath.Remap(i, 0, _createdRuntimeVisualsWithLegacyCounterpartPairs.Count, 0, 1);

                //Get old visuals path
                string oldVisualsPath = AssetDatabase.GetAssetPath(oldVisuals);

                //Rename old visuals, put "_Legacy" as the suffix
                AssetDatabase.RenameAsset(oldVisualsPath, string.Format("{0}_Legacy", oldVisuals.name));

                //Create new visuals asset
                AssetDatabase.CreateAsset(newVisuals, oldVisualsPath);

                //Import asset
                AssetDatabase.ImportAsset(oldVisualsPath);
            }
            yield return 1;
        }

        private IEnumerator CreateCharacterVairantDefAssets()
        {
            yield return 0;

            for (int i = 0; i < _createdRuntimeVariantsWithLegacyCounterpartPairs.Count; i++)
            {
                CharacterVariantDef newVariant = _createdRuntimeVariantsWithLegacyCounterpartPairs[i].Item2;
                VariantDef oldVariant = _createdRuntimeVariantsWithLegacyCounterpartPairs[i].Item1;

                yield return R2EKMath.Remap(i, 0, _createdRuntimeTiersWithLegacyCounterpartPairs.Count, 0, 1);

                //Get old variant path
                string oldVariantPath = AssetDatabase.GetAssetPath(oldVariant);

                //Rename old variant, put "_Legacy" as the suffix.
                AssetDatabase.RenameAsset(oldVariantPath, string.Format("{0}_Legacy", oldVariant.name));

                //Create new variant asset
                AssetDatabase.CreateAsset(newVariant, oldVariantPath);

                //Import Asset
                AssetDatabase.ImportAsset(oldVariantPath);
            }
            yield return 1;
        }

        //TODO: Impl this
        private CharacterVariantTierDef GetRuntimeTierFromLegacy(VariantDef legacyVariant)
        {
            throw new NotImplementedException();
        }

        private IVariantSpawnCondition GetBasicSpawnConditionFromLegacy(VariantDef legacyVariant)
        {
            if(!legacyVariant.variantSpawnCondition)
            {
                return null;
            }

            var legacySpawnCondition = legacyVariant.variantSpawnCondition;
            BasicSpawnCondition basicSpawnCondition = new BasicSpawnCondition
            {
                customStages = legacySpawnCondition.customStages.ToArray(),
                stages = legacySpawnCondition.stages,
                minimumStageCompletions = legacySpawnCondition.minimumStageCompletions,
                requiredUnlock = legacySpawnCondition.requiredUnlock,
                forbiddenUnlock = legacySpawnCondition.forbiddenUnlock,
                requiredExpansionDefs = legacySpawnCondition.requiredExpansionDefs.ToArray()
            };
            return basicSpawnCondition;
        }

        private IVariantMasterModifier[] GetMasterModifiersFromLegacy(VariantDef legacyVariant)
        {
            List<IVariantMasterModifier> masterModifiers = new List<IVariantMasterModifier>();

            if(legacyVariant.aiModifier.HasFlag(BasicAIModifier.Unstable))
            {
                masterModifiers.Add(new UnstableAIModifier());
            }
            if(legacyVariant.aiModifier.HasFlag(BasicAIModifier.ForceSprint))
            {
                masterModifiers.Add(new AlwaysSprintAIModifier());
            }

            bool dampBonusNotAprox0 = !Mathf.Approximately(legacyVariant.baseAIDampBonus, 0);
            bool dampMultiplierNotAprox1 = !Mathf.Approximately(legacyVariant.baseAIDampMultiplier, 1);
            if(dampBonusNotAprox0 || dampMultiplierNotAprox1)
            {
                masterModifiers.Add(new BaseAIDampModifier(legacyVariant.baseAIDampBonus, legacyVariant.baseAIDampMultiplier));
            }

            return masterModifiers.ToArray();
        }

        private VariantInventoryDefinition GetInventoryDefinitionFromLegacy(VariantDef legacyVariant)
        {
            if(!legacyVariant.variantInventory)
            {
                return new VariantInventoryDefinition();
            }

            VariantInventory legacyInventory = legacyVariant.variantInventory;

            List<AddressableItemCountPair> addressableItemCountPairs = new List<AddressableItemCountPair>();
            VariantInventoryDefinition.AddressableEquipmentInfo addressableEquipmentInfo = default;

            for(int i = 0; i < legacyInventory.itemInventory.Length; i++)
            {
                var legacyItem = legacyInventory.itemInventory[i];
                addressableItemCountPairs.Add(new AddressableItemCountPair
                {
                    count = legacyItem.amount,
                    itemDef = legacyItem.item
                });
            }

            if(legacyInventory.equipmentInfo != null && 
                (legacyInventory.equipmentInfo.equipment.AssetExists || !string.IsNullOrWhiteSpace(legacyInventory.equipmentInfo.equipment.Address)))
            {
                addressableEquipmentInfo = new VariantInventoryDefinition.AddressableEquipmentInfo
                {
                    equipmentDef = legacyInventory.equipmentInfo.equipment,
                    aiMaxUseDistance = legacyInventory.equipmentInfo.aiMaxUseDistance,
                    aiMaxUseHealthFraction = legacyInventory.equipmentInfo.aiMaxUseHealthFraction,
                    canTriggerEquipment = legacyInventory.equipmentInfo.usable,
                    timeBetweenEquipmentSwitches = float.PositiveInfinity
                };
            }

            return new VariantInventoryDefinition(addressableItemCountPairs.ToArray(), addressableEquipmentInfo);
        }

        private IVariantNameProvider GetVariantNameProviderFromLegacy(VariantDef legacyVariant)
        {
            if (legacyVariant.nameOverrides.Length == 0)
                return null;

            //TODO: Log issue here, we've went from an array of overrides to an interface impl override.
            if(legacyVariant.nameOverrides.Length > 1)
            {

            }

            var firstOverride = legacyVariant.nameOverrides[0];
            switch(firstOverride.overrideType)
            {
                case OverrideNameType.Prefix:
                    return new VariantNamePrefix { prefixToken = firstOverride.token };
                case OverrideNameType.Suffix:
                    return new VariantNameSuffix { suffixToken = firstOverride.token };
                case OverrideNameType.Complete:
                    return new VariantNameOverride { overrideToken = firstOverride.token };
            }

            return null;
        }

        private VariantSkillReplacement[] GetSkillReplacementsFromLegacy(VariantDef legacyVariant)
        {
            List<VariantSkillReplacement> result = new List<VariantSkillReplacement>();

            for(int i = 0; i < legacyVariant.skillReplacements.Length; i++)
            {
                var legacySkillReplacement = legacyVariant.skillReplacements[i];
                result.Add(new VariantSkillReplacement(legacySkillReplacement.skillDef, legacySkillReplacement.skillSlot));
            }

            return result.ToArray();
        }

        private IVariantStatModifier GetBasicStatModifiersFromLegacy(VariantDef legacyVariant)
        {
            BasicStatModifier basicStatModifier = new BasicStatModifier
            {
                armorBonus = legacyVariant.armorBonus,
                armorMultiplier = legacyVariant.armorMultiplier,
                attackSpeedMultiplier = legacyVariant.attackSpeedMultiplier,
                damageMultiplier = legacyVariant.damageMultiplier,
                healthMultiplier = legacyVariant.healthMultiplier,
                moveSpeedMultiplier = legacyVariant.moveSpeedMultiplier,
                regenBonus = legacyVariant.regenBonus,
                regenMultiplier = legacyVariant.regenMultiplier,
                shieldBonus = legacyVariant.shieldBonus,
                shieldMultiplier = legacyVariant.shieldMultiplier,
            };

            return basicStatModifier;
        }

        private VariantBuffStorage GetVariantBuffsFromLegacy(VariantDef legacyVariant)
        {
            if(!legacyVariant.variantInventory || legacyVariant.variantInventory.buffInfos.Length == 0)
            {
                return new VariantBuffStorage();
            }

            var legacyBuffInfos = legacyVariant.variantInventory.buffInfos;
            List<VariantBuffInfo> runtimeBuffInfos = new List<VariantBuffInfo>();
            for(int i = 0; i < legacyBuffInfos.Length; i++)
            {
                var legacyBuffInfo = legacyBuffInfos[i];
                VariantBuffInfo runtimeBuffInfo = new VariantBuffInfo { buffDef = legacyBuffInfo.buff, count = legacyBuffInfo.amount };
                if(legacyBuffInfo.time > 0)
                {
                    runtimeBuffInfo.timedApplicationImpl = new LegacyTimedBuffApplication { totalTimeForBuffs = legacyBuffInfo.time };
                }
                runtimeBuffInfos.Add(runtimeBuffInfo);
            }

            return new VariantBuffStorage(runtimeBuffInfos.ToArray());
        }

        private CharacterVariantVisualModifier GetVisualModifierFromLegacyOrNull(VariantDef legacyVariant)
        {
            if(!legacyVariant.visualModifier)
            {
                return null;
            }

            for(int i = 0; i < _createdRuntimeVisualsWithLegacyCounterpartPairs.Count; i++)
            {
                if (_createdRuntimeVisualsWithLegacyCounterpartPairs[i].Item1 == legacyVariant.visualModifier)
                {
                    return _createdRuntimeVisualsWithLegacyCounterpartPairs[i].Item2;
                }
            }

            return null;
        }

        private VariantComponentCollection GetVariantComponentsFromLegacy(VariantDef legacyVariant)
        {
            if(legacyVariant.componentProviders.Length == 0)
            {
                return new VariantComponentCollection();
            }

            List<SerializableSystemType> variantComponents = new List<SerializableSystemType>();
            for(int i = 0; i < legacyVariant.componentProviders.Length;)
            {
                variantComponents.Add(legacyVariant.componentProviders[i].componentToAdd);
            }
            return new VariantComponentCollection(variantComponents.ToArray());
        }
        #endregion

    }
}