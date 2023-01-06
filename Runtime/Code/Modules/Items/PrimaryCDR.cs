using Moonstorm;
using R2API;
using RoR2;

namespace VAPI.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
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