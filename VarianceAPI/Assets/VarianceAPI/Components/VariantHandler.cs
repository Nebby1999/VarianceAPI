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
using Random = UnityEngine.Random;

namespace VarianceAPI.Components
{
    public class VariantHandler : MonoBehaviour
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

        public float healthModifier = 1;
        public float moveSpeedModifier = 1;
        public float attackSpeedModifier = 1;
        public float damageModifier = 1;
        public float armorModifier = 1;
        public float sizeModifier = 1;
        public float armorBonus = 0;

        public bool scaleColliders;


        public void Start()
        {
            charBody = gameObject.GetComponent<CharacterBody>();
            charModel = charBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            charMaster = charBody.master;
            charDeathBehavior = gameObject.GetComponent<CharacterDeathBehavior>();
        }

        public void Modify()
        {
            if (!charBody)
                Destroy(this);


            MergeVariantInfos();

            #region Announce Arrival
            var randomVariantInfo = GetRandomVariantInfo(VariantInfos.Where(x => x.variantTier >= VariantTier.Rare).ToList());
            if(highestTier >= VariantTier.Rare)
            {
                if(!string.IsNullOrEmpty(randomVariantInfo.arrivalMessage))
                {
                    Chat.AddMessage(randomVariantInfo.arrivalMessage);
                }
                else
                {
                    VAPILog.LogD($"{randomVariantInfo.identifier} Variant is Rare or Legendary, but doesnt have an arrival message set! using generic message.");
                    Chat.AddMessage($"A {charBody.GetDisplayName()} with unique qualities has appeared!");
                }
            }
            #endregion

            #region AI Modification
            if(aiModifiers.HasFlag(VariantAIModifier.Unstable))
            {
                foreach(AISkillDriver i in charMaster.GetComponents<AISkillDriver>())
                {
                    if(i)
                    {
                        i.minTargetHealthFraction = Mathf.NegativeInfinity;
                        i.maxTargetHealthFraction = Mathf.Infinity;
                        i.minUserHealthFraction = Mathf.NegativeInfinity;
                        i.maxUserHealthFraction = Mathf.Infinity;
                    }
                }
            }

            if(aiModifiers.HasFlag(VariantAIModifier.ForceSprint))
            {
                foreach (AISkillDriver i in charMaster.GetComponents<AISkillDriver>())
                {
                    if (i)
                    {
                        i.shouldSprint = true;
                    }
                }
            }
            #endregion

            #region Stat modification
            charBody.baseMaxHealth *= healthModifier;
            charBody.baseMoveSpeed *= moveSpeedModifier;
            charBody.baseAttackSpeed *= attackSpeedModifier;
            charBody.baseDamage *= damageModifier;
            charBody.levelDamage = charBody.baseDamage * 0.2f;
            charBody.baseArmor *= armorModifier;
            charBody.baseArmor += armorBonus;
            #endregion

            #region Item, Buff and Equipment addition
            foreach (VariantInventoryInfo inventoryInfo in InventoryInfos)
            {
                #region Item Addition
                foreach(var itemInventory in inventoryInfo.ItemInventory)
                {
                    string itemDefName = itemInventory.itemDefName;
                    int amount = itemInventory.amount;

                    bool giveItem = true;
                    if(itemDefName == "ExtraLife")
                    {
                        if(charMaster.GetComponent<PreventRecursion>())
                        {
                            giveItem = false;
                        }
                        else
                        {
                            charMaster.gameObject.AddComponent<PreventRecursion>();
                        }
                    }
                    if(giveItem)
                    {
                        ItemDef itemDef = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(itemDefName));
                        if(itemDef != null && amount > 0)
                        {
                            charMaster.inventory.GiveItem(itemDef.itemIndex, amount);
                        }
                    }
                }
                #endregion

                #region Buff Addition
                foreach(var buffInfo in inventoryInfo.Buffs)
                {
                    int amount = buffInfo.amount;
                    float time = buffInfo.time;
                    string buffDefName = buffInfo.buffDefName;

                    BuffDef buffDef = BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex(buffDefName));
                    if(buffDef)
                    {
                        if(time > 0)
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
                #endregion
            }
            #region Equipment Addition
            var randomEquipment = GetRandomVariantInfo(VariantInfos.Where(x=> x.variantInventory && x.variantInventory.equipmentDefName != string.Empty).ToList()).variantInventory;
            if(randomEquipment)
            {
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(randomEquipment.equipmentDefName));
                if(equipmentDef)
                {
                    charMaster.inventory.GiveEquipmentString(randomEquipment.equipmentDefName);
                    gameObject.AddComponent<VariantEquipmentHandler>().animationCurve = randomEquipment.fireCurve;
                }
                else
                {
                    VAPILog.LogW($"Could not find EquipmentDef with the name {randomEquipment.equipmentDefName}");
                }
            }
            #endregion
            //Adds purple healthbar
            if (highestTier >= VariantTier.Uncommon)
            {
                //this.master.inventory.GiveItem(ContentPackProvider.contentPack.itemDefs.Find("VAPI_PurpleHealthbar"));
            }
            #endregion

