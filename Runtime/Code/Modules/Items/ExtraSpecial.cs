using Moonstorm;
using RoR2;

namespace VAPI.Items
{
    public class ExtraSpecial : ItemBase
    {
        public override ItemDef ItemDef { get; } = VAPIAssets.LoadAsset<ItemDef>("ExtraSpecial");

        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.CharacterBody.RecalculateStats += AddSpecial;
        }

        private void AddSpecial(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            var skillLoc = self.skillLocator;
            if (skillLoc.special)
            {
                skillLoc.special.SetBonusStockFromBody(self.GetItemCount(ItemDef));
            }
        }
    }
}
