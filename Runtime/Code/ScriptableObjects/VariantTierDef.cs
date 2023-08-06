using Moonstorm.AddressableAssets;
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// A ScriptableObject used to represent a variant's Tier
    /// </summary>
    [CreateAssetMenu(fileName = "New VariantTierDef", menuName = "VarianceAPI/VariantTierDef")]
    public class VariantTierDef : ScriptableObject
    {
        [Tooltip("The VariantTier's internal index, set this to AssignedAtRuntime for custom tiers")]
        [SerializeField]
        private VariantTierIndex _tier;
        [Tooltip("Wether this variant will announce its arrival in chat")]
        public bool announcesArrival;
        [Tooltip("The sound event to play when a variant of this tier spawns.")]
        public NetworkSoundEventDef soundEvent;

        [Tooltip("A list of items that are added to variants of this tier when they spawn")]
        public List<AddressReferencedItemDef> tierItemDefs = new List<AddressReferencedItemDef>();
        [HideInInspector, Obsolete("Use \"tierItemDefs\" instead")]
        public List<AddressableItemDef> tierItems = new List<AddressableItemDef>();

        [Tooltip("A non timedd buff that's applied to variants of this tier when they spawn")]
        public AddressReferencedBuffDef tierBuffDef;
        [HideInInspector, Obsolete("Use \"tierBuffDef\" instead")]
        public AddressableBuffDef tierBuff;

        [Space]
        [Tooltip("The experience muttliplier for Variants of this tier")]
        public float experienceMultiplier;
        [Tooltip("The gold multiplier for Variants of this tier")]
        public float goldMultiplier;
        [Tooltip("The chances for Variants of this tier to drop White items")]
        public float whiteItemDropChance;
        [Tooltip("The chances for Variants of this tier to drop Green items")]
        public float greenItemDropChance;
        [Tooltip("The chances for Variants of this tier to drop Red items")]
        public float redItemDropChance;

        /// <summary>
        /// Returns <see cref="goldMultiplier"/> minus 1 if <see cref="goldMultiplier"/> is greater than 1
        /// </summary>
        public float ExperienceMultiplierMinus1
        {
            get
            {
                if (experienceMultiplier < 1)
                    return experienceMultiplier;
                return experienceMultiplier - 1;
            }
        }

        /// <summary>
        /// Returns <see cref="goldMultiplier"/> minus 1 if <see cref="goldMultiplier"/> is greater than 1
        /// </summary>
        public float GoldMultiplierMinus1
        {
            get
            {
                if (goldMultiplier < 1)
                    return goldMultiplier;
                return goldMultiplier - 1;
            }
        }

        /// <summary>
        /// The internal tier for this variant
        /// </summary>
        public VariantTierIndex Tier { get => _tier; internal set => _tier = value; }

        /// <summary>
        /// Awake method for VariantTierDef
        /// <para>Ensures the now deprecated <see cref="tierBuff"/>, <see cref="tierItems"/> are migrated to their new fields</para>
        /// </summary>
        public virtual void Awake()
        {
#if !UNITY_EDITOR
            Migrate();
#endif
        }

        [ContextMenu("Migrate to R2API.Addressables")]
        private void Migrate()
        {
            if (tierBuffDef.IsInvalid)
                tierBuffDef = tierBuff;

            if(tierItemDefs.Count == 0 && tierItems.Count > 0)
            {
                tierItemDefs.AddRange(tierItems.Select(x => (AddressReferencedItemDef)x));
            }
        }
        /// <summary>
        /// Adds the items specified in <see cref="tierItems"/> to the target inventory
        /// </summary>
        /// <param name="targetInventory">The inventory that will recieve this tier's items</param>
        public virtual void AddTierItems(Inventory targetInventory)
        {
            if (!NetworkServer.active)
                return;

            foreach (AddressReferencedItemDef itemDef in tierItemDefs)
            {
                if (itemDef)
                    targetInventory.GiveItem(itemDef);
            }
        }

        /// <summary>
        /// Adds the buff specified in <see cref="tierBuff"/> to the target body
        /// </summary>
        /// <param name="targetBody">The body that will recieve this tier's buffs</param>
        public virtual void AddTierBuff(CharacterBody targetBody)
        {
            if (!NetworkServer.active || !tierBuffDef)
                return;

            targetBody.AddBuff(tierBuffDef);
        }

        /// <summary>
        /// Creates the <see cref="VariantRewardInfo"/> that will be used for Variants that use this tier
        /// </summary>
        /// <param name="runInstance">The current run</param>
        /// <returns>The VariantRewardInfo to use</returns>
        public virtual VariantRewardInfo CreateVariantRewardInfo(Run runInstance)
        {
            var vri = new VariantRewardInfo(goldMultiplier, experienceMultiplier, whiteItemDropChance, greenItemDropChance, redItemDropChance);
            vri.SetIndicesAndNextItems(runInstance);
            return vri;
        }
    }
}
