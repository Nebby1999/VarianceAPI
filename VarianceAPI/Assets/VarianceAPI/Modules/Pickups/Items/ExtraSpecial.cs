using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VarianceAPI.Modules;
using System.Threading.Tasks;

namespace VarianceAPI.Items
{
    public class ExtraSpecial : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.VAPIAssets.LoadAsset<ItemDef>("ExtraSpecial");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<ExtraSpecialBehavior>(stack);
        }

        public class ExtraSpecialBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalcStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if(skillLocator)
                {
                    if((bool)skillLocator.special)
                    {
                        skillLocator.special.SetBonusStockFromBody(stack);
                    }
                }
            }

            public void RecalcStatsStart()
            {
            }
        }
    }
}
