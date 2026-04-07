using System.Collections.Generic;
using RoR2.Editor;
using System.Text;
using UnityEditor;
using VAPI.Legacy;
using System;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using UnityEngine;
using System.Linq;
using HG;

namespace VAPI.Editor
{ 
    /*
     * TODO:
     *  Rename legacy tier defs to "{0}_LEGACY"
     */
    public static class VariantUpgrader
    {
        private static bool _throwExceptions;
        private static StringBuilder _logBuilder = new StringBuilder();
        private static List<VariantDef> _variantDefsInProject = new List<VariantDef>();
        private static List<CharacterVariantDef> _createdCharacterVariantDefs = new List<CharacterVariantDef>();
        private static Dictionary<VariantTierDef, CharacterVariantTierDef> _legacyTierDefToRuntimeTierDef = new Dictionary<VariantTierDef, CharacterVariantTierDef>();

        [MenuItem("Tools/VAPI/Upgrade from 2.0 to 3.0")]
        private static void Upgrade2_0ScrobjsTo3_0Scrobjs()
        {
            var value = EditorUtility.DisplayDialogComplex("WARNING, READ THIS", "You're about to run the VariantUpgrader, which will attempt to upgrade any VAPI.Legacy ScriptableObject into their respective VAPI.Runtime counterparts. You'll obtain a log for the upgrade process with any errors found along the way.\n\rIt is EXTREMELY RECOMMENDED to back up your Assets folder OR utilize a repository to avoid any badly upgraded data!\n\rTo Continue with the upgrader, click either \"Execute Upgrade.\" Otherwise, click \"Cancel.\"", "Execute Upgrade (Throw Exceptions)", "Execute Upgrade (Treat Exceptions as Warnings)", "Cancel.");
            if (value == 1)
            {
                return;
            }

            _throwExceptions = value == 0;
            EditorCoroutineUtility.StartCoroutineOwnerless(RunUpgrader());
        }

        private static IEnumerator RunUpgrader()
        {
            var progress = new DisposableProgressBar("Preparing to Upgrade", "Info", 0);

            IEnumerator subroutine = ClearCollectionsAndLogBuilder();
            while(subroutine.MoveNext())
            {
                yield return null;
            }

            subroutine = FillCollections(progress);
            while(subroutine.MoveNext())
            {
                yield return null;
            }

            subroutine = UpgradeVariantDefs(progress);
            while(subroutine.MoveNext())
            {
                yield return null;
            }

            progress.Dispose();
        }


        private static IEnumerator ClearCollectionsAndLogBuilder()
        {
            _logBuilder.Clear();
            _variantDefsInProject.Clear();
            yield break;
        }

