using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Modules;

namespace VarianceAPI.Items
{
    public class SpecialCDR : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.VAPIAssets.LoadAsset<ItemDef>("SpecialCDR");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<SpecialCDRBehavior>(stack);
        }

        public class SpecialCDRBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalcStatsEnd()
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

            public void RecalcStatsStart()
            {
            }
        }
    }
}