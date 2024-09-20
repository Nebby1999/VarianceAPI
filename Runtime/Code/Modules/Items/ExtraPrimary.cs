using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;

namespace VAPI.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class ExtraPrimary : VAPIItem
    {
        public override VAPIAssetRequest<ItemDef> GetAssetRequest() => VAPIAssets.LoadAssetAsync<ItemDef>("ExtraPrimary");

        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.CharacterBody.RecalculateStats += AddPrimary;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void AddPrimary(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            var skillLoc = self.skillLocator;
            if (skillLoc.primary)
            {
                skillLoc.primary.SetBonusStockFromBody(skillLoc.primary.bonusStockFromBody + self.GetItemCount(itemDef));
            }
        }
    }
}
