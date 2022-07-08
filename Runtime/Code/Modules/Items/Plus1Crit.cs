using Moonstorm;
using R2API;
using RoR2;

namespace VAPI.Items
{
    public class Plus1Crit : ItemBase
    {
        public override ItemDef ItemDef { get; } = VAPIAssets.LoadAsset<ItemDef>("Plus1Crit");

        public override void Initialize()
        {
            base.Initialize();
            RecalculateStatsAPI.GetStatCoefficients += Add1Crit;
        }

        private void Add1Crit(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.critAdd += sender.GetItemCount(ItemDef);
        }
    }
}