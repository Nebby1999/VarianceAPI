/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VarianceAPI.ScriptableObjects;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using UnityEditor;
using EntityStates;

public class VariantInfoCreator : MonoBehaviour
{
    public static string Path
    {
        get
        {
            return CreateFolder();
        }
    }
    public static void CreateNerVariantInfo(VarianceAPI.Scriptables.VariantInfo v)
    {
        var newInfo = ScriptableObject.CreateInstance<VariantInfo>();
        newInfo.identifier = v.identifierName;
        newInfo.bodyName = v.bodyName + "Body";
        newInfo.overrideNames = CreateOverrideNames(v.overrideName);
        newInfo.unique = v.variantConfig.isUnique;
        newInfo.spawnRate = v.variantConfig.spawnRate;
        newInfo.aiModifier = v.aiModifier;
        newInfo.variantTier = v.variantTier;
        newInfo.givesRewards = v.givesRewards;
        newInfo.variantInventory = CreateNewVariantInventory(v.variantInventory, v.buff, v.customEquipment);
        newInfo.skillReplacements = CreateSkillReplacements(v.skillReplacement);
        newInfo.healthMultiplier = v.healthMultiplier;
        newInfo.moveSpeedMultiplier = v.moveSpeedMultiplier;
        newInfo.attackSpeedMultiplier = v.attackSpeedMultiplier;
        newInfo.damageMultiplier = v.damageMultiplier;
        newInfo.armorMultiplier = v.armorMultiplier;
        newInfo.armorBonus = v.armorBonus;
        newInfo.visualModifier = CreateNewVisualModifier(v.materialReplacement, v.lightReplacement, v.meshReplacement);
        newInfo.sizeModifier = CreateNewSizeModifier(v.sizeModifier);
        newInfo.arrivalMessage = v.arrivalMessage;
        newInfo.extraComponents = CreateNewExtraComponents(v.extraComponents);
        var newDeathState = new SerializableEntityStateType();
        newDeathState.typeName = v.customDeathState;
        newInfo.customDeathState = newDeathState;

        AssetDatabase.CreateAsset(newInfo, Path);
    }

