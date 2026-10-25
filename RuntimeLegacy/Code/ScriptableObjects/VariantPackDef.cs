using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using UnityEngine;

namespace VAPI.Legacy
{
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