using R2API;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Projectiles;
using static R2API.DamageAPI;

namespace NebbysWrath.Projectiles
{
    public class IchorSpike : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; set; } = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/ImpVoidSpikeProjectile"), "IchorSpike", false);

        public static GameObject ichorSpike;

        public override void Initialize()
        {
            var damageComponent = ProjectilePrefab.GetComponent<ProjectileDamage>();
            damageComponent.damageType = RoR2.DamageType.Generic;

            var damageTypeComponent = ProjectilePrefab.AddComponent<ModdedDamageTypeHolderComponent>();
            damageTypeComponent.Add(DamageTypes.PulverizeOnHit.pulverizeOnHit);

            var controller = ProjectilePrefab.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(controller.ghostPrefab, "IchorSpikeGhost", false);
            ghostPrefab.GetComponent<Light>().color = new Color(0.98f, 0.71f, 0, 1);

            var material = MainClass.nebbysWrathAssets.LoadAsset<Material>("matIchorClaw");
            var meshRenderer = ghostPrefab.GetComponentInChildren<MeshRenderer>();

            meshRenderer.material = material;
            meshRenderer.sharedMaterial = material;

            controller.ghostPrefab = ghostPrefab;

            ichorSpike = ProjectilePrefab;
        }
    }
}
