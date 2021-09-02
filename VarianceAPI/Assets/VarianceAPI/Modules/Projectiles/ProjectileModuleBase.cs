using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Projectiles;

namespace VarianceAPI.ModuleBases
{
    public abstract class ProjectileModuleBase
    {
        public static bool delegates = false;

        public static Dictionary<GameObject, ProjectileBase> loadedProjectiles = new Dictionary<GameObject, ProjectileBase>();

        public GameObject[] LoadedProjectiles
        {
            get
            {
                return ContentPack.projectilePrefabs;
            }
        }

        public abstract SerializableContentPack ContentPack { get; set; }

        public abstract Assembly Assembly { get; set; }

        public virtual void Initialize()
        {
            if(!delegates)
            {
                delegates = true;
            }
        }

        public virtual IEnumerable<ProjectileBase> InitializeProjectiles()
        {
            return Assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ProjectileBase)))
                .Select(projectileBase => (ProjectileBase)Activator.CreateInstance(projectileBase));
        }
    }
}
