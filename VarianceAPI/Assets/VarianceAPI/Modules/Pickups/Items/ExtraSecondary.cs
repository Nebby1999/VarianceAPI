using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VarianceAPI.Modules;
using System.Threading.Tasks;

namespace VarianceAPI.Items
{
    public class ExtraSecondary : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.VAPIAssets.LoadAsset<ItemDef>("ExtraSecondary");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<ExtraSecondaryBehavior>(stack);
        }

        public class ExtraSecondaryBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalcStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if(skillLocator)
                {
                    if((bool)skillLocator.secondary)
                    {
                        skillLocator.secondary.SetBonusStockFromBody(stack);
                    }
                }
            }

            public void RecalcStatsStart()
            {
            }
        }
    }
}
