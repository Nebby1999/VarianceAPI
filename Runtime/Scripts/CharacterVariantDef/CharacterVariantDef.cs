#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.UIElements;
using EntityStates;
using HG;
using UnityEngine.Networking;
using R2API.AddressReferencedAssets;
#nullable enable

namespace VAPI
{
    [Serializable]
    public struct VariantBuffInfo
    {
        public AddressReferencedBuffDef? buffDef;
        public int count;
        public float timer;
    }

    [CreateAssetMenu(fileName = "New CharacterVariantDef", menuName = "VarianceAPI/CharacterVariantDef")]
    public sealed class CharacterVariantDef : ScriptableObject
    {
        public CharacterVariantIndex characterVariantIndex { get; internal set; }
        [Header("General Settings")]
        public VariantCharacterTarget targetCharacter = new VariantCharacterTarget();
        public VariantTierDef? variantTier = null;
        public bool isUnique;
        [Range(0, 100)]
        public float spawnRate;
        public string arrivalToken = "";

        [Header("Spawn Conditions")]
        [SerializeReference, SubclassSelector]
        public IVariantSpawnCondition? spawnCondition = null;

        [Header("Master Related")]
        [SerializeReference, SubclassSelector]
        public IVariantMasterModifier?[] masterModifiers = Array.Empty<IVariantMasterModifier?>();
        public VariantInventoryDefinition inventoryDefinition = new VariantInventoryDefinition();

        [Header("Body Related")]
        [SerializeReference, SubclassSelector]
        public IVariantNameProvider? variantNameProvider = null;
        public SerializableEntityStateType deathStateOverride;
        public VariantSkillReplacement[] skillReplacements = Array.Empty<VariantSkillReplacement>();
        [SerializeReference, SubclassSelector]
        public IVariantStatModifier? statModifier = null;
        public VariantBuffInfo[] buffInfos = Array.Empty<VariantBuffInfo>();
        public VariantVisualModifier? visualModifier = null;
        public float scaleMultiplier = 1f;

        [Header("Other")]
        [SerializableSystemType.RequiredBaseType(typeof(VariantComponent))]
        public SerializableSystemType[] additionalComponents = Array.Empty<SerializableSystemType>();

        private void OnValidate()
        {
            spawnCondition?.Validate();
            variantNameProvider?.Validate();
            statModifier?.Validate();
            foreach(var masterModifier in masterModifiers)
            {
                masterModifier?.Validate();
            }
        }

        public bool IsAvailable()
        {
            if (spawnRate == 0)
                return false;

            //TODO: Check for rulebook

            return spawnCondition?.IsAvailable() ?? true;
        }
    }

}