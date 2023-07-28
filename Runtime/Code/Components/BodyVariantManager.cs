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
        public CharacterBody CharacterBody { get; private set; }
        /// <summary>
        /// The Variant's CharacterMaster
        /// </summary>
        public CharacterMaster CharacterMaster { get => CharacterBody.master; }
        /// <summary>
        /// The Variant's CharacterDeathBehaviour
        /// </summary>
        public CharacterDeathBehavior CharacterDeathBehavior { get; private set; }
        /// <summary>
        /// The Variant's CharacterModel
        /// </summary>
        public CharacterModel CharacterModel { get; private set; }

        /// <summary>
        /// Wether the VariantDefs get applied on Start
        /// </summary>
        public bool applyOnStart = true;

        private readonly SyncListInt variantIndices = new SyncListInt();
        private readonly List<VariantVisuals> visualsForCoroutine = new List<VariantVisuals>();
        private bool hasApplied = false;
        private EquipmentIndex storedEquip;
        private ItemDisplayRuleSet storedIDRS;

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
                variantIndices.Add((int)vd.VariantIndex);
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

            CharacterBody = GetComponent<CharacterBody>();
            CharacterDeathBehavior = GetComponent<CharacterDeathBehavior>();

            if (CharacterBody.modelLocator && CharacterBody.modelLocator.modelTransform)
                CharacterModel = CharacterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
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
            if (!CharacterBody || variantsInBody == null)
            {
                Destroy(this);
                return;
            }


            var announcedArrival = false;
            for (int i = 0; i < variantsInBody.Count; i++)
            {
                VariantDef current = variantsInBody[i];
#if DEBUG
                VAPILog.Debug($"Applying {current} to {CharacterBody}");
#endif
                try
                {
                    VariantTierDef tier = current.VariantTierDef;
                    if (!announcedArrival && VAPIConfig.sendArrivalMesssages)
                        announcedArrival = AnnounceArrival(current, tier);

                    VariantInventory inventory = current.variantInventory;

                    if (inventory)
                    {
                        inventory.AddBuffs(CharacterBody);
                    }
                    tier.AddTierBuff(CharacterBody);

                    if (CharacterMaster)
                    {
                        if (inventory)
                        {
                            inventory.AddItems(CharacterMaster.inventory);
                            inventory.SetEquipment(CharacterMaster.inventory, CharacterBody);
                        }
                        tier.AddTierItems(CharacterMaster.inventory);
                    }

                    if (CharacterDeathBehavior && current.deathStateOverride.stateType != null)
                        CharacterDeathBehavior.deathState = current.deathStateOverride;

                    ModifySkills(current.skillReplacements);
                    ModifyStats(current);

                    VariantVisuals visuals = current.visualModifier;
                    if (visuals)
                    {
                        visualsForCoroutine.Add(visuals);
                    }

                    VariantSizeModifier sizeModifier = current.sizeModifier;
                    if (sizeModifier && CharacterModel)
                    {
                        sizeModifier.ApplySize(CharacterModel.transform, CharacterBody.GetComponentsInChildren<KinematicCharacterMotor>());
                    }

                    ModifyAI(current.aiModifier);

                    ModifyName(current.nameOverrides);

                    AddComponents(current.componentProviders);
                }
                catch (Exception e)
                {
                    VAPILog.Error($"Exception while trying to apply variant defs to {CharacterBody.GetDisplayName()}, {e}");
                }
            }

            if (NetworkServer.active)
            {
                CharacterBody.healthComponent.health = CharacterBody.healthComponent.fullHealth;
                CharacterBody.healthComponent.shield = CharacterBody.healthComponent.fullShield;
            }

            CharacterBody.RecalculateStats();
            var healthComponent = CharacterBody.healthComponent;
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
            foreach (VariantVisuals visuals in visualsForCoroutine)
            {
                visuals.ApplyMaterials(CharacterModel);
                visuals.ApplyLights(CharacterModel);

                if (VAPIConfig.activateMeshReplacementSystem)
                {
                    if (visuals.ApplyMeshes(CharacterModel, out storedIDRS, out MeshType meshType))
                    {
                        if (meshType != MeshType.Default)
                            TryFuckWithBoneStructure(meshType);
                    }
                }
            }
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
                    Chat.AddMessage(Language.GetStringFormatted("VAPI_GENERIC_ARRIVAL", CharacterBody.GetDisplayName()));
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
            SkillLocator skillLocator = CharacterBody.skillLocator;
            if (skillLocator)
            {
                foreach (VariantDef.VariantSkillReplacement skillReplacement in skillReplacements)
                {
                    var skillDefToSet = skillReplacement.skillDef ? skillReplacement.skillDef : VAPIAssets.Instance.emptySkillDef;
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
            CharacterBody.baseMaxHealth *= variantDef.healthMultiplier;
            CharacterBody.baseMoveSpeed *= variantDef.moveSpeedMultiplier;
            CharacterBody.baseAttackSpeed *= variantDef.attackSpeedMultiplier;
            CharacterBody.baseDamage *= variantDef.damageMultiplier;
            CharacterBody.levelDamage = CharacterBody.baseDamage * 0.2f;
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

            foreach (AISkillDriver driver in CharacterMaster.GetComponents<AISkillDriver>())
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
            foreach (var component in providers)
            {
                Type typeToAdd = (Type)component.componentToAdd;

                switch (component.attachmentType)
                {
                    case ComponentAttachmentType.Body:
                        SetupComponent(CharacterBody.gameObject.AddComponent(typeToAdd));
                        break;
                    case ComponentAttachmentType.Master:
                        if (CharacterMaster)
                            SetupComponent(CharacterMaster.gameObject.AddComponent(typeToAdd));
                        break;
                    case ComponentAttachmentType.Model:
                        if (CharacterModel)
                            SetupComponent(CharacterModel.gameObject.AddComponent(typeToAdd));
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
            vc.CharacterBody = CharacterBody;
            vc.CharacterMaster = CharacterMaster;
            vc.CharacterModel = CharacterModel;
            vc.VariantDefs = variantsInBody;
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
