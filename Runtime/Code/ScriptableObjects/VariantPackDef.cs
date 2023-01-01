using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantPackDef", menuName = "VarianceAPI/VariantPackDef")]
    public class VariantPackDef : ScriptableObject
    {
        public ConfigFile TierConfiguration { get; internal set; }
        public ConfigFile VariantConfiguration { get; internal set; }
        public VariantPackIndex VariantPackIndex { get; internal set; }
        public RuleChoiceDef EnabledChoice { get; internal set; }

        public string nameToken;
        public string tooltipToken;
        public string descriptionToken;
        public Sprite packEnabledIcon;
        public Sprite packDisabledIcon;
        public VariantTierDef[] variantTiers = Array.Empty<VariantTierDef>();
        public VariantDef[] variants = Array.Empty<VariantDef>();

        private void OnValidate()
        {
            if(!packEnabledIcon && variants.Length > 0)
            {
                Debug.LogError($"VariantPackDef {name} does not have a pack icon, and it has VariantDefs in its variants array, this WILL cause a crash. please supply a pack icon.", this);
            }
        }
    }
}