            var VisualModifier = GetRandomVisualModifier(VisualModifiers);
            #region Material replacements
            if(VisualModifier.MaterialReplacements != null)
            {
                for (int i = 0; i < VisualModifier.MaterialReplacements.Length; i++)
                {
                    var current = VisualModifier.MaterialReplacements[i];
                    charModel.baseRendererInfos[current.rendererIndex].defaultMaterial = current.material;
                }
            }
            #endregion

            #region Mesh Replacements
            if(VisualModifier.MeshReplacements != null)
            {
                for(int i = 0; i < VisualModifier.MeshReplacements.Length; i++)
                {
                    var current = VisualModifier.MeshReplacements[i];
                    charModel.baseRendererInfos[current.rendererIndex].renderer.GetComponent<SkinnedMeshRenderer>().sharedMesh = current.mesh;
                }
            }
            #endregion

            #region Light Replacements
            if(VisualModifier.LightReplacements != null)
            {
                for(int i = 0; i < VisualModifier.LightReplacements.Length; i++)
                {
                    var current = VisualModifier.LightReplacements[i];
                    var lightInfo = charModel.baseLightInfos[current.rendererIndex];
                    lightInfo.defaultColor = current.color;
                    lightInfo.light.type = current.lightType;
                }
            }
            #endregion

            #region Skill Replacements

            SkillLocator skillLocator = charBody.skillLocator;

