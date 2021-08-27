using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Modules;

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
            public void RecalcStatsEnd()
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

            public void RecalcStatsStart()
            {
            }
        }
    }
}