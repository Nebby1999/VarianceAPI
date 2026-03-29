#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace VAPI
{
    [CreateAssetMenu(fileName = "New CharacterVariantDef", menuName = "VarianceAPI/CharacterVariantDef")]
    public sealed class CharacterVariantDef : ScriptableObject
    {
        [Header("General Settings")]
        public VariantCharacterTarget targetCharacter = new VariantCharacterTarget();
        //TODO: Add VariantTierDefs
        //public VariantTierDef variantTierDef;
        public bool allowMerging;
        [Range(0, 100)]
        public float spawnRate;

        [SerializeReference]
        public IVariantSpawnCondition spawnCondition = new AlwaysAvailableSpawnCondition();

        [ContextMenu("Spawn Conditions/Basic Spawn Conditions")]
        private void BasicSpawnCondition()
        {
            spawnCondition = new BasicSpawnCondition();
        }

        [ContextMenu("Spawn Conditions/Always Available")]
        private void AlwaysAvailableSpawnCondition()
        {
            spawnCondition = new AlwaysAvailableSpawnCondition();
        }
    }
}