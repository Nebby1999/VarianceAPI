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
    public class HealingGrenade : SporeGrenade
    {
        public override void OnEnter()
        {
            projectilePrefab = Projectiles.HealingGrenade.projectile;
            var healingWard = projectilePrefab.GetComponent<ProjectileImpactExplosion>().childrenProjectilePrefab.GetComponent<HealingWard>();
            healingWard.healPoints = characterBody.baseMaxHealth / 10;
            healingWard.healFraction = 0.01f;
            base.OnEnter();
        }
    }
}
