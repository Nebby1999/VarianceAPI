using R2API;
using UnityEngine;
using UnityEngine.Networking;

namespace VarianceAPI.Modules.Prefabs
{
    public abstract class PrefabBase<T> : PrefabBase where T : PrefabBase<T>
    {
        public static T instance { get; private set; }
    }

    public abstract class PrefabBase
    {
        public GameObject PrefabObject;

        public abstract void Init();
        protected GameObject InstantiatePrefabClone(string prefabPath, string prefabName)
        {
            var clone = PrefabAPI.InstantiateClone(Resources.Load<GameObject>(prefabPath), prefabName, true);
            if(!clone.GetComponent<NetworkIdentity>())
            {
                clone.AddComponent<NetworkIdentity>();
            }
            return clone;
        }
        protected void CreateProjectilePrefab(GameObject projectilePrefab)
        {
            if (projectilePrefab)
            {
                PrefabAPI.RegisterNetworkPrefab(projectilePrefab);
                ProjectileAPI.Add(projectilePrefab);
            }
        }
    }
}