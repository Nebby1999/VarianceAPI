using Moonstorm;
using RoR2;

namespace VAPI.Items
{
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
            var skillLoc = self.skillLocator;
            if (skillLoc.utility)
            {
                skillLoc.utility.SetBonusStockFromBody(self.GetItemCount(ItemDef));
            }
        }
    }
}
