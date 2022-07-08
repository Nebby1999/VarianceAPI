using Moonstorm;
using R2API;
using RoR2;

namespace VAPI.Items
{
    public class SecondaryCDR : ItemBase
    {
        public override ItemDef ItemDef { get; } = VAPIAssets.LoadAsset<ItemDef>("SecondaryCDR");

        public override void Initialize()
        {
            base.Initialize();
            RecalculateStatsAPI.GetStatCoefficients += ReduceCooldown;
        }

        private void ReduceCooldown(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.secondaryCooldownMultAdd -= sender.GetItemCount(ItemDef) * 0.01f;
        }
    }
}