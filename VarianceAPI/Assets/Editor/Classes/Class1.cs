using EntityStates;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VarianceAPI.ScriptableObjects;

public class VariantInfoCreator : MonoBehaviour
{
    public static string Path;

    public static string assetNamePrefix;
    public static void CreateNerVariantInfo(VarianceAPI.Scriptables.VariantInfo v)
    {
        Path = "";
        CreateFolder(v);
        var newInfo = ScriptableObject.CreateInstance<VariantInfo>();
        newInfo.name = v.name;
        assetNamePrefix = newInfo.name.Replace(".asset", "");
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
        Debug.Log(newInfo.name);
        AssetDatabase.CreateAsset(newInfo, Path + "\\" + newInfo.name + ".asset");
        AssetDatabase.Refresh();
    }

    private static string CreateFolder(VarianceAPI.Scriptables.VariantInfo v)
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
            path = path.Replace($"/{v.name}.asset", string.Empty);
        }
        if(path.Length > 0)
        {
            if(!Directory.Exists(Path))
            {
                Debug.Log("Pain");
                var thing = AssetDatabase.CreateFolder(path, "Migrated");
                Path = AssetDatabase.GUIDToAssetPath(thing);
                Debug.Log(Path);
                AssetDatabase.Refresh();
            }
            Selection.activeObject = obj;
            return Path;
        }
        Selection.activeObject = obj;
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
        int flag = 0;
        toReturn.name = assetNamePrefix + "_VariantInventory.asset";
        if(itemInventory != null)
        {
            toReturn.ItemInventory = CreateItemInventory(itemInventory);
        }
        else
        {
            flag++;
        }
        if(variantBuffs.Length != 0)
        {
            toReturn.Buffs = CreateBuffInventory(variantBuffs);
        }
        else
        {
            flag++;
        }
        if(equipmentInfo != null)
        {
            toReturn.equipmentDefName = equipmentInfo.equipmentString;
            toReturn.fireCurve = equipmentInfo.animationCurve;
        }
        else
        {
            flag++;
        }
        Debug.Log("Variant inventory flag count: " + flag);
        if(flag >= 3)
        {
            return null;
        }
        AssetDatabase.CreateAsset(toReturn, Path + "\\" + toReturn.name);
        return toReturn;

    }
    private static VariantInventoryInfo.VariantInventory[] CreateItemInventory(VarianceAPI.Scriptables.VariantInventory itemInventory)
    {
        var toReturn = new List<VariantInventoryInfo.VariantInventory>();
        for (int i = 0; i < itemInventory.counts.Length; i++)
        {
            var toAdd = new VariantInventoryInfo.VariantInventory();
            toAdd.amount = itemInventory.counts[i];
            if(itemInventory.itemStrings[i].StartsWith("VAPI_"))
            {
                itemInventory.itemStrings[i] = itemInventory.itemStrings[i].Replace("VAPI_", string.Empty);
            }
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
        toReturn.name = assetNamePrefix + "_VisualModifier.asset";
        int flag = 0;
        if(materialReplacements.Length != 0)
        {
            toReturn.MaterialReplacements = CreateMaterialReplacements(materialReplacements);
        }
        else
        {
            flag++;
        }
        if(lightReplacements.Length != 0)
        {
            toReturn.LightReplacements = CreateLightReplacements(lightReplacements);
        }
        else
        {
            flag++;
        }
        if(meshReplacements.Length != 0)
        {
            toReturn.MeshReplacements = CreateMeshReplacements(meshReplacements);
        }
        else
        {
            flag++;
        }

        Debug.Log("Variant visual modifier flag count: " + flag);
        if(flag >= 3)
        {
            return null;
        }

        AssetDatabase.CreateAsset(toReturn, Path + "\\" + toReturn.name);

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
        if(sizeModifier == null)
        {
            return null;
        }
        var toReturn = ScriptableObject.CreateInstance<VariantSizeModifier>();
        toReturn.name = assetNamePrefix + "_SizeModifier.asset";
        toReturn.newSize = sizeModifier.newSize;
        toReturn.scaleCollider = sizeModifier.scaleCollider;

        AssetDatabase.CreateAsset(toReturn, Path + "\\" + toReturn.name);

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
