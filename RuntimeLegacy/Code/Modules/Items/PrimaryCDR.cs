using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;

namespace VAPI.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class PrimaryCDR : VAPIItem
    {
        public override VAPIAssetRequest<ItemDef> GetAssetRequest() => VAPIAssets.LoadAssetAsync<ItemDef>("PrimaryCDR");

        public override void Initialize()
        {
            base.Initialize();
            RecalculateStatsAPI.GetStatCoefficients += ReduceCooldown;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void ReduceCooldown(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.primaryCooldownMultAdd -= sender.GetItemCount(itemDef) * 0.01f;
        }
    }
}