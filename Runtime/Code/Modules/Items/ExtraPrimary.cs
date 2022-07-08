using Moonstorm;
using RoR2;
using R2API;

namespace VAPI.Items
{
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
            var skillLoc = self.skillLocator;
            if(skillLoc.primary)
            {
                skillLoc.primary.SetBonusStockFromBody(self.GetItemCount(ItemDef));
            }
        }
    }
}
