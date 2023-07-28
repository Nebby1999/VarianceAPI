using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A ScriptableObject used to represent a VariantPack
    /// </summary>
    [CreateAssetMenu(fileName = "New VariantPackDef", menuName = "VarianceAPI/VariantPackDef")]
    public class VariantPackDef : ScriptableObject
    {
        /// <summary>
        /// The ConfigFile used for this VariantPack's TierDefs
        /// </summary>
        public ConfigFile TierConfiguration { get; internal set; }
        /// <summary>
        /// The ConfigFile used for this VariantPack's VariantDefs
        /// </summary>
        public ConfigFile VariantConfiguration { get; internal set; }
        /// <summary>
        /// The BepInPlugin that registered this VariantPackDef
        /// </summary>
        public BepInPlugin BepInPlugin { get; internal set; }
        /// <summary>
        /// The VariantPack's index, do not set this value manually
        /// </summary>
        public VariantPackIndex VariantPackIndex { get; internal set; }
        /// <summary>
        /// The choice that represents if this VariantPack is enabled
        /// </summary>
        public RuleChoiceDef EnabledChoice { get; internal set; }

        [Tooltip("The VariantPack's name")]
        public string nameToken;
        [Tooltip("The VariantPack's Tooltip")]
        public string tooltipToken;
        [Tooltip("A description of the VariantPack")]
        public string descriptionToken;
        [Tooltip("An icon that's displayed when this VariantPack is enabled")]
        public Sprite packEnabledIcon;
        [Tooltip("The VariantPack's VariantTierDefs")]
        public VariantTierDef[] variantTiers = Array.Empty<VariantTierDef>();
        [Tooltip("The VariantPack's VariantDefs")]
        public VariantDef[] variants = Array.Empty<VariantDef>();

        private void OnValidate()
        {
            if (!packEnabledIcon && variants.Length > 0)
            {
                Debug.LogError($"VariantPackDef {name} does not have a pack icon, and it has VariantDefs in its variants array, this WILL cause a crash. please supply a pack icon.", this);
            }
        }
    }
}