using EntityStates;
using KinematicCharacterController;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VarianceAPI.ScriptableObjects;
using VarianceAPI.Utils;
using Random = UnityEngine.Random;

namespace VarianceAPI.Components
{
    public class VariantHandler : MonoBehaviour
    {
        public VariantInfo[] VariantInfos;

        public CharacterBody charBody;
        public CharacterMaster charMaster;
        public CharacterDeathBehavior charDeathBehavior;
        public CharacterModel charModel;
        public ItemDef purpleHealthbar;
        public EquipmentIndex storedEquipment;
        public ItemDisplayRuleSet storedIDRS;


        public void GetComponents()
        {
            charBody = gameObject.GetComponent<CharacterBody>();
            charModel = charBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            charMaster = charBody.master;
            charDeathBehavior = gameObject.GetComponent<CharacterDeathBehavior>();
            purpleHealthbar = Assets.VAPIAssets.LoadAsset<ItemDef>("PurpleHealthbar");
        }

        public void Modify()
        {
            GetComponents();
            if (!charBody)
                Destroy(this);

            var announcedArrival = false;
            for (int i = 0; i < VariantInfos.Length; i++)
            {
                var current = VariantInfos[i];
                if(ConfigLoader.EnableVariantArrivalAnnouncements.Value)
                {
                    if (current.variantTier >= VariantTier.Rare && !announcedArrival)
                    {
                        announcedArrival = true;
                            AnnounceArrival(current);
                    }
                }

                ModifyStats(current);

                if (NetworkServer.active)
                {
                    if (current.variantInventory != null)
                    {
                            AddItems(current.variantInventory);
                            AddBuffs(current.variantInventory);
                            GiveEquipment(current.variantInventory);
                    }
                    if (current.variantTier >= VariantTier.Uncommon)
                    {
                        charMaster.inventory.GiveItem(purpleHealthbar);
                    }
                }

                if (current.visualModifier != null)
                {
                        ReplaceMaterials(current.visualModifier);
                        ReplaceLights(current.visualModifier);
                        ReplaceMesh(current.visualModifier);
                }

                ReplaceSkill(current);

                AddComponent(current);

                AddName(current);

                ReplaceDeathState(current);

                ScaleVariant(current);

                if(NetworkServer.active)
                    charBody.healthComponent.health = charBody.healthComponent.fullHealth;

                charBody.RecalculateStats();

                ModifyAI(current);
            }
        }

        #region Announce Arrival
        private void AnnounceArrival(VariantInfo variantInfo)
        {
            if (variantInfo)
            {
                if (variantInfo.variantTier >= VariantTier.Rare)
                {
                    if (!string.IsNullOrEmpty(variantInfo.arrivalMessage))
                    {
                        Chat.AddMessage(Language.GetStringFormatted(variantInfo.arrivalMessage));
                    }
                    else
                    {
                        VAPILog.LogD($"{variantInfo.identifier} Variant is Rare or Legendary, but doesnt have an arrival message set! using generic message.");
                        Chat.AddMessage($"A {charBody.GetDisplayName()} with unique qualities has appeared!");
                    }
                }
            }
        }
        #endregion

        #region Modify AI
        private void ModifyAI(VariantInfo variantInfo)
        {
            if (variantInfo.aiModifier.HasFlag(VariantAIModifier.Unstable))
            {
                foreach (AISkillDriver i in charMaster.GetComponents<AISkillDriver>())
                {
                    if (i)
                    {
                        i.minTargetHealthFraction = Mathf.NegativeInfinity;
                        i.maxTargetHealthFraction = Mathf.Infinity;
                        i.minUserHealthFraction = Mathf.NegativeInfinity;
                        i.maxUserHealthFraction = Mathf.Infinity;
                    }
                }
            }

            if (variantInfo.aiModifier.HasFlag(VariantAIModifier.ForceSprint))
            {
                foreach (AISkillDriver i in charMaster.GetComponents<AISkillDriver>())
                {
                    if (i)
                    {
                        i.shouldSprint = true;
                    }
                }
            }
        }
        #endregion

        #region Modify Stats
        private void ModifyStats(VariantInfo variantInfo)
        {
            charBody.baseMaxHealth *= variantInfo.healthMultiplier;
            charBody.baseMoveSpeed *= variantInfo.moveSpeedMultiplier;
            charBody.baseAttackSpeed *= variantInfo.attackSpeedMultiplier;
            charBody.baseDamage *= variantInfo.damageMultiplier;
            charBody.levelDamage = charBody.baseDamage * 0.2f;
            charBody.baseArmor *= variantInfo.armorMultiplier;
            charBody.baseArmor += variantInfo.armorBonus;
        }
        #endregion

