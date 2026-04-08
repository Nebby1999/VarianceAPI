#nullable enable

using EntityStates;
using RoR2.Skills;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using HG;
using System;

namespace VAPI
{
    public class CharacterVariantFactory
    {
        private class CharacterVariantFactoryException : Exception
        {
            public CharacterVariantFactoryException() { }
            public CharacterVariantFactoryException(string message) : base(message) { }
            public CharacterVariantFactoryException(string message, Exception inner) : base(message, inner) { }
        }
        #region CharacterVariantDefFields
        private VariantCharacterTarget? _characterTarget;
        private CharacterVariantTierDef? _tierDef;
        private bool _isUnique = false;
        private float _spawnRate = 0;
        private string _arrivalToken = "";
        private IVariantSpawnCondition? _spawnCondition;
        private List<IVariantMasterModifier> _masterModifiers = new List<IVariantMasterModifier>();
        private VariantInventoryDefinition _inventoryDefinition = new VariantInventoryDefinition();
        private IVariantNameProvider? _nameProvider;
        private SerializableEntityStateType? _deathStateOverride;
        private List<VariantSkillReplacement> _skillReplacements = new List<VariantSkillReplacement>();
        private IVariantStatModifier? _statModifier;
        private List<VariantBuffInfo> _buffInfos = new List<VariantBuffInfo>();
        private float _scaleMultiplier = 1;
        private VariantVisualModifier? _visualModifier;
        private List<SerializableSystemType> _componentCollection = new List<SerializableSystemType>();
        #endregion

        public CharacterVariantFactory WithTargetCharacter(VariantCharacterTarget variantCharacterTarget)
        {
            _characterTarget = variantCharacterTarget;
            return this;
        }

        public CharacterVariantFactory WithTier(CharacterVariantTierDef tierDef)
        {
            _tierDef = tierDef;
            return this;
        }

        public CharacterVariantFactory WithUniqueness(bool isUnique)
        {
            _isUnique = isUnique;
            return this;
        }

        public CharacterVariantFactory WithSpawnRate(float spawnRate0To100)
        {
            _spawnRate = Mathf.Clamp(spawnRate0To100, 0, 100);
            return this;
        }

        public CharacterVariantFactory WithArrivalToken(string arrivalToken)
        {
            _arrivalToken = arrivalToken;
            return this;
        }

        public CharacterVariantFactory WithSpawnCondition(BasicSpawnCondition condition)
        {
            return WithSpawnCondition((IVariantSpawnCondition)condition);
        }
        public CharacterVariantFactory WithSpawnCondition(IVariantSpawnCondition condition)
        {
            _spawnCondition = condition;
            return this;
        }

        public CharacterVariantFactory ClearMasterModifiers()
        {
            _masterModifiers.Clear();
            return this;
        }
        public CharacterVariantFactory AddMasterModifier(BaseAIDampModifier modifier) => AddMasterModifier((IVariantMasterModifier)modifier);
        public CharacterVariantFactory AddMasterModifier(AlwaysSprintAIModifier modifier) => AddMasterModifier((IVariantMasterModifier)modifier);
        public CharacterVariantFactory AddMasterModifier(UnstableAIModifier modifier) => AddMasterModifier((IVariantMasterModifier)modifier);
        public CharacterVariantFactory AddMasterModifier(IVariantMasterModifier masterModifier)
        {
            _masterModifiers.Add(masterModifier);
            return this;
        }

        public CharacterVariantFactory WithInventoryDefinition(VariantInventoryDefinition inventoryDefinition)
        {
            _inventoryDefinition = inventoryDefinition;
            return this;
        }

        public CharacterVariantFactory WithNameProvider(VariantNameFormatter nameProvider) => WithNameProvider((IVariantNameProvider)nameProvider);
        public CharacterVariantFactory WithNameProvider(VariantNamePrefix nameProvider) => WithNameProvider((IVariantNameProvider)nameProvider);
        public CharacterVariantFactory WithNameProvider(VariantNameSuffix nameProvider) => WithNameProvider((IVariantNameProvider)nameProvider);
        public CharacterVariantFactory WithNameProvider(VariantNameOverride nameOverride) => WithNameProvider((IVariantNameProvider)nameOverride);
        public CharacterVariantFactory WithNameProvider(IVariantNameProvider nameProvider)
        {
            _nameProvider = nameProvider;
            return this;
        }

        public CharacterVariantFactory WithDeathStateOverride<T>() where T : EntityState => WithDeathStateOverride(new SerializableEntityStateType(typeof(T)));
        public CharacterVariantFactory WithDeathStateOverride(SerializableEntityStateType stateType)
        {
            _deathStateOverride = stateType;
            return this;
        }

        public CharacterVariantFactory ClearSkillReplacements()
        {
            _skillReplacements.Clear();
            return this;
        }
        public CharacterVariantFactory AddSkillReplacement(SkillDef? skillDef, SkillSlot slot) => AddSkillReplacement(new VariantSkillReplacement(skillDef, slot));
        public CharacterVariantFactory AddSkillReplacement(SkillDef? skillDef, string slotName) => AddSkillReplacement(new VariantSkillReplacement(skillDef, slotName));
        public CharacterVariantFactory AddSkillReplacement(VariantSkillReplacement replacement)
        {
            _skillReplacements.Add(replacement);
            return this;
        }

