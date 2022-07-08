using Moonstorm;
using RoR2;

namespace VarianceAPI.Items
{
    public class SpecialCDR : ItemBase
    {
        public override ItemDef ItemDef { get; } = Assets.VAPIAssets.LoadAsset<ItemDef>("SpecialCDR");

        /*public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<SpecialCDRBehavior>(stack);
        }

        public class SpecialCDRBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalculateStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if (skillLocator)
                {
                    if ((bool)skillLocator.special)
                    {
                        skillLocator.special.cooldownScale -= stack / 100f;
                    }
                }
            }

            public void RecalculateStatsStart()
            {
            }
        }*/
    }
}