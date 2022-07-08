using Moonstorm;
using RoR2;
using R2API;

namespace VAPI.Items
{
    public class PrimaryCDR : ItemBase
    {
        public override ItemDef ItemDef { get; } = VAPIAssets.LoadAsset<ItemDef>("PrimaryCDR");

        public override void Initialize()
        {
            base.Initialize();
            RecalculateStatsAPI.GetStatCoefficients += ReduceCooldown;
        }

        private void ReduceCooldown(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.primaryCooldownMultAdd -= sender.GetItemCount(ItemDef) * 0.01f;
        }
    }
}