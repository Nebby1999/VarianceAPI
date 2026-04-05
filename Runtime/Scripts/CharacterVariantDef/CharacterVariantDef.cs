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
using RoR2;
using MSU;
#nullable enable

namespace VAPI
{
    [Serializable]
    public struct VariantBuffInfo
    {
        private readonly struct DisposableVariantBuff : IDisposable
        {
            public readonly CharacterBody targetBody;
            public readonly BuffDef buffDef;
            public readonly int buffCount;

            public DisposableVariantBuff(CharacterBody targetBody, BuffDef buffDef, int buffCount)
            {
                this.targetBody = targetBody;
                this.buffDef = buffDef;
                this.buffCount = buffCount;
            }

            public void Dispose()
            {
                if(!targetBody)
                {
                    return;
                }

                for(int i = 0; i < buffCount; i++)
                {
                    targetBody.RemoveBuff(buffDef);
                }
            }
        }
        public AddressReferencedBuffDef? buffDef;
        public int count;

        public IDisposable? ApplyBuff(CharacterBody targetBody)
        {
            if (buffDef == null || !targetBody)
                return null;

            BuffDef buff = buffDef.LoadAssetNow();
            if (!buff)
                return null;

            DisposableVariantBuff disposableVariantBuff = new DisposableVariantBuff(targetBody, buff, count);
            for (int i = 0; i < count; i++)
            {
                targetBody.AddBuff(buff);
            }
            return disposableVariantBuff;
        }
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
        [Min(1)]
        public float scaleMultiplier = 1f;

        [Header("Other")]
        public VariantComponentCollection additionalVariantComponents = new VariantComponentCollection();

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