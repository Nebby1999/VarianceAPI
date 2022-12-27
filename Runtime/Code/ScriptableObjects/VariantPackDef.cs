using BepInEx.Configuration;
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
        public ConfigFile configurationFile { get; internal set; }
        public VariantPackIndex VariantPackIndex { get; internal set; }
        public string nameToken;
        public string tooltipToken;
        public string descriptionToken;
        public Sprite packEnabledIcon;
        public Sprite packDisabledIcon;
        public VariantTierDef[] variantTiers = Array.Empty<VariantTierDef>();
        public VariantDef[] variants = Array.Empty<VariantDef>();
    }
}