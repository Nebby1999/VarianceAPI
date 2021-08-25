using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Modules.Prefabs;

namespace NebbysWrath.Prefabs.ImpOverlord
{
    public class IchorSpike : PrefabBase<IchorSpike>
    {
        public static GameObject prefab;
        public override void Init()
        {
            BuildPrefab();
            prefab = PrefabObject;
            CreateProjectilePrefab(PrefabObject);
        }
        internal void BuildPrefab()
        {
            PrefabObject = InstantiatePrefabClone("Prefabs/Projectiles/ImpVoidSpikeProjectile", "IchorSpike");
            var projectileDamage = PrefabObject.GetComponent<ProjectileDamage>();
            projectileDamage.damageType = RoR2.DamageType.Generic;
            ProjectileController ghostPrefab = PrefabObject.GetComponent<ProjectileController>();
            ghostPrefab.ghostPrefab = InstantiatePrefabClone("Prefabs/ProjectileGhosts/ImpVoidspikeProjectileGhost", "IchorSpikeGhost");
            ghostPrefab.ghostPrefab.GetComponent<Light>().color = new Color(0.98f, 0.71f, 0, 1);
            ghostPrefab.ghostPrefab.GetComponentInChildren<MeshRenderer>().material = MainClass.nebbysWrathAssets.LoadAsset<Material>("IchorClaw");
        }
    }
}