    private static string CreateFolder()
    {
        var path = "";
        var obj = Selection.activeObject;
        if(obj == null)
        {
            path = "Assets";
        }
        else
        {
            path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
        }
        path = path + "NewVariantInfo";
        if(path.Length > 0)
        {
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        return "Assets";
    }
    private static VariantInfo.VariantOverrideName[] CreateOverrideNames(VarianceAPI.Scriptables.VariantOverrideName[] overrideNames)
    {
        var toReturn = new List<VariantInfo.VariantOverrideName>();

        foreach (var thing in overrideNames)
        {
            var toAdd = new VariantInfo.VariantOverrideName();
            toAdd.overrideType = thing.overrideType;
            toAdd.textToAdd = thing.textToAdd;
            toReturn.Add(toAdd);
        }
        return toReturn.ToArray();
    }

    private static VariantInventoryInfo CreateNewVariantInventory(VarianceAPI.Scriptables.VariantInventory itemInventory, VarianceAPI.Scriptables.VariantBuff[] variantBuffs, VarianceAPI.Scriptables.EquipmentInfo equipmentInfo)
    {
        var toReturn = ScriptableObject.CreateInstance<VariantInventoryInfo>();
        toReturn.ItemInventory = CreateItemInventory(itemInventory);
        toReturn.Buffs = CreateBuffInventory(variantBuffs);
        toReturn.equipmentDefName = equipmentInfo.equipmentString;
        toReturn.fireCurve = equipmentInfo.animationCurve;

        AssetDatabase.CreateAsset(toReturn, Path);
        return toReturn;

    }
    private static VariantInventoryInfo.VariantInventory[] CreateItemInventory(VarianceAPI.Scriptables.VariantInventory itemInventory)
    {
        var toReturn = new List<VariantInventoryInfo.VariantInventory>();
        for (int i = 0; i < itemInventory.counts.Length; i++)
        {
            var toAdd = new VariantInventoryInfo.VariantInventory();
            toAdd.amount = itemInventory.counts[i];
            toAdd.itemDefName = itemInventory.itemStrings[i];
            toReturn.Add(toAdd);
        }
        return toReturn.ToArray();
    }

    private static VariantInventoryInfo.VariantBuff[] CreateBuffInventory(VarianceAPI.Scriptables.VariantBuff[] variantBuffs)
    {
        var toReturn = new List<VariantInventoryInfo.VariantBuff>();
        for (int i = 0; i < variantBuffs.Length; i++)
        {
            var toAdd = new VariantInventoryInfo.VariantBuff();
            toAdd.amount = variantBuffs[i].stacks;
            toAdd.buffDefName = variantBuffs[i].buffDef;
            toAdd.time = variantBuffs[i].time;

            toReturn.Add(toAdd);
        }
        return toReturn.ToArray();
    }
    private static VariantInfo.VariantSkillReplacement[] CreateSkillReplacements(VarianceAPI.Scriptables.VariantSkillReplacement[] skillReplacements)
    {
        var toReturn = new List<VariantInfo.VariantSkillReplacement>();
        for(int i = 0; i < skillReplacements.Length; i++)
        {
            var toAdd = new VariantInfo.VariantSkillReplacement();
            toAdd.skillDef = skillReplacements[i].skillDef;
            toAdd.skillSlot = skillReplacements[i].skillSlot;
            toReturn.Add(toAdd);
        }
        return toReturn.ToArray();
    }

    private static VariantVisualModifier CreateNewVisualModifier(VarianceAPI.Scriptables.VariantMaterialReplacement[] materialReplacements, VarianceAPI.Scriptables.VariantLightReplacement[] lightReplacements, VarianceAPI.Scriptables.VariantMeshReplacement[] meshReplacements)
    {
        var toReturn = ScriptableObject.CreateInstance<VariantVisualModifier>();
        toReturn.MaterialReplacements = CreateMaterialReplacements(materialReplacements);
        toReturn.LightReplacements = CreateLightReplacements(lightReplacements);
        toReturn.MeshReplacements = CreateMeshReplacements(meshReplacements);

        AssetDatabase.CreateAsset(toReturn, Path);

        return toReturn;
    }

    private static VariantVisualModifier.VariantMaterialReplacement[] CreateMaterialReplacements(VarianceAPI.Scriptables.VariantMaterialReplacement[] materialReplacements)
    {
        var toReturn = new List<VariantVisualModifier.VariantMaterialReplacement>();
        for (int i = 0; i < materialReplacements.Length; i++)
        {
            var toAdd = new VariantVisualModifier.VariantMaterialReplacement();
            toAdd.ElementName = materialReplacements[i].name;
            toAdd.identifier = materialReplacements[i].identifier;
            toAdd.material = materialReplacements[i].material;
            toAdd.rendererIndex = materialReplacements[i].rendererIndex;

            toReturn.Add(toAdd);
        }
        return toReturn.ToArray();
    }

    private static VariantVisualModifier.VariantLightReplacement[] CreateLightReplacements(VarianceAPI.Scriptables.VariantLightReplacement[] lightReplacements)
    {
        var toReturn = new List<VariantVisualModifier.VariantLightReplacement>();
        for (int i = 0; i < lightReplacements.Length; i++)
        {
            var toAdd = new VariantVisualModifier.VariantLightReplacement();
            toAdd.ElementName = lightReplacements[i].name;
            toAdd.color = lightReplacements[i].color;
            toAdd.lightType = LightType.Spot;
            toAdd.rendererIndex = lightReplacements[i].rendererIndex;

            toReturn.Add(toAdd);
        }
        return toReturn.ToArray();
    }

    private static VariantVisualModifier.VariantMeshReplacement[] CreateMeshReplacements(VarianceAPI.Scriptables.VariantMeshReplacement[] meshReplacements)
    {
        var toReturn = new List<VariantVisualModifier.VariantMeshReplacement>();
        for (int i = 0; i < meshReplacements.Length; i++)
        {
            var toAdd = new VariantVisualModifier.VariantMeshReplacement();
            toAdd.ElementName = meshReplacements[i].name;
            toAdd.mesh = meshReplacements[i].mesh;
            toAdd.meshType = meshReplacements[i].meshType;
            toAdd.rendererIndex = meshReplacements[i].rendererIndex;

            toReturn.Add(toAdd);
        }
        return toReturn.ToArray();
    }

    private static VariantSizeModifier CreateNewSizeModifier(VarianceAPI.Scriptables.VariantSizeModifier sizeModifier)
    {
        var toReturn = ScriptableObject.CreateInstance<VariantSizeModifier>();
        toReturn.newSize = sizeModifier.newSize;
        toReturn.scaleCollider = sizeModifier.scaleCollider;

        AssetDatabase.CreateAsset(toReturn, Path);

        return toReturn;
    }

    private static VariantInfo.VariantExtraComponent[] CreateNewExtraComponents(VarianceAPI.Scriptables.VariantExtraComponent[] extraComponents)
    {
        var toReturn = new List<VariantInfo.VariantExtraComponent>();
        for(int i = 0; i < extraComponents.Length; i++)
        {
            var toAdd = new VariantInfo.VariantExtraComponent();
            toAdd.componentToAdd = extraComponents[i].componentToAdd;
            toAdd.isAesthetic = extraComponents[i].isAesthetic;

            toReturn.Add(toAdd);
        }

        return toReturn.ToArray();
    }
}
*/