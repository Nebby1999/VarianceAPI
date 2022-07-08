using Moonstorm;
using RoR2;

namespace VarianceAPI.Items
{
    public class ExtraUtility : ItemBase
    {
        public override ItemDef ItemDef { get; } = Assets.VAPIAssets.LoadAsset<ItemDef>("ExtraUtility");

        /*public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<ExtraUtilityBehavior>(stack);
        }

        public class ExtraUtilityBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalculateStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if (skillLocator)
                {
                    if ((bool)skillLocator.utility)
                    {
                        skillLocator.utility.SetBonusStockFromBody(stack);
                    }
                }
            }

            public void RecalculateStatsStart()
            {
            }
        }*/
    }
}
