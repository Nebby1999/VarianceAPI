using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;

namespace VAPI.Legacy.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class Plus1Crit : VAPIItem
    {
        public override VAPIAssetRequest<ItemDef> GetAssetRequest() => VAPIAssets.LoadAssetAsync<ItemDef>("Plus1Crit");

        public override void Initialize()
        {
            base.Initialize();
            RecalculateStatsAPI.GetStatCoefficients += Add1Crit;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void Add1Crit(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.critAdd += sender.GetItemCount(itemDef);
        }
    }
}