﻿using Moonstorm;
using RoR2;

namespace VAPI.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class ExtraPrimary : ItemBase
    {
        public override ItemDef ItemDef { get; } = VAPIAssets.LoadAsset<ItemDef>("ExtraPrimary");

        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.CharacterBody.RecalculateStats += AddPrimary;
        }

        private void AddPrimary(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            var skillLoc = self.skillLocator;
            if (skillLoc.primary)
            {
                skillLoc.primary.SetBonusStockFromBody(skillLoc.primary.bonusStockFromBody + self.GetItemCount(ItemDef));
            }
        }
    }
}
