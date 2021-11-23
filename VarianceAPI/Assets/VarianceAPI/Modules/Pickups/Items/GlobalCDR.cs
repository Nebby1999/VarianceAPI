using Moonstorm;
using RoR2;

namespace VarianceAPI.Items
{
    public class GlobalCDR : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.VAPIAssets.LoadAsset<ItemDef>("GlobalCDR");

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
                    if ((bool)skillLocator.secondary)
                    {
                        skillLocator.secondary.cooldownScale -= stack / 100f;
                    }
                    if ((bool)skillLocator.utility)
                    {
                        skillLocator.utility.cooldownScale -= stack / 100f;
                    }
                    if ((bool)skillLocator.special)
                    {
                        skillLocator.special.cooldownScale -= stack / 100f;
                    }
                }
            }

            public void RecalculateStatsStart()
            {
            }
        }
    }
}