using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;

namespace VAPI.Legacy.Items
{
    /// <summary>
    /// An intrinsic variant item
    /// </summary>
    public class GlobalCDR : VAPIItem
    {
        public override VAPIAssetRequest<ItemDef> GetAssetRequest() => VAPIAssets.LoadAssetAsync<ItemDef>("GlobalCDR");

        public override void Initialize()
        {
            base.Initialize();
            RecalculateStatsAPI.GetStatCoefficients += ReduceCooldowns;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void ReduceCooldowns(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.cooldownMultAdd -= sender.GetItemCount(itemDef) * 0.01f;
        }
    }
}