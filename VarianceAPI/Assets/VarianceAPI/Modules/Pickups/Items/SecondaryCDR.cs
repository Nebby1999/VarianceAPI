using Moonstorm;
using RoR2;

namespace VarianceAPI.Items
{
    public class SecondaryCDR : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.VAPIAssets.LoadAsset<ItemDef>("SecondaryCDR");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<SecondaryCDRBehavior>(stack);
        }

        public class SecondaryCDRBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalculateStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if (skillLocator)
                {
                    if ((bool)skillLocator.secondary)
                    {
                        skillLocator.secondary.cooldownScale -= stack / 100f;
                    }
                }
            }

            public void RecalculateStatsStart()
            {
            }
        }
    }
}