            if(skillLocator)
            {
                var list = new List<VariantInfo.VariantSkillReplacement>();
                list.Add(GetRandomSkillReplacement(SkillSlot.Primary));
                list.Add(GetRandomSkillReplacement(SkillSlot.Secondary));
                list.Add(GetRandomSkillReplacement(SkillSlot.Utility));
                list.Add(GetRandomSkillReplacement(SkillSlot.Special));

                foreach(var value in list)
                {
                    switch(value.skillSlot)
                    {
                        case SkillSlot.Primary:
                            skillLocator.primary.SetSkillOverride(this.gameObject, value.skillDef, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.Secondary:
                            skillLocator.secondary.SetSkillOverride(this.gameObject, value.skillDef, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.Utility:
                            skillLocator.utility.SetSkillOverride(this.gameObject, value.skillDef, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.Special:
                            skillLocator.special.SetSkillOverride(this.gameObject, value.skillDef, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.None:
                            //lol
                            break;
                    }
                }
            }
            #endregion

            #region Extra Components
            for(int i = 0; i < ExtraComponents.Length; i++)
            {
                VariantInfo.VariantExtraComponent extraComponent = ExtraComponents[i];
                if (extraComponent.isAesthetic)
                {
                    ModelLocator modelLocator = charBody.GetComponent<ModelLocator>();
                    if (modelLocator)
                    {
                        Transform modelTransform = modelLocator.modelTransform;
                        if (modelTransform)
                        {
                            Type type;
                            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(Assembly => Assembly.GetReferencedAssemblies().Any(AssName => AssName.FullName == typeof(VariantHandler).AssemblyQualifiedName));
                            if (assemblies != null)
                            {
                                foreach (var assembly in assemblies)
                                {
                                    type = assembly.GetType(extraComponent.componentToAdd);
                                    if (type != null)
                                    {
                                        if (typeof(VariantComponent).IsAssignableFrom(type))
                                        {
                                            VAPILog.LogI("Adding " + type.Name + " Component to " + charBody.GetDisplayName());
                                            modelTransform.gameObject.AddComponent(type);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    VAPILog.LogI("Variant Components that are not aesthetic have yet to be implemented, we're sorry for this inconvenience.");
                }
            }
            #endregion

            #region Name Modification
            foreach(var overrideName in ExtraNames)
            {
                switch(overrideName.overrideType)
                {
                    case OverrideNameType.Preffix:
                        charBody.baseNameToken = overrideName.textToAdd + " " + charBody.GetDisplayName();
                        return;
                    case OverrideNameType.Suffix:
                        charBody.baseNameToken = charBody.GetDisplayName() + " " + overrideName.textToAdd;
                        return;
                    case OverrideNameType.CompleteOverride:
                        charBody.baseNameToken = overrideName.textToAdd;
                        return;
                }
            }
            #endregion

            #region New Death State
            var DeathState = DeathStates[Random.Range(0, DeathStates.Length)];
            charDeathBehavior.deathState = DeathState;
            #endregion

            #region Scale Body
            charBody.modelLocator.modelTransform.localScale *= sizeModifier;
            if(scaleColliders)
            {
                foreach (KinematicCharacterMotor kinematicCharacterMotor in charBody.GetComponentsInChildren<KinematicCharacterMotor>())
                    if (kinematicCharacterMotor) kinematicCharacterMotor.SetCapsuleDimensions(kinematicCharacterMotor.Capsule.radius * this.sizeModifier, kinematicCharacterMotor.CapsuleHeight * sizeModifier, sizeModifier);
            }
            #endregion

            charBody.healthComponent.health = charBody.healthComponent.fullHealth;
            charBody.RecalculateStats();
        }

        private VariantInfo.VariantSkillReplacement GetRandomSkillReplacement(SkillSlot skillSlot)
        {
            var availableSkills = SkillReplacements.Where(x => x.skillSlot == skillSlot).ToArray();
            var rng = Random.Range(0, availableSkills.Length);
            return availableSkills[rng];
        }
        private VariantVisualModifier GetRandomVisualModifier(VariantVisualModifier[] visualModifiers)
        {
            var rng = Random.Range(0, visualModifiers.Length);
            return visualModifiers[rng];
        }
        private VariantInfo GetRandomVariantInfo(List<VariantInfo> specificVariantInfos = null)
        {
            if (specificVariantInfos != null)
            {
                var rng = Random.Range(0, specificVariantInfos.Count);
                return specificVariantInfos[rng];
            }
            else
            {
                var rng = Random.Range(0, VariantInfos.Length);
                return VariantInfos[rng];
            }
        }
        private void MergeVariantInfos()
        {
            foreach (var variantInfo in VariantInfos)
            {
                //If ai modifier is not present, add it as a flag.
                if (!aiModifiers.HasFlag(variantInfo.aiModifier))
                    aiModifiers |= variantInfo.aiModifier;

                //Creates the final modifiers based on the given variantInfos.
                healthModifier += variantInfo.healthMultiplier - 1;
                moveSpeedModifier += variantInfo.moveSpeedMultiplier - 1;
                attackSpeedModifier += variantInfo.attackSpeedMultiplier - 1;
                damageModifier += variantInfo.damageMultiplier - 1;
                armorModifier += variantInfo.armorMultiplier - 1;
                if(variantInfo.sizeModifier)
                {
                    sizeModifier += variantInfo.sizeModifier.newSize - 1;
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
        }
    }
}