        #region Add Items
        private void AddItems(VariantInventoryInfo variantInventory)
        {
            foreach (var itemInventory in variantInventory.ItemInventory)
            {
                string itemDefName = itemInventory.itemDefName;
                int amount = itemInventory.amount;

                bool giveItem = true;
                if (itemDefName == "ExtraLife")
                {
                    if (charMaster.GetComponent<PreventRecursion>())
                    {
                        giveItem = false;
                    }
                    else
                    {
                        charMaster.gameObject.AddComponent<PreventRecursion>();
                    }
                }
                if (giveItem)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(itemDefName));
                    if (itemDef != null && amount > 0)
                    {
                        charMaster.inventory.GiveItem(itemDef.itemIndex, amount);
                    }
                }
            }
        }
        #endregion

        #region Add Buffs
        private void AddBuffs(VariantInventoryInfo variantInventory)
        {
            foreach (var buffInfo in variantInventory.Buffs)
            {
                int amount = buffInfo.amount;
                float time = buffInfo.time;
                string buffDefName = buffInfo.buffDefName;

                BuffDef buffDef = BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex(buffDefName));
                if (buffDef)
                {
                    if (time > 0)
                    {
                        charBody.AddTimedBuff(buffDef, time, amount);
                    }
                    else
                    {
                        charBody.AddBuff(buffDef);
                    }
                }
                else
                {
                    VAPILog.LogW($"Could not find BuffDef with the name {buffDefName}.");
                }
            }
        }
        #endregion

        #region Give Equipment
        private void GiveEquipment(VariantInventoryInfo inventoryInfo)
        {
            if (inventoryInfo.equipmentDefName != "")
            {
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(inventoryInfo.equipmentDefName));
                if (equipmentDef)
                {
                    charMaster.inventory.GiveEquipmentString(inventoryInfo.equipmentDefName);

                    var equipHandler = gameObject.GetComponent<VariantEquipmentHandler>();
                    if (!equipHandler)
                        gameObject.AddComponent<VariantEquipmentHandler>().animationCurve = inventoryInfo.fireCurve;
                    else
                        equipHandler.animationCurve = inventoryInfo.fireCurve;
                }
                else
                {
                    VAPILog.LogW($"Could not find EquipmentDef with the name {inventoryInfo.equipmentDefName}");
                }
            }
        }
        #endregion

        #region Replace Materials
        private void ReplaceMaterials(VariantVisualModifier visualModifier)
        {
            for (int i = 0; i < visualModifier.MaterialReplacements.Length; i++)
            {
                var current = visualModifier.MaterialReplacements[i];
                charModel.baseRendererInfos[current.rendererIndex].defaultMaterial = current.material;
            }
        }
        #endregion

        #region Replace Mesh
        private void ReplaceMesh(VariantVisualModifier visualModifier)
        {
            for (int i = 0; i < visualModifier.MeshReplacements.Length; i++)
            {
                var current = visualModifier.MeshReplacements[i];
                storedIDRS = charModel.itemDisplayRuleSet;
                charModel.itemDisplayRuleSet = null;
                TryFuckWithBoneStructure(current);
                charModel.baseRendererInfos[current.rendererIndex].renderer.GetComponent<SkinnedMeshRenderer>().sharedMesh = current.mesh;
            }
        }
        #endregion

        #region Replace Lights
        private void ReplaceLights(VariantVisualModifier visualModifier)
        {
            for (int i = 0; i < visualModifier.LightReplacements.Length; i++)
            {
                var current = visualModifier.LightReplacements[i];
                charModel.baseLightInfos[current.rendererIndex].defaultColor = current.color;
                charModel.baseLightInfos[current.rendererIndex].light.type = current.lightType;
            }
        }
        #endregion

        #region Replace Skill
        private void ReplaceSkill(VariantInfo variantInfo)
        {
            SkillLocator skillLocator = charBody.skillLocator;
            if (skillLocator)
            {
                foreach (var skillReplacement in variantInfo.skillReplacements)
                {
                    switch (skillReplacement.skillSlot)
                    {
                        case SkillSlot.Primary:
                            skillLocator.primary?.SetSkillOverride(gameObject, skillReplacement.skillDef, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.Secondary:
                            skillLocator.secondary?.SetSkillOverride(gameObject, skillReplacement.skillDef, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.Utility:
                            skillLocator.utility?.SetSkillOverride(gameObject, skillReplacement.skillDef, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.Special:
                            skillLocator.special?.SetSkillOverride(gameObject, skillReplacement.skillDef, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.None:
                            break;
                    }
                }
            }
        }
        #endregion

        #region Add Component
        private void AddComponent(VariantInfo variantInfo)
        {
            foreach (var extraComponent in variantInfo.extraComponents)
            {
                Type typeToAdd = (Type)extraComponent.componentToAdd;

                switch (extraComponent.componentType)
                {
                    case ComponentType.Body:
                        VAPILog.LogI($"Adding {typeToAdd.Name} Component to {charBody.gameObject}");
                        charBody.gameObject.AddComponent(typeToAdd);
                        break;
                    case ComponentType.Master:
                        VAPILog.LogI($"Adding {typeToAdd} Component to {charMaster.gameObject}");
                        charMaster.gameObject.AddComponent(typeToAdd);
                        break;
                    case ComponentType.Model:
                        VAPILog.LogI($"Adding {typeToAdd} Component to {charModel.gameObject}");
                        charModel.gameObject.AddComponent(typeToAdd);
                        break;
                }
            }
        }
        #endregion

        #region Add Name
        private void AddName(VariantInfo variantInfo)
        {
            foreach (var overrideName in variantInfo.overrideNames)
            {
                switch (overrideName.overrideType)
                {
                    case OverrideNameType.Preffix:
                        charBody.baseNameToken = Language.GetStringFormatted(overrideName.textToAdd) + " " + charBody.GetDisplayName();
                        break;
                    case OverrideNameType.Suffix:
                        charBody.baseNameToken = charBody.GetDisplayName() + " " + Language.GetStringFormatted(overrideName.textToAdd);
                        break;
                    case OverrideNameType.CompleteOverride:
                        charBody.baseNameToken = Language.GetStringFormatted(overrideName.textToAdd);
                        break;
                }
            }
        }
        #endregion

        #region Replace Death State
        private void ReplaceDeathState(VariantInfo variantInfo)
        {
            SerializableEntityStateType deathState = variantInfo.customDeathState;

            if (deathState.typeName != string.Empty)
                charDeathBehavior.deathState = deathState;
        }
        #endregion

        #region Scale Variant
        private void ScaleVariant(VariantInfo variantInfo)
        {
            var sizeMod = variantInfo.sizeModifier;
            if(sizeMod != null)
            {
                charBody.modelLocator.modelTransform.localScale *= sizeMod.newSize;
                if (sizeMod.scaleCollider)
                {
                    foreach (KinematicCharacterMotor kinematicCharacterMotor in charBody.GetComponentsInChildren<KinematicCharacterMotor>())
                        if (kinematicCharacterMotor) kinematicCharacterMotor.SetCapsuleDimensions(kinematicCharacterMotor.Capsule.radius * sizeMod.newSize, kinematicCharacterMotor.CapsuleHeight * sizeMod.newSize, sizeMod.newSize);
                }
            }
        }
        #endregion

        //Bellow arent necesarily related to variant stuff.

        #region Mesh Replacement Crazyness
        private void TryFuckWithBoneStructure(VariantVisualModifier.VariantMeshReplacement meshReplacement)
        {
            storedEquipment = charMaster.inventory.GetEquipmentIndex();
            charMaster.inventory.SetEquipmentIndex(EquipmentIndex.None);
            Invoke("RestoreEquipment", 0.2f);

            switch (meshReplacement.meshType)
            {
                case MeshType.Default:
                    break;
                case MeshType.BeetleGuard:
                    FuckBoneStructure(MeshType.BeetleGuard);
                    break;
            }
        }
        private void FuckBoneStructure(MeshType meshType)
        {
            if (meshType == MeshType.Beetle)
            {
                List<Transform> transforms = new List<Transform>();
                foreach (var item in charBody.GetComponentsInChildren<Transform>())
                {
                    if (!item.name.Contains("Hurtbox") && !item.name.Contains("BeetleBody") && !item.name.Contains("Mesh") && !item.name.Contains("mdl"))
                    {
                        transforms.Add(item);
                    }
                }

                Transform temp = transforms[14];
                transforms[14] = transforms[11];
                transforms[11] = temp;
                temp = transforms[15];
                transforms[15] = transforms[12];
                transforms[12] = temp;
                temp = transforms[16];
                transforms[16] = transforms[13];
                transforms[13] = temp;
                foreach (var item in charBody.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    item.bones = transforms.ToArray();
                }
            }
            else if (meshType == MeshType.BeetleGuard)
            {
                List<Transform> transforms = new List<Transform>();
                foreach (var item in charBody.GetComponentsInChildren<Transform>())
                {
                    if (!item.name.Contains("Hurtbox") && !item.name.Contains("IK") && !item.name.Contains("_end"))
                    {
                        transforms.Add(item);
                    }
                }
                foreach (var item in charBody.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    item.bones = transforms.ToArray();
                }
            }
            else if (meshType == MeshType.MiniMushrum)
            {
                List<Transform> transforms = new List<Transform>();
                foreach (var item in charBody.GetComponentsInChildren<Transform>())
                {
                    if (!item.name.Contains("Hurtbox") && !item.name.Contains("IK") && !item.name.Contains("_end") && !item.name.Contains("miniMush_R_Palps_02"))
                    {
                        transforms.Add(item);
                    }
                }
                for (int i = 0; i < 7; i++)
                {
                    transforms.RemoveAt(transforms.Count - 1);
                }
                foreach (var item in charBody.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    item.bones = transforms.ToArray();
                }
            }
        }
        private void RestoreEquipment()
        {
            ModelLocator modelLocator = charBody.GetComponent<ModelLocator>();
            if (modelLocator)
            {

                if (charModel && storedIDRS != null)
                {
                    charModel.itemDisplayRuleSet = storedIDRS;
                }

                if(NetworkServer.active)
                    charMaster.inventory.SetEquipmentIndex(storedEquipment);
            }
        }
        #endregion
    }
}