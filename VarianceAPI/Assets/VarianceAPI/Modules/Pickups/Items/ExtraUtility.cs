using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VarianceAPI.Modules;
using System.Threading.Tasks;

namespace VarianceAPI.Items
{
    public class ExtraUtility : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.VAPIAssets.LoadAsset<ItemDef>("ExtraUtility");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<ExtraUtilityBehavior>(stack);
        }

        public class ExtraUtilityBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalcStatsEnd()
            {
                var skillLocator = body.skillLocator;
                if(skillLocator)
                {
                    if((bool)skillLocator.utility)
                    {
                        skillLocator.utility.SetBonusStockFromBody(stack);
                    }
                }
            }

            public void RecalcStatsStart()
            {
            }
        }
    }
}
