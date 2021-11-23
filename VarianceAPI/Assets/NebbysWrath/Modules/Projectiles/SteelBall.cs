using Moonstorm;
using R2API;
using RoR2.Projectile;
using UnityEngine;

namespace NebbysWrath.Projectiles
{
    public class SteelBall : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; set; } = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/BellBall"), "SteelBall", false);

        public static GameObject projectile;

        public static GameObject preppedBomb;

        public override void Initialize()
        {
            ProjectilePrefab.transform.localScale *= 4;
            ProjectileController controller = ProjectilePrefab.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(controller.ghostPrefab, "SteelBallGhost", false);
            ghostPrefab.transform.localScale *= 4;
            ghostPrefab.GetComponentInChildren<MeshRenderer>().material = MainClass.nebbysWrathAssets.LoadAsset<Material>("matSteelContraption");

            controller.ghostPrefab = ghostPrefab;

            projectile = ProjectilePrefab;
        }
    }
}
