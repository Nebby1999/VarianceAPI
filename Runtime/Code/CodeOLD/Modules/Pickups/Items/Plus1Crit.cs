using Moonstorm;
using R2API;
using RoR2;

namespace VarianceAPI.Items
{
    public class Plus1Crit : ItemBase
    {
        public override ItemDef ItemDef { get; } = Assets.VAPIAssets.LoadAsset<ItemDef>("Plus1Crit");

        /*public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<Plus1CritBehavior>(stack);
        }

        public class Plus1CritBehavior : CharacterBody.ItemBehavior, IBodyStatArgModifier
        {
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.critAdd += stack;
            }
        }*/
    }
}