using MSU;
using RoR2;
using RoR2.ContentManagement;

namespace VAPI.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class ExtraSpecial : VAPIItem
    {
        public override VAPIAssetRequest<ItemDef> GetAssetRequest() => VAPIAssets.LoadAssetAsync<ItemDef>("ExtraSpecial");
        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.CharacterBody.RecalculateStats += AddSpecial;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void AddSpecial(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            var skillLoc = self.skillLocator;
            if (skillLoc.special)
            {
                skillLoc.special.SetBonusStockFromBody(skillLoc.special.bonusStockFromBody + self.GetItemCount(itemDef));
            }
        }
    }
}
