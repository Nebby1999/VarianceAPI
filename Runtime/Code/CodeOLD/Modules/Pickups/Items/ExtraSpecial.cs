using Moonstorm;
using RoR2;

namespace VarianceAPI.Items
{
    public class ExtraSpecial : ItemBase
    {
        public override ItemDef ItemDef { get; } = Assets.VAPIAssets.LoadAsset<ItemDef>("ExtraSpecial");

        /*public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<ExtraSpecialBehavior>(stack);
        }

        public class ExtraSpecialBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalculateStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if (skillLocator)
                {
                    if ((bool)skillLocator.special)
                    {
                        skillLocator.special.SetBonusStockFromBody(stack);
                    }
                }
            }

            public void RecalculateStatsStart()
            {
            }
        }*/
    }
}
