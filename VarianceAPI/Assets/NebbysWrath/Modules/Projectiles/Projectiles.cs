using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Projectiles;
using VarianceAPI.ModuleBases;
using RoR2.ContentManagement;
using System.Reflection;
using UnityEngine;

namespace NebbysWrath.Projectiles
{
    public class Projectiles : ProjectileModuleBase
    {
        public override SerializableContentPack ContentPack { get; set; } = ContentPackProvider.serializedContentPack;
        public override Assembly Assembly { get; set; } = typeof(Projectiles).Assembly;

        public static Dictionary<GameObject, ProjectileBase> NWProjectiles = new Dictionary<GameObject, ProjectileBase>();

        public override void Initialize()
        {
            base.Initialize();
            InitializeProjectiles();
        }
        public override IEnumerable<ProjectileBase> InitializeProjectiles()
        {
            base.InitializeProjectiles()
                .ToList()
                .ForEach(projectileBase =>
                {
                    HG.ArrayUtils.ArrayAppend(ref ContentPack.projectilePrefabs, projectileBase.ProjectilePrefab);

                    projectileBase.Initialize();

                    loadedProjectiles.Add(projectileBase.ProjectilePrefab, projectileBase);

                    NWProjectiles.Add(projectileBase.ProjectilePrefab, projectileBase);
                });

            return null;
        }
    }
}
