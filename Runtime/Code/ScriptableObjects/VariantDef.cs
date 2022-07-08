using UnityEngine;
using R2API;
using System.Collections.Generic;
using System.Linq;
using Moonstorm.AddressableAssets;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New VariantDef", menuName = "VarianceAPI/VariantDef")]
    public class VariantDef : ScriptableObject
    {
        public VariantIndex VariantIndex { get; internal set; }

        public string bodyName;

        public bool isUnique = false;

        [Range(0, 100)]
        public float spawnRate;

        [Tooltip("The conditions specified here need to be met for this variant to spawn, leave this blank if you want the variant to not have spawning restrictions")]
        public VariantSpawnCondition variantSpawnConditions;
    }
}
