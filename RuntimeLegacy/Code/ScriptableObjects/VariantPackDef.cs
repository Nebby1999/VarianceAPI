using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using UnityEngine;

namespace VAPI.Legacy
{
    [Obsolete("VarianceAPI's 2.0 systems are obsolete, VariantPacks are now raw C# files that you give to the VariantPackManager, they're similar to ContentPacks in that sense.")]
    public class VariantPackDef : ScriptableObject
    {
        public string nameToken;
        public string tooltipToken;
        public string descriptionToken;
        public Sprite packEnabledIcon;
        public VariantTierDef[] variantTiers = Array.Empty<VariantTierDef>();
        public VariantDef[] variants = Array.Empty<VariantDef>();
    }
}