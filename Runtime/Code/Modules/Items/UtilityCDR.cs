using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;

namespace VAPI.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class UtilityCDR : VAPIItem
    {
        public override VAPIAssetRequest<ItemDef> GetAssetRequest() => VAPIAssets.LoadAssetAsync<ItemDef>("UtilityCDR");

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
            args.utilityCooldownMultAdd -= sender.GetItemCount(itemDef) * 0.01f;
        }
    }
}