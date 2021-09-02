using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VarianceAPI.Modules;
using System.Threading.Tasks;

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
            public void RecalcStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if(skillLocator)
                {
                    if((bool)skillLocator.primary)
                    {
                        skillLocator.primary.SetBonusStockFromBody(stack);
                    }
                }
            }

            public void RecalcStatsStart()
            {
            }
        }
    }
}
