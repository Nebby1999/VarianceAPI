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
    public class VariantHandler : NetworkBehaviour
    {
        public VariantInfo[] VariantInfos;

        public VariantInventoryInfo[] InventoryInfos;
        public VariantVisualModifier[] VisualModifiers;
        public VariantInfo.VariantSkillReplacement[] SkillReplacements;
        public VariantInfo.VariantExtraComponent[] ExtraComponents;
        public VariantInfo.VariantOverrideName[] ExtraNames;
        public SerializableEntityStateType[] DeathStates;

        public VariantAIModifier aiModifiers;
        public VariantTier highestTier;

        public CharacterBody charBody;
        public CharacterMaster charMaster;
        public CharacterDeathBehavior charDeathBehavior;
        public CharacterModel charModel;
        public ItemDef purpleHealthbar;
        public EquipmentIndex storedEquipment;
        public ItemDisplayRuleSet storedIDRS;

        public float healthModifier = 1;
        public float moveSpeedModifier = 1;
        public float attackSpeedModifier = 1;
        public float damageModifier = 1;
        public float armorModifier = 1;
        public float sizeModifier = 1;
        public float armorBonus = 0;

        public bool scaleColliders;


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

            MergeVariantInfos();
            AnnounceArrival();
            ModifyAI();
            ModifyStats();

            foreach (VariantInventoryInfo inventoryInfo in InventoryInfos)
            {
                AddItems(inventoryInfo.ItemInventory);
                AddBuffs(inventoryInfo.Buffs);
            }
            GiveEquipment();
            
            if (highestTier >= VariantTier.Uncommon)
            {
                charMaster.inventory.GiveItem(purpleHealthbar);
            }

            foreach(VariantVisualModifier visualModifier in VisualModifiers)
            {
                ReplaceMaterials(visualModifier.MaterialReplacements);
                ReplaceMesh(visualModifier.MeshReplacements);
                ReplaceLights(visualModifier.LightReplacements);
            }

            foreach(VariantInfo.VariantSkillReplacement skillReplacement in SkillReplacements)
            {
                ReplaceSkill(skillReplacement);
            }
            
            foreach(VariantInfo.VariantExtraComponent extraComponent in ExtraComponents)
            {
                AddComponent(extraComponent);
            }

            foreach(VariantInfo.VariantOverrideName overrideName in ExtraNames)
            {
                AddName(overrideName);
            }
            
            foreach(SerializableEntityStateType state in DeathStates)
            {
                ReplaceDeathState(state);
            }

            ScaleVariant();

            charBody.healthComponent.health = charBody.healthComponent.fullHealth;
            charBody.RecalculateStats();
        }

        #region Announce Arrival
        private void AnnounceArrival()
        {
            var randomVariantInfo = VariantInfos.Where(x => x.variantTier >= VariantTier.Rare).PickRandom();
            if (randomVariantInfo)
            {
                if (highestTier >= VariantTier.Rare)
                {
                    Debug.Log("Announcing Arrival");
                    if (!string.IsNullOrEmpty(randomVariantInfo.arrivalMessage))
                    {
                        Chat.AddMessage(Language.GetStringFormatted(randomVariantInfo.arrivalMessage));
                    }
                    else
                    {
                        VAPILog.LogD($"{randomVariantInfo.identifier} Variant is Rare or Legendary, but doesnt have an arrival message set! using generic message.");
                        Chat.AddMessage($"A {charBody.GetDisplayName()} with unique qualities has appeared!");
                    }
                }
            }
        }
        #endregion

        #region Modify AI
        private void ModifyAI()
        {
            Debug.Log("Starting AI modification");
            if (aiModifiers.HasFlag(VariantAIModifier.Unstable))
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

            if (aiModifiers.HasFlag(VariantAIModifier.ForceSprint))
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
        private void ModifyStats()
        {
            Debug.Log("Starting Stat modification");
            charBody.baseMaxHealth *= healthModifier;
            charBody.baseMoveSpeed *= moveSpeedModifier;
            charBody.baseAttackSpeed *= attackSpeedModifier;
            charBody.baseDamage *= damageModifier;
            charBody.levelDamage = charBody.baseDamage * 0.2f;
            charBody.baseArmor *= armorModifier;
            charBody.baseArmor += armorBonus;
        }
        #endregion

        #region Add Items
        private void AddItems(VariantInventoryInfo.VariantInventory[] variantInventory)
        {
            foreach (var itemInventory in variantInventory)
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
        private void AddBuffs(VariantInventoryInfo.VariantBuff[] buffs)
        {
            foreach (var buffInfo in buffs)
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
        private void GiveEquipment()
        {
            var randomVariantInfo = VariantInfos.Where(x => x.variantInventory != null).PickRandom();
            if (randomVariantInfo)
            {
                var inventory = randomVariantInfo.variantInventory;
                if (inventory.equipmentDefName != "")
                {
                    EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(inventory.equipmentDefName));
                    if (equipmentDef)
                    {
                        charMaster.inventory.GiveEquipmentString(inventory.equipmentDefName);
                        gameObject.AddComponent<VariantEquipmentHandler>().animationCurve = inventory.fireCurve;
                    }
                    else
                    {
                        VAPILog.LogW($"Could not find EquipmentDef with the name {inventory.equipmentDefName}");
                    }
                }
            }
        }
        #endregion

        #region Replace Materials
        private void ReplaceMaterials(VariantVisualModifier.VariantMaterialReplacement[] materialReplacements)
        {
            if (materialReplacements != null)
            {
                for (int i = 0; i < materialReplacements.Length; i++)
                {
                    var current = materialReplacements[i];
                    charModel.baseRendererInfos[current.rendererIndex].defaultMaterial = current.material;
                }
            }
        }
        #endregion

        #region Replace Mesh
        private void ReplaceMesh(VariantVisualModifier.VariantMeshReplacement[] meshReplacements)
        {
            if (meshReplacements != null)
            {
                for (int i = 0; i < meshReplacements.Length; i++)
                {
                    var current = meshReplacements[i];
                    storedIDRS = charModel.itemDisplayRuleSet;
                    charModel.itemDisplayRuleSet = null;
                    TryFuckWithBoneStructure(current);
                    charModel.baseRendererInfos[current.rendererIndex].renderer.GetComponent<SkinnedMeshRenderer>().sharedMesh = current.mesh;
                }
            }
        }
        #endregion

        #region Replace Lights
        private void ReplaceLights(VariantVisualModifier.VariantLightReplacement[] lightReplacements)
        {
            if (lightReplacements != null)
            {
                for (int k = 0; k < lightReplacements.Length; k++)
                {
                    var current = lightReplacements[k];
                    charModel.baseLightInfos[current.rendererIndex].defaultColor = current.color;
                    charModel.baseLightInfos[current.rendererIndex].light.type = current.lightType;
                }
            }
        }
        #endregion

        #region Replace Skill
        private void ReplaceSkill(VariantInfo.VariantSkillReplacement skillReplacement)
        {
            SkillLocator skillLocator = charBody.skillLocator;
            if(skillLocator)
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
        #endregion

        #region Add Component
        private void AddComponent(VariantInfo.VariantExtraComponent extraComponent)
        {
            switch(extraComponent.componentType)
            {
                case ComponentType.Body:
                    VAPILog.LogI($"Adding {extraComponent.componentToAdd.componentType.Name} Component to {charBody.gameObject}");
                    charBody.gameObject.AddComponent(extraComponent.componentToAdd.componentType);
                    break;
                case ComponentType.Master:
                    VAPILog.LogI($"Adding {extraComponent.componentToAdd.componentType.Name} Component to {charMaster.gameObject}");
                    charMaster.gameObject.AddComponent(extraComponent.componentToAdd.componentType);
                    break;
                case ComponentType.Model:
                    VAPILog.LogI($"Adding {extraComponent.componentToAdd.componentType.Name} Component to {charModel.gameObject}");
                    charModel.gameObject.AddComponent(extraComponent.componentToAdd.componentType);
                    break;
            }
        }
        #endregion

        #region Add Name
        private void AddName(VariantInfo.VariantOverrideName overrideName)
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
        #endregion

        #region Replace Death State
        private void ReplaceDeathState(SerializableEntityStateType deathState)
        {
            if (deathState.typeName != null)
                charDeathBehavior.deathState = deathState;
        }
        #endregion

        #region Scale Variant
        private void ScaleVariant()
        {
            charBody.modelLocator.modelTransform.localScale *= sizeModifier;
            if (scaleColliders)
            {
                foreach (KinematicCharacterMotor kinematicCharacterMotor in charBody.GetComponentsInChildren<KinematicCharacterMotor>())
                    if (kinematicCharacterMotor) kinematicCharacterMotor.SetCapsuleDimensions(kinematicCharacterMotor.Capsule.radius * this.sizeModifier, kinematicCharacterMotor.CapsuleHeight * sizeModifier, sizeModifier);
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

            switch(meshReplacement.meshType)
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

                charMaster.inventory.SetEquipmentIndex(storedEquipment);
            }
        }
        #endregion

        #region Merge
        private void MergeVariantInfos()
        {
            var healthMultiplier = 1f;
            var moveSpeedMultiplier = 1f;
            var attackSpeedMultiplier = 1f;
            var damageMultiplier = 1f;
            var armorMultiplier = 1f;
            var sizeMultiplier = 1f;
            foreach (var variantInfo in VariantInfos)
            {
                //If ai modifier is not present, add it as a flag.
                if (!aiModifiers.HasFlag(variantInfo.aiModifier))
                    aiModifiers |= variantInfo.aiModifier;

                //Creates the final modifiers based on the given variantInfos.
                healthMultiplier *= variantInfo.healthMultiplier;
                moveSpeedMultiplier *= variantInfo.moveSpeedMultiplier;
                attackSpeedMultiplier *= variantInfo.attackSpeedMultiplier;
                damageMultiplier *= variantInfo.damageMultiplier;
                armorMultiplier *= variantInfo.armorMultiplier;
                if(variantInfo.sizeModifier)
                {
                    sizeMultiplier *= variantInfo.sizeModifier.newSize;
                    scaleColliders = variantInfo.sizeModifier.scaleCollider;
                }
                armorBonus += variantInfo.armorBonus;


                //Adds all the inventories to the inventoryInfos
                if (variantInfo.variantInventory)
                {
                    HG.ArrayUtils.ArrayAppend(ref InventoryInfos, variantInfo.variantInventory);
                }

                //Sets the highest variantTier
                if(highestTier < variantInfo.variantTier)
                {
                    highestTier = variantInfo.variantTier;
                }

                //Adds all the visual modifiers to the visual modifiers
                if(variantInfo.visualModifier)
                {
                    HG.ArrayUtils.ArrayAppend(ref VisualModifiers, variantInfo.visualModifier);
                }

                //Adds all the skill replacements to the Skill Replacements
                if(variantInfo.skillReplacements != null)
                {
                    foreach(var thing in variantInfo.skillReplacements)
                    {
                        HG.ArrayUtils.ArrayAppend(ref SkillReplacements, thing);
                    }
                }

                //Adds extra components
                if(variantInfo.extraComponents != null)
                {
                    foreach(var thing in variantInfo.extraComponents)
                    {
                        HG.ArrayUtils.ArrayAppend(ref ExtraComponents, thing);
                    }
                }

                //Adds extra names
                if (variantInfo.overrideNames != null)
                {
                    foreach (var thing in variantInfo.overrideNames)
                    {
                        HG.ArrayUtils.ArrayAppend(ref ExtraNames, thing);
                    }
                }
                if(variantInfo.customDeathState.typeName != string.Empty)
                {
                    HG.ArrayUtils.ArrayAppend(ref DeathStates, variantInfo.customDeathState);
                }
            }
            healthModifier = healthMultiplier;
            moveSpeedModifier = moveSpeedMultiplier;
            attackSpeedModifier = attackSpeedMultiplier;
            damageModifier = damageMultiplier;
            armorModifier = armorMultiplier;
            sizeModifier = sizeMultiplier;
        }
        #endregion
    }
}