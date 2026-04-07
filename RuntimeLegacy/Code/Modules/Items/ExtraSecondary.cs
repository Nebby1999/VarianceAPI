using MSU;
using RoR2;
using RoR2.ContentManagement;

namespace VAPI.Legacy.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class ExtraSecondary : VAPIItem
    {
        public override VAPIAssetRequest<ItemDef> GetAssetRequest() => VAPIAssets.LoadAssetAsync<ItemDef>("ExtraSecondary");
        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.CharacterBody.RecalculateStats += AddSecondary;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void AddSecondary(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            var skillLoc = self.skillLocator;
            if (skillLoc.secondary)
            {
                skillLoc.secondary.SetBonusStockFromBody(skillLoc.secondary.bonusStockFromBody + self.GetItemCount(itemDef));
            }
        }
    }
}
