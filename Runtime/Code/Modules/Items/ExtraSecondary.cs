using Moonstorm;
using RoR2;

namespace VAPI.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class ExtraSecondary : ItemBase
    {
        public override ItemDef ItemDef { get; } = VAPIAssets.LoadAsset<ItemDef>("ExtraSecondary");

        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.CharacterBody.RecalculateStats += AddSecondary;
        }

        private void AddSecondary(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            var skillLoc = self.skillLocator;
            if (skillLoc.secondary)
            {
                skillLoc.secondary.SetBonusStockFromBody(self.GetItemCount(ItemDef));
            }
        }
    }
}
