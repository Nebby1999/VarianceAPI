using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Modules;

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
            public void RecalcStatsEnd()
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

            public void RecalcStatsStart()
            {
            }
        }
    }
}