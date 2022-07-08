using Moonstorm;
using RoR2;
using R2API;

namespace VAPI.Items
{
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