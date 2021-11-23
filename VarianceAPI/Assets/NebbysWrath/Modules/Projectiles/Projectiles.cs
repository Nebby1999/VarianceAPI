using Moonstorm;
using RoR2.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NebbysWrath.Projectiles
{
    public class Projectiles : ProjectileModuleBase
    {
        public override SerializableContentPack ContentPack { get; set; } = ContentPackProvider.serializedContentPack;

        public static Dictionary<GameObject, ProjectileBase> NWProjectiles = new Dictionary<GameObject, ProjectileBase>();

        public override void Init()
        {
            MainClass.logger.LogInfo($"Initializing Projectiles.");
            base.Init();
            InitializeProjectiles();
        }
        public override IEnumerable<ProjectileBase> InitializeProjectiles()
        {
            base.InitializeProjectiles()
                .ToList()
                .ForEach(projectileBase => AddProjectile(projectileBase, ContentPack));

            return null;
        }
    }
}
