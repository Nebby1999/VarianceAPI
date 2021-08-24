/*using RoR2;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VarianceAPI.Components;
using VarianceAPI.Scriptables;
using Logger = VarianceAPI.MainClass;

namespace VarianceAPI.Modules
{
    /// <summary>
    /// Class containing multiple methods that help with the creation of variants
    /// </summary>
    public class Helpers
    {
        /// <summary>
        /// Helper for replacing the material in rendererIndex 0
        /// </summary>
        /// <param name="newMaterial"> The new material the variant</param>
        /// <returns></returns>
        public static VariantMaterialReplacement[] SimpleMaterialReplacement(Material newMaterial)
        {
            return SimpleMaterialReplacement(newMaterial, 0);
        }

        /// <summary>
        /// Helper for replacing a material in a specific rendererIndex
        /// </summary>
        /// <param name="newMaterial">The new material the variant uses</param>
        /// <param name="index">What index is targeted</param>
        /// <returns></returns>
        public static VariantMaterialReplacement[] SimpleMaterialReplacement(Material newMaterial, int index)
        {
            VariantMaterialReplacement replacement = ScriptableObject.CreateInstance<VariantMaterialReplacement>();
            replacement.rendererIndex = index;
            replacement.material = newMaterial;

            VariantMaterialReplacement[] matReplacement = new VariantMaterialReplacement[]
            {
                replacement
            };

            return matReplacement;
        }

        /// <summary>
        /// Helper for replacing a material in a specific rendererIndex
        /// </summary>
        /// <param name="newMaterial">The new material the variant uses</param>
        /// <param name="index">What index is targeted</param>
        /// <returns></returns>
        public static VariantMaterialReplacement SingleMaterialReplacement(Material newMaterial, int index)
        {
            VariantMaterialReplacement replacement = ScriptableObject.CreateInstance<VariantMaterialReplacement>();
            replacement.rendererIndex = index;
            replacement.material = newMaterial;

            return replacement;
        }

        /// <summary>
        /// Helper for replacing multiple materials
        /// </summary>
        /// <param name="newMaterials">dictionary with rendererindex, material. refer to the wiki page on the Helper class for an example on how to use</param>
        /// <returns></returns>
        public static VariantMaterialReplacement[] MultiMaterialReplacement(Dictionary<int, Material> newMaterials)
        {
            List<VariantMaterialReplacement> matReplacement = new List<VariantMaterialReplacement>();
            foreach (KeyValuePair<int, Material> kvp in newMaterials)
            {
                VariantMaterialReplacement replacement = SingleMaterialReplacement(kvp.Value, kvp.Key);
                matReplacement.Add(replacement);
            }

            return matReplacement.ToArray();
        }

        /// <summary>
        /// Helper for replacing the mesh in rendererIndex 0
        /// </summary>
        /// <param name="newMesh"> The new mesh of the variant</param>
        /// <returns></returns>
        public static VariantMeshReplacement[] SimpleMeshReplacement(Mesh newMesh)
        {
            return SimpleMeshReplacement(newMesh, 0);
        }

        /// <summary>
        /// Helper for replacing a mesh in a specific rendererIndex
        /// </summary>
        /// <param name="newMesh">The new mesh the variant uses</param>
        /// <param name="index">What index is targeted</param>
        /// <returns></returns>
        public static VariantMeshReplacement[] SimpleMeshReplacement(Mesh newMesh, int index)
        {
            VariantMeshReplacement replacement = ScriptableObject.CreateInstance<VariantMeshReplacement>();
            replacement.rendererIndex = index;
            replacement.mesh = newMesh;

            VariantMeshReplacement[] meshReplacement = new VariantMeshReplacement[]
            {
                replacement
            };

            return meshReplacement;
        }

        /// <summary>
        /// Helper for replacing a mesh in a specific rendererIndex
        /// </summary>
        /// <param name="newMesh">The new mesh the variant uses</param>
        /// <param name="index">What index is targeted</param>
        /// <returns></returns>
        public static VariantMeshReplacement SingleMeshReplacement(Mesh newMesh, int index)
        {
            VariantMeshReplacement replacement = ScriptableObject.CreateInstance<VariantMeshReplacement>();
            replacement.rendererIndex = index;
            replacement.mesh = newMesh;

            return replacement;
        }

        /// <summary>
        /// Method for replacing a variant's skill
        /// </summary>
        /// <param name="skillSlot">Skill slot to target</param>
        /// <param name="skill">Replacement Skill</param>
        /// <returns></returns>
        public static VariantSkillReplacement SingleSkillReplacement(SkillSlot skillSlot, SkillDef skill)
        {
            VariantSkillReplacement replacement = ScriptableObject.CreateInstance<VariantSkillReplacement>();
            replacement.skillSlot = skillSlot;
            replacement.skillDef = skill;
            return replacement;
        }

        /// <summary>
        /// Method for replacing a variant's primary skill
        /// </summary>
        /// <param name="skill">Skilldef to use</param>
        /// <returns></returns>
        public static VariantSkillReplacement[] PrimaryReplacement(SkillDef skill)
        {
            VariantSkillReplacement skillReplacement = ScriptableObject.CreateInstance<VariantSkillReplacement>();
            skillReplacement.skillSlot = SkillSlot.Primary;
            skillReplacement.skillDef = skill;

            return new VariantSkillReplacement[]
            {
                skillReplacement
            };
        }

        /// <summary>
        /// Method for replacing a variant's secondary skill
        /// </summary>
        /// <param name="skill">Skilldef to use</param>
        /// <returns></returns>
        public static VariantSkillReplacement[] SecondaryReplacement(SkillDef skill)
        {
            VariantSkillReplacement skillReplacement = ScriptableObject.CreateInstance<VariantSkillReplacement>();
            skillReplacement.skillSlot = SkillSlot.Secondary;
            skillReplacement.skillDef = skill;

            return new VariantSkillReplacement[]
            {
                skillReplacement
            };
        }

        /// <summary>
        /// Method for replacing a variant's utility skill
        /// </summary>
        /// <param name="skill">Skilldef to use</param>
        /// <returns></returns>
        public static VariantSkillReplacement[] UtilityReplacement(SkillDef skill)
        {
            VariantSkillReplacement skillReplacement = ScriptableObject.CreateInstance<VariantSkillReplacement>();
            skillReplacement.skillSlot = SkillSlot.Utility;
            skillReplacement.skillDef = skill;

            return new VariantSkillReplacement[]
            {
                skillReplacement
            };
        }

        /// <summary>
        /// Method for replacing a variant's Special skill
        /// </summary>
        /// <param name="skill">Skilldef to use</param>
        /// <returns></returns>
        public static VariantSkillReplacement[] SpecialReplacement(SkillDef skill)
        {
            VariantSkillReplacement skillReplacement = ScriptableObject.CreateInstance<VariantSkillReplacement>();
            skillReplacement.skillSlot = SkillSlot.Special;
            skillReplacement.skillDef = skill;

            return new VariantSkillReplacement[]
            {
                skillReplacement
            };
        }

        /// <summary>
        /// Method to replace multiple skills 
        /// </summary>
        /// <param name="skillReplacements">dictionary with SkillSlot, SkillDef. refer to the wiki page on the Helper class for an example on how to use</param>
        /// <returns></returns>
        public static VariantSkillReplacement[] MultiSkillReplacement(Dictionary<SkillSlot, SkillDef> skillReplacements)
        {
            List<VariantSkillReplacement> skillReplacement = new List<VariantSkillReplacement>();

            foreach (KeyValuePair<SkillSlot, SkillDef> kvp in skillReplacements)
            {
                VariantSkillReplacement replacement = SingleSkillReplacement(kvp.Key, kvp.Value);
                skillReplacement.Add(replacement);
            }

            return skillReplacement.ToArray();
        }

        /// <summary>
        /// Helper for changing a Grounded variant's size.
        /// <para>Do not use this for flying variants, use FlyingSizeModifier instead</para>
        /// </summary>
        /// <param name="newSize">New Size, where 1.0 = 100% Base Size</param>
        /// <returns></returns>
        public static VariantSizeModifier GroundSizeModifier(float newSize)
        {
            VariantSizeModifier sizeModifier = ScriptableObject.CreateInstance<VariantSizeModifier>();
            sizeModifier.newSize = newSize;
            sizeModifier.scaleCollider = false;

            return sizeModifier;
        }

        /// <summary>
        /// Helper for changing a Flying variant's Size
        /// <para>Do not use this for grounded variants, use GroundSizeModifier instead</para>
        /// </summary>
        /// <param name="newSize">New Size, where 1.0 = 100% Base Size</param>
        /// <returns></returns>
        public static VariantSizeModifier FlyingSizeModifier(float newSize)
        {
            VariantSizeModifier sizeModifier = ScriptableObject.CreateInstance<VariantSizeModifier>();
            sizeModifier.newSize = newSize;
            sizeModifier.scaleCollider = true;

            return sizeModifier;
        }

        public static VariantConfig CreateConfig(VariantInfo variantInfo)
        {
            VariantConfig config = ScriptableObject.CreateInstance<VariantConfig>();
            config.identifier = variantInfo.identifierName;
            config.spawnRate = variantInfo.spawnRate;
            config.isUnique = variantInfo.unique;
            if (variantInfo.isModded)
            {
                Logger.Log.LogError("Error! Given variant info is for a modded variant! use CreateModdedConfig instead!");
                return null;
            }
            return config;
        }
        public static VariantConfig CreateModdedConfig(VariantInfo variantInfo, string modAuthor, string modName)
        {
            VariantConfig config = ScriptableObject.CreateInstance<VariantConfig>();
            config.identifier = variantInfo.identifierName;
            config.spawnRate = variantInfo.spawnRate;
            config.isUnique = variantInfo.unique;
            config.isModded = variantInfo.isModded;
            config.modAuthor = modAuthor;
            config.modName = modName;
            if (!variantInfo.isModded)
            {
                Logger.Log.LogError("Error! Given variant info is for a vanilla variant! use CreateConfig instead!");
                return null;
            }
            return config;
        }

        public static VariantOverrideName[] CreateOverrideName(string textToAdd, OverrideNameType overrideNameType)
        {
            List<VariantOverrideName> thingToReturn = new List<VariantOverrideName>();

            VariantOverrideName variantOverrideName = ScriptableObject.CreateInstance<VariantOverrideName>();
            variantOverrideName.textToAdd = textToAdd;
            variantOverrideName.overrideType = overrideNameType;

            return thingToReturn.ToArray();
        }
        public static VariantOverrideName[] CreateOverrideName(string firstText, OverrideNameType firstOverrideType, string secondText, OverrideNameType secondOverrideType)
        {
            List<VariantOverrideName> thingToReturn = new List<VariantOverrideName>();

            VariantOverrideName firstVariantOverrideName = ScriptableObject.CreateInstance<VariantOverrideName>();
            firstVariantOverrideName.textToAdd = firstText;
            firstVariantOverrideName.overrideType = firstOverrideType;
            thingToReturn.Add(firstVariantOverrideName);

            VariantOverrideName secondVariantOverrideName = ScriptableObject.CreateInstance<VariantOverrideName>();
            secondVariantOverrideName.textToAdd = secondText;
            secondVariantOverrideName.overrideType = secondOverrideType;
            thingToReturn.Add(secondVariantOverrideName);

            return thingToReturn.ToArray();
        }

        public static CustomVariantReward CreateVariantReward(float goldMultiplier, uint bonusGold, float xpMultiplier, uint bonusXP, float whiteChance, float greenChance, float redChance)
        {
            CustomVariantReward customVariantReward = ScriptableObject.CreateInstance<CustomVariantReward>();
            customVariantReward.goldMultiplier = goldMultiplier;
            customVariantReward.goldBonus = bonusGold;
            customVariantReward.xpMultiplier = xpMultiplier;
            customVariantReward.xpBonus = bonusXP;
            customVariantReward.whiteItemChance = whiteChance;
            customVariantReward.greenItemChance = greenChance;
            customVariantReward.redItemChance = redChance;

            return customVariantReward;
        }

        public static CustomVariantReward CreateVariantReward(float goldMultiplier, float xpMultiplier, float whiteChance, float greenChance, float redChance)
        {
            CustomVariantReward customVariantReward = ScriptableObject.CreateInstance<CustomVariantReward>();
            customVariantReward.goldMultiplier = goldMultiplier;
            customVariantReward.xpMultiplier = xpMultiplier;
            customVariantReward.whiteItemChance = whiteChance;
            customVariantReward.greenItemChance = greenChance;
            customVariantReward.redItemChance = redChance;

            return customVariantReward;
        }
        /// <summary>
        /// Main method to add new variants
        /// </summary>
        /// <param name="info">the specific information for the variant.</param>
        public static void AddVariant(VariantInfo info)
        {
            AddVariant(Resources.Load<GameObject>("Prefabs/CharacterBodies/" + info.bodyName + "Body"), info);
        }

        /// <summary>
        /// Method that adds a new variant to the game
        /// </summary>
        /// <param name="bodyPrefab">the body prefab minus the Body part, for example, to create an Imp Overlord variant you would type "ImpBoss" instead of "ImpBossBody"</param>
        /// <param name="info">The specific information for the variant</param>
        public static void AddVariant(GameObject bodyPrefab, VariantInfo info)
        {
            if(!bodyPrefab)
            {
                Logger.Log.LogError("Failed to add Variant! the body prefab could not be found...");
                return;
            }
            Logger.Log.LogMessage("Adding " + info.identifierName + " VariantHandler to the BodyPrefab of name " + info.bodyName + "Body!");
            VariantHandler variantHandler = bodyPrefab.AddComponent<VariantHandler>();
            variantHandler.Init(info);
        }

        /// <summary>
        /// Method that adds a modded variant to the game
        /// <para>if the body is not found then it throws an error and no variant is created</para>
        /// </summary>
        /// <param name="info">The speccific information for the Variant</param>
        public static void AddModdedVariant(VariantInfo info)
        {
            On.RoR2.BodyCatalog.Init += (orig) =>
            {
                orig();

                GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(info.bodyName + "Body");
                if(!bodyPrefab)
                {
                    Logger.Log.LogError("Failed to add variant! " + info.bodyName + "Body does not exist. (Are you missing a mod?)");
                    Debug.LogError("Variance API: Failed to add variant: " + info.bodyName + "Body does not exist.");
                    return;
                }
                Logger.Log.LogMessage("Adding " + info.identifierName + " VariantHandler to the BodyPrefab of name " + info.bodyName + "Body!");
                bodyPrefab.AddComponent<VariantHandler>().Init(info);
            };
        }
    }
}*/