using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Modules;

namespace VarianceAPI.Items
{
    public class Plus1Crit : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.VAPIAssets.LoadAsset<ItemDef>("Plus1Crit");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<Plus1CritBehavior>(stack);
        }

        public class Plus1CritBehavior : CharacterBody.ItemBehavior, IStatItemBehavior
        {
            public void RecalcStatsEnd()
            {
                body.crit += stack;
            }

            public void RecalcStatsStart()
            {
            }
        }
    }
}