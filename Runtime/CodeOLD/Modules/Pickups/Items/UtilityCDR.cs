using Moonstorm;
using RoR2;

namespace VarianceAPI.Items
{
    public class UtilityCDR : ItemBase
    {
        public override ItemDef ItemDef { get; } = Assets.VAPIAssets.LoadAsset<ItemDef>("UtilityCDR");

        /*public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<UtilityCDRBehavior>(stack);
        }

        public class UtilityCDRBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalculateStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if (skillLocator)
                {
                    if ((bool)skillLocator.utility)
                    {
                        skillLocator.utility.cooldownScale -= stack / 100f;
                    }
                }
            }

            public void RecalculateStatsStart()
            {
            }
        }*/
    }
}