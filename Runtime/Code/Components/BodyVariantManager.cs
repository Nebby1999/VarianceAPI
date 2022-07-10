using KinematicCharacterController;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI.Components
{
    public class BodyVariantManager : NetworkBehaviour
    {
        public ReadOnlyCollection<VariantDef> variantsInBody;

        public CharacterBody CharacterBody { get; private set; }
        public CharacterMaster CharacterMaster { get; private set; }
        public CharacterDeathBehavior CharacterDeathBehavior { get; private set; }
        public CharacterModel CharacterModel { get; private set; }

        private SyncListInt variantIndices = new SyncListInt();
        private bool applied = false;
        private EquipmentIndex storedEquip;
        private ItemDisplayRuleSet storedIDRS;

        #region Networking Related
        public override void OnStartClient()
        {
            variantIndices.Callback = UpdateList;
        }
        private void UpdateList(SyncList<int>.Operation op, int itemIndex)
        {
            variantsInBody = new ReadOnlyCollection<VariantDef>(variantIndices.Select(i => VariantCatalog.GetVariantDef((VariantIndex)i)).ToList());
        }

        [Server]
        public void AddVariants(VariantDef[] variantDefs)
        {
            foreach (VariantDef variant in variantDefs)
                AddVariant(variant);
        }

        [Server]
        public void AddVariant(VariantDef variant)
        {
            if (variant.VariantIndex == VariantIndex.None)
                throw new InvalidOperationException($"{variant} has a VariantIndex of None");

            AddVariantInternal(variant.VariantIndex);
        }

        [Server]
        public void RemoveVariant(VariantDef variant)
        {
            if (variant.VariantIndex == VariantIndex.None)
                throw new InvalidOperationException($"{variant} has a VariantIndex of None");

            RemoveVariantInternal(variant.VariantIndex);
        }

        private void RemoveVariantInternal(VariantIndex variant)
        {
            variantIndices.Remove((int)variant);
        }

        private void AddVariantInternal(VariantIndex index)
        {
            variantIndices.Add((int)index);
        }
        #endregion

        public void Awake()
        {
            CharacterBody = GetComponent<CharacterBody>();
            CharacterMaster = CharacterBody.master;
            CharacterDeathBehavior = GetComponent<CharacterDeathBehavior>();

            if (CharacterBody.modelLocator && CharacterBody.modelLocator.modelTransform)
                CharacterModel = CharacterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
        }

        public void Apply()
        {
            if (!CharacterBody)
            {
                Destroy(this);
                return;
            }

            var announcedArrival = false;
            for (int i = 0; i < variantsInBody.Count; i++)
            {
                VariantDef current = variantsInBody[i];
                try
                {
                    VariantTierDef tier = current.VariantTierDef;
                    if(!announcedArrival)
                        announcedArrival = AnnounceArrival(current, tier);

                    VariantInventory inventory = current.variantInventory;
                    if(inventory)
                    {
                        if (CharacterMaster && CharacterMaster.inventory)
                        {
                            inventory.AddItems(CharacterMaster.inventory);
                            inventory.SetEquipment(CharacterMaster.inventory, CharacterBody);
                            tier.AddTierItems(CharacterMaster.inventory);
                        }
                        inventory.AddBuffs(CharacterBody);
                    }

                    if (CharacterDeathBehavior && current.deathStateOverride.stateType != null)
                        CharacterDeathBehavior.deathState = current.deathStateOverride;

                    ModifySkills(current.skillReplacements);

                    ModifyStats(current);

                    VariantVisuals visuals = current.visualModifier;
                    if(visuals && CharacterModel)
                    {
                        visuals.ApplyMaterials(CharacterModel);
                        visuals.ApplyLights(CharacterModel);
                        
                        if(VAPIConfig.activateMeshReplacementSystem.Value)
                        {
                            if(visuals.ApplyMeshes(CharacterModel, out storedIDRS, out MeshType meshType))
                            {
                                if (meshType != MeshType.Default)
                                    TryFuckWithBoneStructure(meshType);
                            }
                        }
                    }

                    VariantSizeModifier sizeModifier = current.sizeModifier;
                    if(sizeModifier && CharacterModel)
                    {
                        sizeModifier.ApplySize(CharacterModel.transform, CharacterBody.GetComponentsInChildren<KinematicCharacterMotor>());
                    }

                    ModifyAI(current.aiModifier);

                    ModifyName(current.nameOverrides);

                    AddComponents(current.componentProviders);
                }
                catch(Exception e)
                {
                    VAPILog.Error($"Exception while trying to apply variant defs to {CharacterBody.GetDisplayName()}, {e}");
                }
            }
        }

        private bool AnnounceArrival(VariantDef variantDef, VariantTierDef tierDef)
        {
            bool announced = false;
            if(tierDef.announcesArrival)
            {
                if(!string.IsNullOrEmpty(variantDef.arrivalToken))
                {
                    Chat.AddMessage(Language.GetStringFormatted(variantDef.arrivalToken));
                }
                else
                {
                    VAPILog.Debug($"{variantDef}'s tier announces its arrival, but it doesnt have a token set, using generic message.");
                    Chat.AddMessage(Language.GetStringFormatted("GENERIC_ARRIVAL", CharacterBody.GetDisplayName()));
                }
                announced = true;
            }

            if(tierDef.soundEvent)
            {
                EffectManager.SimpleSoundEffect(tierDef.soundEvent.index, transform.position, true);
                announced = true;
            }
            return announced;
        }

        private void ModifySkills(VariantDef.VariantSkillReplacement[] skillReplacements)
        {
            SkillLocator skillLocator = CharacterBody.skillLocator;
            if(skillLocator)
            {
                foreach(VariantDef.VariantSkillReplacement skillReplacement in skillReplacements)
                {
                    switch(skillReplacement.skillSlot)
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

        private void ModifyStats(VariantDef variantDef)
        {
            CharacterBody.baseMaxHealth *= variantDef.healthMultiplier;
            CharacterBody.baseMoveSpeed *= variantDef.moveSpeedMultiplier;
            CharacterBody.baseAttackSpeed *= variantDef.attackSpeedMultiplier;
            CharacterBody.baseDamage *= variantDef.damageMultiplier;
            CharacterBody.baseArmor += variantDef.armorBonus;
            CharacterBody.baseArmor *= variantDef.armorMultiplier;
            CharacterBody.baseRegen += variantDef.regenBonus;
            CharacterBody.baseRegen *= variantDef.regenMultiplier;
            CharacterBody.baseMaxShield += variantDef.shieldBonus;
            CharacterBody.baseMaxShield *= variantDef.shieldMultiplier;
        }

        private void ModifyAI(BasicAIModifier aiModifier)
        {
            if (!CharacterMaster)
                return;
            
            foreach(AISkillDriver driver in CharacterMaster.GetComponents<AISkillDriver>())
            {
                if (driver)
                {
                    if (aiModifier.HasFlag(BasicAIModifier.Unstable))
                    {
                        driver.minTargetHealthFraction = Mathf.NegativeInfinity;
                        driver.maxTargetHealthFraction = Mathf.Infinity;
                        driver.minUserHealthFraction = Mathf.NegativeInfinity;
                        driver.maxUserHealthFraction = Mathf.Infinity;
                    }
                    if(aiModifier.HasFlag(BasicAIModifier.ForceSprint))
                    {
                        driver.shouldSprint = true;
                    }
                }
            }
        }

        private void ModifyName(VariantDef.VariantOverrideName[] overrideNames)
        {
            foreach(var overrideName in overrideNames)
            {
                switch(overrideName.overrideType)
                {
                    case OverrideNameType.Prefix:
                        CharacterBody.baseNameToken = Language.GetStringFormatted(overrideName.token) + " " + CharacterBody.GetDisplayName();
                        break;
                    case OverrideNameType.Suffix:
                        CharacterBody.baseNameToken = CharacterBody.GetDisplayName() + " " + Language.GetStringFormatted(overrideName.token);
                        break;
                    case OverrideNameType.Complete:
                        CharacterBody.baseNameToken = Language.GetStringFormatted(overrideName.token);
                        break;
                }
            }
        }

        private void AddComponents(VariantDef.VariantComponentProvider[] providers)
        {
            foreach(var component in providers)
            {
                Type typeToAdd = (Type)component.componentToAdd;

                switch(component.attachmentType)
                {
                    case ComponentAttachmentType.Body:
                        CharacterBody.gameObject.AddComponent(typeToAdd);
                        break;
                    case ComponentAttachmentType.Master:
                        if (CharacterMaster)
                            CharacterMaster.gameObject.AddComponent(typeToAdd);
                        break;
                    case ComponentAttachmentType.Model:
                        if (CharacterModel)
                            CharacterModel.gameObject.AddComponent(typeToAdd);
                        break;
                }
            }
        }

        #region Mesh Replacement Jank
        private void TryFuckWithBoneStructure(MeshType meshType)
        {
            if (CharacterMaster && CharacterMaster.inventory)
            {
                storedEquip = CharacterMaster.inventory.GetEquipmentIndex();
                CharacterMaster.inventory.SetEquipmentIndex(EquipmentIndex.None);
                Invoke(nameof(RestoreEquipment), 0.2f);
            }

            switch(meshType)
            {
                case MeshType.Default:
                    break;
                case MeshType.Beetle:
                    BeetleMeshReplacement();
                    break;
                case MeshType.BeetleGuard:
                    BeetleGuardMeshReplacement();
                    break;
                case MeshType.MiniMushrum:
                    MiniMushrumMeshReplacement();
                    break;
                case MeshType.MagmaWorm:
                    MagmaWormMeshReplacement();
                    break;
                case MeshType.OverloadingWorm:
                    OverloadingWormMeshReplacement();
                    break;
            }
        }

        private void BeetleMeshReplacement()
        {
            List<Transform> transforms = new List<Transform>();
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
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
            foreach (var item in CharacterBody.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.bones = transforms.ToArray();
            }
        }

        private void BeetleGuardMeshReplacement()
        {
            List<Transform> t = new List<Transform>();
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
            {
                if (!item.name.Contains("Hurtbox") && !item.name.Contains("IK") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in CharacterBody.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.bones = t.ToArray();
            }
        }

        private void MiniMushrumMeshReplacement()
        {
            List<Transform> t = new List<Transform>();
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
            {
                if (!item.name.Contains("Hurtbox") && !item.name.Contains("IK") && !item.name.Contains("_end") && !item.name.Contains("miniMush_R_Palps_02"))
                {
                    t.Add(item);
                }
            }
            for (int i = 0; i < 7; i++)
            {
                t.RemoveAt(t.Count - 1);
            }
            foreach (var item in CharacterBody.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.bones = t.ToArray();
            }
        }

        private void MagmaWormMeshReplacement()
        {
            List<Transform> t = new List<Transform>();
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Head") && !item.name.Contains("_end") && !item.name.Contains("Center"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Jaw") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("eye.") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Neck") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in CharacterBody.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.bones = t.ToArray();
            }
        }

        private void OverloadingWormMeshReplacement()
        {
            List<Transform> t = new List<Transform>();
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Head") && !item.name.Contains("_end") && !item.name.Contains("Center"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Jaw") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("eye.") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in CharacterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Neck") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in CharacterBody.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.bones = t.ToArray();
            }
        }

        private void RestoreEquipment()
        {
            CharacterModel.itemDisplayRuleSet = storedIDRS;

            CharacterMaster.inventory.SetEquipmentIndex(storedEquip);
        }
        #endregion
    }
}
