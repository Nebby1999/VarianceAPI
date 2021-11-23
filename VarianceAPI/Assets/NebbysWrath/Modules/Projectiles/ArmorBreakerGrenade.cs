using Moonstorm;
using R2API;
using RoR2.Projectile;
using UnityEngine;
using static R2API.DamageAPI;

namespace NebbysWrath.Projectiles
{
    public class ArmorBreakerGrenade : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; set; } = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/sporegrenadeprojectile"), "HealingGrenade", false);

        public static GameObject projectile;

        public static GameObject DOTHealingZone;

        public override void Initialize()
        {
            var impactExplosion = ProjectilePrefab.GetComponent<ProjectileImpactExplosion>();
            var armorBreakerChild = impactExplosion.childrenProjectilePrefab.InstantiateClone("BreakerWard");
            var moddedDamageType = armorBreakerChild.AddComponent<ModdedDamageTypeHolderComponent>();
            moddedDamageType.Add(DamageTypes.PulverizeOnHit.pulverizeOnHit);

            impactExplosion.childrenProjectilePrefab = armorBreakerChild;

            HG.ArrayUtils.ArrayAppend(ref ContentPackProvider.serializedContentPack.projectilePrefabs, armorBreakerChild);

            ProjectileController controller = ProjectilePrefab.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(controller.ghostPrefab, "HealingGrenadeGhost", false);
            ghostPrefab.GetComponentInChildren<MeshRenderer>().material = MainClass.nebbysWrathAssets.LoadAsset<Material>("matADShroom");

            controller.ghostPrefab = ghostPrefab;

            projectile = ProjectilePrefab;
        }
    }
}
