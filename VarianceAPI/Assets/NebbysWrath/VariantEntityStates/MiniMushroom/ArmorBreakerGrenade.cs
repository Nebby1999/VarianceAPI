using EntityStates.MiniMushroom;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NebbysWrath.VariantEntityStates.MiniMushroom
{
    public class ArmorBreakerGrenade : SporeGrenade
    {
        public override void OnEnter()
        {
            projectilePrefab = Projectiles.ArmorBreakerGrenade.projectile;
            base.OnEnter();
        }
    }
}