        private static IEnumerator FillCollections(DisposableProgressBar progress)
        {
            var variantDefGuids = AssetDatabase.FindAssets("t:VariantDef");
            for(int i = 0; i < variantDefGuids.Length; i++)
            {
                progress.Update(R2EKMath.Remap(i, 0, variantDefGuids.Length, 0, 0.5f), "Building Lookup Tables and Collections (Step 1 of ?)", $"Loaded VariantDef {i + 1} of {variantDefGuids.Length}");
                yield return null;

                var assetPath = AssetDatabase.GUIDToAssetPath(variantDefGuids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<VariantDef>(assetPath);
                if(asset)
                {
                    _variantDefsInProject.Add(asset);
                }
            }

            var variantTierDefGUIDS = AssetDatabase.FindAssets($"t:{nameof(VariantTierDef)}");
            var characterVariantTierDefGUIDS = AssetDatabase.FindAssets($"t:{nameof(CharacterVariantTierDef)}");

            for(int i = 0; i < variantTierDefGUIDS.Length; i++)
            {
                progress.Update(R2EKMath.Remap(i, 0, variantTierDefGUIDS.Length, 0.5f, 1f), null, "Building TierDef Lookup Table.");
                yield return null;

                VariantTierDef legacyTierDef = AssetDatabaseUtil.LoadAssetFromGUID<VariantTierDef>(variantTierDefGUIDS[i]);
                foreach(var characterVariantTierDefGUID in characterVariantTierDefGUIDS)
                {
                    CharacterVariantTierDef runtimeTierdef = AssetDatabaseUtil.LoadAssetFromGUID<CharacterVariantTierDef>(characterVariantTierDefGUID);

                    string matchString = runtimeTierdef.name + "_LEGACY";
                    if(matchString.Equals(legacyTierDef.name, StringComparison.OrdinalIgnoreCase))
                    {
                        if(!_legacyTierDefToRuntimeTierDef.TryAdd(legacyTierDef, runtimeTierdef))
                        {
                            LogWarning($"Runtime Tier {runtimeTierdef} matches with legacy tier {legacyTierDef}. But the Legacy Tier has already been added to the lookup table.");
                        }
                    }
                }
            }
            yield break;
        }

        private static IEnumerator UpgradeVariantDefs(DisposableProgressBar progress)
        {
            for(int i = 0; i < _variantDefsInProject.Count; i++)
            {
                VariantDef variantDef = _variantDefsInProject[i];
                progress.Update(R2EKMath.Remap(i, 0, _variantDefsInProject.Count, 0, 1), "Upgrading VariantDefs (Step 2 of ?)", $"Upgrading {variantDef}");
                yield return null;

                //Register undo.
                Undo.RegisterCompleteObjectUndo(variantDef, "Upgrade to CharacterVariantDef");

                var upgradeSingleSubroutine = UpgradeVariantDef(variantDef);
                //While loop set to true, which will progress the upgrade single subroutine. As long as it doesnt throw an exception or there's more work to do, it'll keep processing, otherwise it'll break off from this while loop. Lets us have TryCatch with yield return null.
                while(true)
                {
                    yield return null;
                    try
                    {
                        bool workLeft = upgradeSingleSubroutine.MoveNext();
                        if (!workLeft)
                        {
                            break;
                        }
                    }
                    catch(Exception ex)
                    {
                        LogError($"Failed to upgrade {variantDef} Undoing any modifications. {ex}");
                        Undo.PerformUndo();
                        break;
                    }
                }
            }
        }

        private static IEnumerator UpgradeVariantDef(VariantDef variantDef)
        {
            CharacterVariantDef result = ScriptableObject.CreateInstance<CharacterVariantDef>();

            //Upgrade from bodyName to target class.
            result.targetCharacter.targetType = VariantCharacterTarget.TargetType.CharacterBody;
            result.targetCharacter.keyIsFromCatalog = true;
            result.targetCharacter.key = variantDef.bodyName;

            //Upgrade from Legacy VariantTier to Runtime VariantTier
            if(variantDef._variantTierDef && _legacyTierDefToRuntimeTierDef.TryGetValue(variantDef._variantTierDef, out CharacterVariantTierDef runtimeTierDef))
            {
                result.variantTier = runtimeTierDef;
            }
            else if(variantDef._variantTier == VariantTierIndex.AssignedAtRuntime)
            {
                ThrowOrLogError(new NotSupportedException("Cannot deferr Legacy Tier to Runtime Tier when no VariantTierDef asset is present and the VariantTier enum is set to Assigned at Runtime."));
            }
            else if (variantDef._variantTier != VariantTierIndex.None)
            {
                foreach (var (legacy, runtime) in _legacyTierDefToRuntimeTierDef)
                {
                    if (legacy.tier == variantDef._variantTier)
                    {
                        result.variantTier = runtime;
                        break;
                    }
                }
            }

            //Set isUnique
            result.isUnique = variantDef.isUnique;

            //set spawn rate
            result.spawnRate = variantDef.spawnRate;

            //Set arrival token
            result.arrivalToken = variantDef.arrivalToken;

            //Create spawn condition
            result.spawnCondition = CreateSpawnCondition(variantDef);

            //Create master modifiers
            result.masterModifiers = CreateMasterModifiers(variantDef);

            //Create inventory definition
            result.inventoryDefinition = CreateInventoryDefinition(variantDef);

            //Create name provider
            result.variantNameProvider = CreateNameProvider(variantDef);

            //Create death state override
            result.deathStateOverride = CreateDeathStateOverride(variantDef);

            //Create skill replacements
            result.skillReplacements = CreateSkillReplacements(variantDef);

            //Create stat modifier
            result.statModifier = CreateStatModifier(variantDef);

            //Create buff storage
            result.variantBuffs = CreateBuffStorage(variantDef);

            //Create visual modifier
            result.visualModifier = CreateVisualModifier(variantDef);

            //Set scale multiplier
            result.scaleMultiplier = variantDef.sizeModifier ? variantDef.sizeModifier.sizeCoefficient : 1;

            //Create component collection
            result.additionalVariantComponents = CreateComponentCollection(variantDef);
            yield break;
        }

        private static IVariantSpawnCondition CreateSpawnCondition(VariantDef orig)
        {
            if (!orig.variantSpawnCondition)
            {
                return null;
            }

            VariantSpawnCondition legacySpawnCondition = orig.variantSpawnCondition;
            Type legacyType = legacySpawnCondition.GetType();
            if (legacySpawnCondition.GetType().IsSubclassOf(typeof(VariantSpawnCondition)))
            {
                //Try find static method to upgrade
                var upgradeMethod = legacyType.GetMethod("UpgradeToIVariantSpawnCondition", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if(upgradeMethod == null)
                {
                    ThrowOrLogError(new MissingMemberException($"{legacySpawnCondition} is a Subclass of VariantSpawnCondition, And a \"static IVariantSpawnCondition UpgradeToIVariantSpawnCondition({legacyType.Name})\" method was not found. Cannot upgrade SpawnCondition."));
                    return null;
                }

                return (IVariantSpawnCondition)upgradeMethod.Invoke(null, new object[] { legacySpawnCondition });
            }

            var result = new BasicSpawnCondition()
            {
                minimumStageCompletions = legacySpawnCondition.minimumStageCompletions,
                customStages = legacySpawnCondition.customStages.ToArray(),
                forbiddenUnlock = legacySpawnCondition.forbiddenUnlock,
                requiredUnlock = legacySpawnCondition.requiredUnlock,
                requiredExpansionDefs = legacySpawnCondition.requiredExpansionDefs.ToArray(),
                stages = legacySpawnCondition.stages
            };
            return result;
        }

        private static IVariantMasterModifier[] CreateMasterModifiers(VariantDef variantDef)
        {
            List<IVariantMasterModifier> createdModifiers = new List<IVariantMasterModifier>();

            if (variantDef.aiModifier.HasFlag(BasicAIModifier.Unstable))
            {
                createdModifiers.Add(new UnstableAIModifier());
            }
            if(variantDef.aiModifier.HasFlag(BasicAIModifier.ForceSprint))
            {
                createdModifiers.Add(new AlwaysSprintAIModifier());
            }

            //If the BaseAIDamp Bonus or Multiplier are not approximately the default values, create the damp modifier.
            if((!Mathf.Approximately(variantDef.baseAIDampBonus, 0)) || (!Mathf.Approximately(variantDef.baseAIDampMultiplier, 1)))
            {
                createdModifiers.Add(new BaseAIDampModifier
                {
                    baseAIDampBonus = variantDef.baseAIDampBonus,
                    baseAIDampMultiplier = variantDef.baseAIDampMultiplier,
                });
            }
            return createdModifiers.ToArray();
        }

        private static VariantInventoryDefinition CreateInventoryDefinition(VariantDef variantDef)
        {
            if(!variantDef.variantInventory)
            {
                return new VariantInventoryDefinition();
            }

            var legacyInventory = variantDef.variantInventory;

            VariantInventoryDefinition.AddressableEquipmentInfo equipmentInfo = new VariantInventoryDefinition.AddressableEquipmentInfo
            {
                equipmentDef = legacyInventory.equipmentInfo.equipment,
                aiMaxUseDistance = legacyInventory.equipmentInfo.aiMaxUseDistance,
                aiMaxUseHealthFraction = legacyInventory.equipmentInfo.aiMaxUseHealthFraction,
                canTriggerEquipment = legacyInventory.equipmentInfo.usable,
                timeBetweenEquipmentSwitches = float.PositiveInfinity,
            };
            List<AddressableItemCountPair> itemCountPairs = new List<AddressableItemCountPair>();

            for(int i = 0; i < legacyInventory.itemInventory.Length; i++)
            {
                var legacyItemCountPair = legacyInventory.itemInventory[i];

                itemCountPairs.Add(new AddressableItemCountPair
                {
                    count = legacyItemCountPair.amount,
                    itemDef = legacyItemCountPair.item
                });
            }

            return new VariantInventoryDefinition
            {
                equipmentInfo = equipmentInfo,
                itemsToGrant = itemCountPairs.ToArray()
            };
        }

        private static IVariantNameProvider CreateNameProvider(VariantDef variantDef)
        {
            if (variantDef.nameOverrides.Length == 0)
                return null;

            if(variantDef.nameOverrides.Length > 1)
            {
                LogWarning($"{variantDef} has multiple Name Overrides, this is no longer supported. Only the first Override will be upgraded.");
            }

            var firstNameOverride = variantDef.nameOverrides[0];

            switch (firstNameOverride.overrideType)
            {
                case OverrideNameType.Prefix:
                    return new VariantNamePrefix() { prefixToken = firstNameOverride.token };
                case OverrideNameType.Suffix:
                    return new VariantNameSuffix() { suffixToken = firstNameOverride.token };
                case OverrideNameType.Complete:
                    return new VariantNameOverride() { overrideToken = firstNameOverride.token };
            }

            LogWarning($"{variantDef}'s first Name Override had an invalid OverrideNameType, returning null NameProvider.");
            return null;
        }

        private static VariantDeathStateOverride CreateDeathStateOverride(VariantDef variantDef)
        {
            var result = new VariantDeathStateOverride();
            if(string.IsNullOrWhiteSpace(variantDef.deathStateOverride._typeName))
            {
                return result;
            }

            Type legacyDeath = variantDef.deathStateOverride.GetType();
            if(legacyDeath == null)
            {
                ThrowOrLogError(new TypeLoadException($"Could not retrieve DeathState type with name {variantDef.deathStateOverride._typeName}"));
                return result;
            }

            result.deathStateOverride = variantDef.deathStateOverride;
            return result;
        }

        private static VariantSkillReplacement[] CreateSkillReplacements(VariantDef variantDef)
        {
            List<VariantSkillReplacement> skillReplacements = new List<VariantSkillReplacement>();

            for (int i = 0; i < variantDef.skillReplacements.Length; i++)
            {
                VariantDef.VariantSkillReplacement legacySkillReplacement = variantDef.skillReplacements[i];
                skillReplacements.Add(new VariantSkillReplacement { skillDef = legacySkillReplacement.skillDef, slot = legacySkillReplacement.skillSlot });
                if(legacySkillReplacement.skillSlot == RoR2.SkillSlot.None)
                {
                    LogInfo($"Skill replacement at index {i} for {variantDef} has it's slot set to None, reminder that you can now specify a GenericSkill via it's name.");
                }
            }
            return skillReplacements.ToArray();
        }

        private static IVariantStatModifier CreateStatModifier(VariantDef variantDef)
        {
            return new BasicStatModifier
            {
                armorBonus = variantDef.armorBonus,
                armorMultiplier = variantDef.armorMultiplier,
                attackSpeedMultiplier = variantDef.attackSpeedMultiplier,
                damageMultiplier = variantDef.damageMultiplier,
                healthMultiplier = variantDef.healthMultiplier,
                moveSpeedMultiplier = variantDef.moveSpeedMultiplier,
                regenBonus = variantDef.regenBonus,
                regenMultiplier = variantDef.regenMultiplier,
                shieldBonus = variantDef.shieldBonus,
                shieldMultiplier = variantDef.shieldMultiplier,
            };
        }

        private static VariantBuffStorage CreateBuffStorage(VariantDef variantDef)
        {
            List<VariantBuffInfo> runtimeBuffInfos = new List<VariantBuffInfo>();
            var result = new VariantBuffStorage();
            if(!variantDef.variantInventory)
            {
                return result;
            }

            for(int i = 0; i < variantDef.variantInventory.buffInfos.Length; i++)
            {
                var legacyBuffInfo = variantDef.variantInventory.buffInfos[i];

                IVariantBuffInfoTimedApplication? timedApplication = null;
                if (!Mathf.Approximately(legacyBuffInfo.time, 0))
                {
                    timedApplication = new LegacyTimedBuffApplication { totalTimeForBuffs = legacyBuffInfo.time };
                }
                runtimeBuffInfos.Add(new VariantBuffInfo
                {
                    buffDef = legacyBuffInfo.buff,
                    count = legacyBuffInfo.amount,
                    timedApplicationImpl = timedApplication
                });
            }

            result.buffInfos = runtimeBuffInfos.ToArray();
            return result;
        }

        //TODO: do this
        private static VariantVisualModifier CreateVisualModifier(VariantDef variantDef)
        {
            throw new NotImplementedException();
        }

        private static VariantComponentCollection CreateComponentCollection(VariantDef variantDef)
        {
            var result = new VariantComponentCollection();

            if (variantDef.componentProviders.Length <= 0)
                return result;

            List<SerializableSystemType> components = new List<SerializableSystemType>();
            for(int i = 0; i < variantDef.componentProviders.Length; i++)
            {
                var legacyComponent = variantDef.componentProviders[i];
                Type componentType = ((Type)legacyComponent.componentToAdd);
                if(componentType == null)
                {
                    ThrowOrLogError(new TypeLoadException($"Component provider at index {i} for {variantDef} has componentToAddField that resolved to a null type."));
                    return result;
                }

                if(componentType.IsSubclassOf(typeof(VAPI.Legacy.Components.VariantComponent)))
                {
                    ThrowOrLogError(new InvalidOperationException($"Component provider at index {i} for {variantDef}'s componentToAddField resolved to a type that subclasses the legacy VariantComponent. Make it subclass the Runtime VariantComponent instead."));
                    return result;
                }

                components.Add(legacyComponent.componentToAdd);
            }
            result.variantComponents = components.ToArray();
            return result;
        }

        private static void LogInfo(string message)
        {
            _logBuilder.AppendLine(FormatStringForLog(message, "INFO"));
        }

        private static void LogWarning(string message)
        {
            _logBuilder.AppendLine(FormatStringForLog(message, "WARN"));
        }

        private static void LogError(string message)
        {
            _logBuilder.AppendLine(FormatStringForLog(message, "ERROR"));
        }

        private static void ThrowOrLogError(Exception exception)
        {
            if(_throwExceptions)
            {
                throw exception;
            }
            else
            {
                try
                {
                    throw exception;
                }
                catch(Exception ex)
                {
                    LogError($"Caught Exception: {ex}");
                }
            }
        }

        private static string FormatStringForLog(string message, string logLevelString)
        {
            return string.Format("[VariantUpgrader-{0}]: {1}", logLevelString, message);
        }
    }
}