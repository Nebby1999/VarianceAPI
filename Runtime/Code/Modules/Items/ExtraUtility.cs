using Moonstorm;
using RoR2;

namespace VAPI.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class ExtraUtility : ItemBase
    {
        public override ItemDef ItemDef { get; } = VAPIAssets.LoadAsset<ItemDef>("ExtraUtility");
        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.CharacterBody.RecalculateStats += AddSpecial;
        }

        private void AddSpecial(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            var skillLoc = self.skillLocator;
            if (skillLoc.utility)
            {
                skillLoc.utility.SetBonusStockFromBody(skillLoc.utility.bonusStockFromBody + self.GetItemCount(ItemDef));
            }
        }
    }
}
