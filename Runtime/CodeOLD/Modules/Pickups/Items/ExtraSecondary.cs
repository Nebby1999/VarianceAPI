using Moonstorm;
using RoR2;

namespace VarianceAPI.Items
{
    public class ExtraSecondary : ItemBase
    {
        public override ItemDef ItemDef { get; } = Assets.VAPIAssets.LoadAsset<ItemDef>("ExtraSecondary");

        /*public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<ExtraSecondaryBehavior>(stack);
        }

        public class ExtraSecondaryBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalculateStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if (skillLocator)
                {
                    if ((bool)skillLocator.secondary)
                    {
                        skillLocator.secondary.SetBonusStockFromBody(stack);
                    }
                }
            }

            public void RecalculateStatsStart()
            {
            }
        }*/
    }
}
