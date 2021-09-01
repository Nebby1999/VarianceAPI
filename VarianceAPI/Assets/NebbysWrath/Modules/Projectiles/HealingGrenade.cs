using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Projectiles;

namespace NebbysWrath.Projectiles
{
    public class HealingGrenade : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; set; } = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/sporegrenadeprojectile"), "HealingGrenade", false);

        public static GameObject projectile;

        public static GameObject DOTHealingZone;

        public override void Initialize()
        {
            var impactExplosion = ProjectilePrefab.GetComponent<ProjectileImpactExplosion>();
            var healingChild = impactExplosion.childrenProjectilePrefab.InstantiateClone("HealingWard", false);
            var healingWard = healingChild.AddComponent<HealingWard>();
            healingWard.radius = 15;
            healingWard.interval = 0.5f;
            healingWard.rangeIndicator = ProjectilePrefab.transform;
            impactExplosion.childrenProjectilePrefab = healingChild;

            HG.ArrayUtils.ArrayAppend(ref ContentPackProvider.serializedContentPack.projectilePrefabs, healingChild);

            DOTHealingZone = healingChild;

            ProjectileController controller = ProjectilePrefab.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(controller.ghostPrefab, "HealingGrenadeGhost", false);
            ghostPrefab.GetComponentInChildren<MeshRenderer>().material = MainClass.nebbysWrathAssets.LoadAsset<Material>("matHealerShroom");

            controller.ghostPrefab = ghostPrefab;

            projectile = ProjectilePrefab;
        }
    }
}
