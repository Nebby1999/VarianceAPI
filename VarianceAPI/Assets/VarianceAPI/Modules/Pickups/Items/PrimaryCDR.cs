using Moonstorm;
using RoR2;

namespace VarianceAPI.Items
{
    public class PrimaryCDR : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.VAPIAssets.LoadAsset<ItemDef>("PrimaryCDR");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<PrimaryCDRBehavior>(stack);
        }

        public class PrimaryCDRBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalculateStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if (skillLocator)
                {
                    if ((bool)skillLocator.primary)
                    {
                        skillLocator.primary.cooldownScale -= stack / 100f;
                    }
                }
            }

            public void RecalculateStatsStart()
            {
            }
        }
    }
}