        public CharacterVariantFactory WithStatModifier(BasicStatModifier statModifier) => WithStatModifier((IVariantStatModifier)statModifier);
        public CharacterVariantFactory WithStatModifier(IVariantStatModifier statModifier)
        {
            _statModifier = statModifier;
            return this;
        }

        public CharacterVariantFactory ClearBuffInfos()
        {
            _buffInfos.Clear();
            return this;
        }
        public CharacterVariantFactory AddBuffInfo(VariantBuffInfo buffInfo)
        {
            _buffInfos.Add(buffInfo);
            return this;
        }

        public CharacterVariantFactory WithVisualModifier(VariantVisualModifier modifier)
        {
            _visualModifier = modifier;
            return this;
        }

        public CharacterVariantFactory WithScaleMultiplier(float scaleMultiplier)
        {
            _scaleMultiplier = Mathf.Max(0 + float.Epsilon, scaleMultiplier);
            return this;
        }

        public CharacterVariantFactory ClearComponentCollection()
        {
            _componentCollection.Clear();
            return this;
        }
        public CharacterVariantFactory AddVariantComponent<T>() where T : VariantComponent => AddVariantComponent((SerializableSystemType)typeof(T));
        public CharacterVariantFactory AddVariantComponent(SerializableSystemType type)
        {
            _componentCollection.Add(type);
            return this;
        }

        public CharacterVariantDef Build(string assetName)
        {
            var result = ScriptableObject.CreateInstance<CharacterVariantDef>();
            try
            {
                result.name = assetName;

                if (_characterTarget == null)
                    throw new InvalidOperationException($"Cannot build Variant {assetName} when there's no CharacterTarget");
                result.targetCharacter = _characterTarget;

                result.variantTier = _tierDef;
                result.isUnique = _isUnique;
                result.spawnRate = _spawnRate;
                result.arrivalToken = _arrivalToken;
                result.spawnCondition = _spawnCondition;

                for(int i = 0; i < _masterModifiers.Count; i++)
                {
                    if (_masterModifiers[i] == null)
                    {
                        VAPILog.Warning($"MasterModifier index {i} while building Variant {assetName} is null.");
                    }
                }
                result.masterModifiers = _masterModifiers.ToArray();
                result.inventoryDefinition = _inventoryDefinition;
                result.variantNameProvider = _nameProvider;
                result.deathStateOverride = _deathStateOverride.HasValue ? new VariantDeathStateOverride(_deathStateOverride.Value) : new VariantDeathStateOverride();

                for (int i = 0; i < _skillReplacements.Count; i++)
                {
                    if (_skillReplacements[i] == null)
                    {
                        throw new InvalidOperationException($"Cannot build Variant {assetName} as it has a null SkillReplacement at index {i}");
                    }
                }
                result.skillReplacements = _skillReplacements.ToArray();
                result.statModifier = _statModifier;
                result.variantBuffs = new VariantBuffStorage(_buffInfos.ToArray());
                result.visualModifier = _visualModifier;
                result.scaleMultiplier = _scaleMultiplier;
                result.additionalVariantComponents = new VariantComponentCollection(_componentCollection.ToArray());
            }
            catch(Exception e)
            {
#if DEBUG
                VAPILog.Error($"Caught Exception during CharacterVariantFactory, please note that these exceptions are not caught on release builds. {e}");
#else
                throw new CharacterVariantFactoryException("Exception during CharacterVariantFactory.Build()", e);
#endif
            }
            return result;
        }

        public void ClearFactory()
        {
            _characterTarget = null;
            _tierDef = null;
            _isUnique = false;
            _spawnRate = 0;
            _arrivalToken = "";
            _spawnCondition = null;
            _masterModifiers.Clear();
            _inventoryDefinition = new VariantInventoryDefinition();
            _nameProvider = null;
            _deathStateOverride = null;
            _skillReplacements.Clear();
            _statModifier = null;
            _buffInfos.Clear();
            _scaleMultiplier = 1;
            _componentCollection.Clear();
        }

        public CharacterVariantFactory() { }
        public CharacterVariantFactory(CharacterVariantFactory other)
        {
            _characterTarget = other._characterTarget;
            _tierDef = other._tierDef;
            _isUnique = other._isUnique;
            _spawnRate = other._spawnRate;
            _arrivalToken = other._arrivalToken;
            _spawnCondition = other._spawnCondition;
            _masterModifiers = new List<IVariantMasterModifier>(other._masterModifiers);
            _inventoryDefinition = other._inventoryDefinition;
            _nameProvider = other._nameProvider;
            _deathStateOverride = other._deathStateOverride;
            _skillReplacements = new List<VariantSkillReplacement>(other._skillReplacements);
            _statModifier = other._statModifier;
            _buffInfos = new List<VariantBuffInfo>(other._buffInfos);
            _scaleMultiplier = other._scaleMultiplier;
            _visualModifier = other._visualModifier;
            _componentCollection = new List<SerializableSystemType>(other._componentCollection);
        }
    }
}