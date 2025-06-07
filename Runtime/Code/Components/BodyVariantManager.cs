using KinematicCharacterController;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI.Components
{
    /// <summary>
    /// The NetworkBehaviour that transforms a regular body into a variant
    /// </summary>
    public class BodyVariantManager : NetworkBehaviour
    {
        /// <summary>
        /// The VariantDefs in the body
        /// </summary>
        public ReadOnlyCollection<VariantDef> variantsInBody;

        /// <summary>
        /// The Variant's CharacterBody
        /// </summary>
        public CharacterBody characterBody { get; private set; }
        /// <summary>
        /// The Variant's CharacterMaster
        /// </summary>
        public CharacterMaster characterMaster { get => characterBody.master; }
        /// <summary>
        /// The Variant's CharacterDeathBehaviour
        /// </summary>
        public CharacterDeathBehavior characterDeathBehavior { get; private set; }
        /// <summary>
        /// The Variant's CharacterModel
        /// </summary>
        public CharacterModel characterModel { get; private set; }

        /// <summary>
        /// Wether the VariantDefs get applied on Start
        /// </summary>
        public bool applyOnStart = true;

        private readonly SyncListInt variantIndices = new SyncListInt();
        private readonly List<VariantVisuals> visualsForCoroutine = new List<VariantVisuals>();
        private bool hasApplied = false;
        private EquipmentIndex storedEquip;
        private ItemDisplayRuleSet storedIDRS;
        private bool modelSkinControllerFinished;

        #region Networking Related
        /// <summary>
        /// Adds a list of Variants to the BodyVariantManager
        /// </summary>
        /// <param name="vd">The variants to add</param>
        public void AddVariants(IEnumerable<VariantDef> vd) => vd.ToList().ForEach(v => AddVariant(v));

        /// <summary>
        /// Adds a single Variant to the BodyVariantManager
        /// </summary>
        /// <param name="vd">The variant to add</param>
        public void AddVariant(VariantDef vd)
        {
            if(vd)
            {
                variantIndices.Add((int)vd.variantIndex);
            }
        }

        private void OnListChanged(SyncList<int>.Operation op, int index)
        {
            variantsInBody = new ReadOnlyCollection<VariantDef>(variantIndices.Select(i => VariantCatalog.GetVariantDef((VariantIndex)i)).ToList());
        }
        #endregion

        private void Awake()
        {
            variantIndices.Callback = OnListChanged;

            characterBody = GetComponent<CharacterBody>();
            characterDeathBehavior = GetComponent<CharacterDeathBehavior>();

            if (characterBody.modelLocator && characterBody.modelLocator.modelTransform)
                characterModel = characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();

            if(characterModel.TryGetComponent<ModelSkinController>(out var mdlSkinController))
            {
                mdlSkinController.onSkinApplied += onSkinApplied;
                return;
            }
            modelSkinControllerFinished = true;
        }

        private void onSkinApplied(int obj)
        {
            modelSkinControllerFinished = true;
        }

        private void Start()
        {
            if (applyOnStart && !hasApplied)
                Apply();
        }

        /// <summary>
        /// Applies the VariantDefs to this body
        /// </summary>
        public void Apply()
        {
            if (hasApplied)
            {
                VAPILog.Warning($"{this} has already been applied!");
                return;
            }

            hasApplied = true;
            if (!characterBody || variantsInBody == null)
            {
                Destroy(this);
                return;
            }


            var announcedArrival = false;
            for (int i = 0; i < variantsInBody.Count; i++)
            {
                VariantDef current = variantsInBody[i];
#if DEBUG
                VAPILog.Debug($"Applying {current} to {characterBody}");
#endif
                try
                {
                    VariantTierDef tier = current.variantTierDef;
                    if (!announcedArrival && VAPIConfig._sendArrivalMesssages)
                        announcedArrival = AnnounceArrival(current, tier);

                    VariantInventory inventory = current.variantInventory;

                    if (inventory)
                    {
                        inventory.AddBuffs(characterBody);
                    }
                    tier.AddTierBuff(characterBody);

                    if (characterMaster)
                    {
                        if (inventory)
                        {
                            inventory.AddItems(characterMaster.inventory);
                            inventory.SetEquipment(characterMaster.inventory, characterBody);
                        }
                        tier.AddTierItems(characterMaster.inventory);
                    }

                    if (characterDeathBehavior && current.deathStateOverride.stateType != null)
                        characterDeathBehavior.deathState = current.deathStateOverride;

                    ModifySkills(current.skillReplacements);
                    ModifyStats(current);

                    VariantVisuals visuals = current.visualModifier;
                    if (visuals)
                    {
                        visualsForCoroutine.Add(visuals);
                    }

                    VariantSizeModifier sizeModifier = current.sizeModifier;
                    if (sizeModifier && characterModel)
                    {
                        sizeModifier.ApplySize(characterModel.transform, characterBody.GetComponentsInChildren<KinematicCharacterMotor>());
                    }

                    ModifyAI(current.aiModifier, current.baseAIDampBonus, current.baseAIDampMultiplier);

                    ModifyName(current.nameOverrides);

                    AddComponents(current.componentProviders);
                }
                catch (Exception e)
                {
                    VAPILog.Error($"Exception while trying to apply variant defs to {characterBody.GetDisplayName()}, {e}");
                }
            }

            if (NetworkServer.active)
            {
                characterBody.healthComponent.health = characterBody.healthComponent.fullHealth;
                characterBody.healthComponent.shield = characterBody.healthComponent.fullShield;
            }

            characterBody.RecalculateStats();
            var healthComponent = characterBody.healthComponent;
            if(healthComponent)
            {
                healthComponent.health = healthComponent.fullHealth;
                healthComponent.shield = healthComponent.fullShield;
            }

            if (visualsForCoroutine.Count > 0)
            {
                StartCoroutine(nameof(ApplyVisuals));
            }
        }

        private IEnumerator ApplyVisuals()
        {
            yield return new WaitForEndOfFrame();
            while (!modelSkinControllerFinished)
                yield return new WaitForEndOfFrame();

            foreach (VariantVisuals visuals in visualsForCoroutine)
            {
                visuals.ApplyMaterials(characterModel);
                visuals.ApplyLights(characterModel);

                if (VAPIConfig._activateMeshReplacementSystem)
                {
                    if (visuals.ApplyMeshes(characterModel, out storedIDRS, out MeshType meshType))
                    {
                        if (meshType != MeshType.Default)
                            TryFuckWithBoneStructure(meshType);
                    }
                }
            }
            characterModel.materialsDirty = true;
        }
        private bool AnnounceArrival(VariantDef variantDef, VariantTierDef tierDef)
        {
            bool announced = false;
            if (tierDef.announcesArrival)
            {
                if (!string.IsNullOrEmpty(variantDef.arrivalToken))
                {
                    Chat.AddMessage(Language.GetStringFormatted(variantDef.arrivalToken));
                }
                else
                {
#if DEBUG
                    VAPILog.Warning($"{variantDef}'s tier announces its arrival, but it doesnt have a token set, using generic message.");
#endif
                    Chat.AddMessage(Language.GetStringFormatted("VAPI_GENERIC_ARRIVAL", characterBody.GetDisplayName()));
                }
                announced = true;
            }

            if (tierDef.soundEvent)
            {
                EffectManager.SimpleSoundEffect(tierDef.soundEvent.index, transform.position, true);
                announced = true;
            }
            return announced;
        }

        private void ModifySkills(VariantDef.VariantSkillReplacement[] skillReplacements)
        {
            SkillLocator skillLocator = characterBody.skillLocator;
            if (skillLocator)
            {
                foreach (VariantDef.VariantSkillReplacement skillReplacement in skillReplacements)
                {
                    var skillDefToSet = skillReplacement.skillDef ? skillReplacement.skillDef : VAPIAssets._emptySkillDef;
                    switch (skillReplacement.skillSlot)
                    {
                        case SkillSlot.Primary:
                            skillLocator.primary?.SetSkillOverride(gameObject, skillDefToSet, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.Secondary:
                            skillLocator.secondary?.SetSkillOverride(gameObject, skillDefToSet, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.Utility:
                            skillLocator.utility?.SetSkillOverride(gameObject, skillDefToSet, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.Special:
                            skillLocator.special?.SetSkillOverride(gameObject, skillDefToSet, GenericSkill.SkillOverridePriority.Upgrade);
                            break;
                        case SkillSlot.None:
                            break;
                    }
                }
            }
        }

        private void ModifyStats(VariantDef variantDef)
        {
            characterBody.baseMaxHealth *= variantDef.healthMultiplier;
            characterBody.baseMoveSpeed *= variantDef.moveSpeedMultiplier;
            characterBody.baseAttackSpeed *= variantDef.attackSpeedMultiplier;
            characterBody.baseDamage *= variantDef.damageMultiplier;
            characterBody.levelDamage = characterBody.baseDamage * 0.2f;
            characterBody.baseArmor += variantDef.armorBonus + (variantDef.variantTierDef ? variantDef.variantTierDef.armorBonus : 0);
            characterBody.baseArmor *= variantDef.armorMultiplier;
            characterBody.baseRegen += variantDef.regenBonus;
            characterBody.baseRegen *= variantDef.regenMultiplier;
            characterBody.baseMaxShield += variantDef.shieldBonus;
            characterBody.baseMaxShield *= variantDef.shieldMultiplier;
        }

        private void ModifyAI(BasicAIModifier aiModifier, float baseAIDampBonus, float baseAIDampModifier)
        {
            if (!characterMaster)
                return;

            foreach(var baseAI in characterMaster.GetComponents<BaseAI>())
            {
                baseAI.aimVectorDampTime += baseAIDampBonus;
                baseAI.aimVectorMaxSpeed *= baseAIDampModifier;
            }

            foreach (AISkillDriver driver in characterMaster.GetComponents<AISkillDriver>())
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
                    if (aiModifier.HasFlag(BasicAIModifier.ForceSprint))
                    {
                        driver.shouldSprint = true;
                    }
                }
            }
        }

        private void ModifyName(VariantDef.VariantOverrideName[] overrideNames)
        {
            foreach (var overrideName in overrideNames)
            {
                switch (overrideName.overrideType)
                {
                    case OverrideNameType.Prefix:
                        characterBody.baseNameToken = Language.GetStringFormatted(overrideName.token) + " " + characterBody.GetDisplayName();
                        break;
                    case OverrideNameType.Suffix:
                        characterBody.baseNameToken = characterBody.GetDisplayName() + " " + Language.GetStringFormatted(overrideName.token);
                        break;
                    case OverrideNameType.Complete:
                        characterBody.baseNameToken = Language.GetStringFormatted(overrideName.token);
                        break;
                }
            }
        }

        private void AddComponents(VariantDef.VariantComponentProvider[] providers)
        {
            foreach (var component in providers)
            {
                Type typeToAdd = (Type)component.componentToAdd;

                switch (component.attachmentType)
                {
                    case ComponentAttachmentType.Body:
                        SetupComponent(characterBody.gameObject.AddComponent(typeToAdd));
                        break;
                    case ComponentAttachmentType.Master:
                        if (characterMaster)
                            SetupComponent(characterMaster.gameObject.AddComponent(typeToAdd));
                        break;
                    case ComponentAttachmentType.Model:
                        if (characterModel)
                            SetupComponent(characterModel.gameObject.AddComponent(typeToAdd));
                        break;
                }
            }
        }

        private void SetupComponent(Component component)
        {
            if (!(component is VariantComponent vc))
            {
                return;
            }
            vc.characterBody = characterBody;
            vc.characterMaster = characterMaster;
            vc.characterModel = characterModel;
            vc.variantDefs = variantsInBody;
        }
        #region Mesh Replacement Jank
        private void TryFuckWithBoneStructure(MeshType meshType)
        {
            if (characterMaster && characterMaster.inventory)
            {
                storedEquip = characterMaster.inventory.GetEquipmentIndex();
                characterMaster.inventory.SetEquipmentIndex(EquipmentIndex.None);
                Invoke(nameof(RestoreEquipment), 0.2f);
            }

            switch (meshType)
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
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
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
            foreach (var item in characterBody.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.bones = transforms.ToArray();
            }
        }

        private void BeetleGuardMeshReplacement()
        {
            List<Transform> t = new List<Transform>();
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
            {
                if (!item.name.Contains("Hurtbox") && !item.name.Contains("IK") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in characterBody.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.bones = t.ToArray();
            }
        }

        private void MiniMushrumMeshReplacement()
        {
            List<Transform> t = new List<Transform>();
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
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
            foreach (var item in characterBody.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.bones = t.ToArray();
            }
        }

        private void MagmaWormMeshReplacement()
        {
            List<Transform> t = new List<Transform>();
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Head") && !item.name.Contains("_end") && !item.name.Contains("Center"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Jaw") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("eye.") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Neck") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in characterBody.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.bones = t.ToArray();
            }
        }

        private void OverloadingWormMeshReplacement()
        {
            List<Transform> t = new List<Transform>();
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Head") && !item.name.Contains("_end") && !item.name.Contains("Center"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Jaw") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("eye.") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in characterBody.GetComponentsInChildren<Transform>())
            {
                if (item.name.Contains("Neck") && !item.name.Contains("_end"))
                {
                    t.Add(item);
                }
            }
            foreach (var item in characterBody.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                item.bones = t.ToArray();
            }
        }

        private void RestoreEquipment()
        {
            characterModel.itemDisplayRuleSet = storedIDRS;

            characterMaster.inventory.SetEquipmentIndex(storedEquip);
        }
        #endregion
    }
}
