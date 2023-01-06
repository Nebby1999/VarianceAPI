using Moonstorm;
using R2API;
using RoR2;

namespace VAPI.Items
{
    /// <summary>
    /// An intrinsic variant item
    /// </summary>
    public class GlobalCDR : ItemBase
    {
        public override ItemDef ItemDef { get; } = VAPIAssets.LoadAsset<ItemDef>("GlobalCDR");

        public override void Initialize()
        {
            base.Initialize();
            RecalculateStatsAPI.GetStatCoefficients += ReduceCooldowns;
        }

        private void ReduceCooldowns(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.cooldownMultAdd -= sender.GetItemCount(ItemDef) * 0.01f;
        }
    }
}