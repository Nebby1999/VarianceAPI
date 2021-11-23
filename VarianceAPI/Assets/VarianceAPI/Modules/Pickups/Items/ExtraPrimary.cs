using Moonstorm;
using RoR2;

namespace VarianceAPI.Items
{
    public class ExtraPrimary : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.VAPIAssets.LoadAsset<ItemDef>("ExtraPrimary");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<ExtraPrimaryBehavior>(stack);
        }

        public class ExtraPrimaryBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {

            public void RecalculateStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if (skillLocator)
                {
                    if ((bool)skillLocator.primary)
                    {
                        skillLocator.primary.SetBonusStockFromBody(stack);
                    }
                }
            }

            public void RecalculateStatsStart()
            {
            }
        }
    }
}
