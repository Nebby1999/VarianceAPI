using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.Projectile;

namespace VarianceAPI.Projectiles
{
    public abstract class ProjectileBase
    {
        public abstract GameObject ProjectilePrefab { get; set; }

        public virtual void Initialize() { }
    }
}
