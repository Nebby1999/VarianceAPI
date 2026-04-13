#nullable enable
using EntityStates;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    public class CharacterVariantDefBuilder
    {
        private class CharacterVariantDefBuilderException : Exception
        {
            public CharacterVariantDefBuilderException() { }
            public CharacterVariantDefBuilderException(string message) : base(message) { }
            public CharacterVariantDefBuilderException(string message, Exception innerException) : base(message, innerException) { }
        }

        public string characterVariantDefName
        {
            get => _characterVariantDefName;
            set
            {
                if(string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("The characterVariantDefName cannot be null, whitespace or empty.");
                }
                _characterVariantDefName = value;
            }
        }
        private string _characterVariantDefName = "";
        public CharacterVariantTarget characterTarget = new CharacterVariantTarget();
        public CharacterVariantTierDef? characterVariantTier = null;
        public bool isUnique = false;
        private float spawnRate
        {
            get => _spawnRate;
            set
            {
                if(!float.IsNormal(value))
                {
                    throw new Exception("SpawnRate cannot be NaN, Infinity or Subnormal");
                }
                _spawnRate = Mathf.Clamp(value, 0, 100);
            }
        }
        public float _spawnRate;
        public string? arrivalToken = null;
        public IVariantSpawnCondition? spawnCondition = null;
        public readonly List<IVariantMasterModifier> masterModifiers = new List<IVariantMasterModifier>();
        public VariantInventoryDefinition? inventoryDefinition = new VariantInventoryDefinition();
        public IVariantNameProvider? nameProvider = null;
        public VariantDeathStateOverride deathStateOverride;
        public readonly List<VariantSkillReplacement> skillReplacements = new List<VariantSkillReplacement>();
        public IVariantStatModifier? statModifier = null;
        public VariantBuffStorage? variantBuffs = new VariantBuffStorage();
        public CharacterVariantVisualModifier? visualModifier = null;
        public float scaleMultiplier
        {
            get => _scaleMultiplier;
            set
            {
                if(float.IsNormal(value))
                {
                    throw new Exception("scaleMultiplier cannot be NaN, Infinity or Subnormal");
                }
                _scaleMultiplier = Mathf.Max(0 + float.Epsilon, value);
            }
        }
        private float _scaleMultiplier;
        public VariantComponentCollection? variantComponents = new VariantComponentCollection();

        #region Chaining Methods
        public CharacterVariantDefBuilder SetCharacterVariantDefName(string name)
        {
            characterVariantDefName = name;
            return this;
        }

        public CharacterVariantDefBuilder SetCharacterTarget(CharacterVariantTarget target)
        {
            characterTarget = target;
            return this;
        }

        public CharacterVariantDefBuilder SetVariantTier(CharacterVariantTierDef? tierDef)
        {
            characterVariantTier = tierDef;
            return this;
        }

        public CharacterVariantDefBuilder SetIsUnique(bool _isUnique)
        {
            isUnique = _isUnique;
            return this;
        }

        public CharacterVariantDefBuilder SetSpawnRate(float _spawnRate)
        {
            spawnRate = _spawnRate;
            return this;
        }

        public CharacterVariantDefBuilder SetArrivalToken(string? _arrivalToken)
        {
            arrivalToken = _arrivalToken;
            return this;
        }

        public CharacterVariantDefBuilder AddMasterModifier(IVariantMasterModifier modifier)
        {
            if(modifier == null)
            {
                VAPILog.Warning($"Cannot add a null VariantMasterModifier!");
            }
            else
            {
                masterModifiers.Add(modifier);
            }
            return this;
        }

        public CharacterVariantDefBuilder SetVariantInventoryDefinition(VariantInventoryDefinition _inventoryDefinition)
        {
            inventoryDefinition = _inventoryDefinition;
            return this;
        }

        public CharacterVariantDefBuilder SetNameProvider(IVariantNameProvider? _nameProvider)
        {
            nameProvider = _nameProvider;
            return this;
        }

        public CharacterVariantDefBuilder SetDeathStateOverride<T>() where T : EntityState => SetDeathStateOverride(new VariantDeathStateOverride(typeof(T)));
        public CharacterVariantDefBuilder SetDeathStateOverride(VariantDeathStateOverride deathOverride)
        {
            deathStateOverride = deathOverride;
            return this;
        }

        public CharacterVariantDefBuilder AddSkillReplacement(VariantSkillReplacement replacement)
        {
            if (replacement == null)
            {
                VAPILog.Warning($"Cannot add a null VariantSkillReplacement!");
            }
            else
            {
                skillReplacements.Add(replacement);
            }
            return this;
        }

        public CharacterVariantDefBuilder SetStatModifier(IVariantStatModifier? _statModifier)
        {
            statModifier = _statModifier;
            return this;
        }

        public CharacterVariantDefBuilder SetVariantBuffs(VariantBuffStorage storage)
        {
            variantBuffs = storage;
            return this;
        }

        public CharacterVariantDefBuilder SetVisualModifier(CharacterVariantVisualModifier _visualModifier)
        {
            visualModifier = _visualModifier;
            return this;
        }

        public CharacterVariantDefBuilder SetScaleMultiplier(float scaleMultiplier)
        {
            _scaleMultiplier = scaleMultiplier;
            return this;
        }

        public CharacterVariantDefBuilder SetComponentCollection(VariantComponentCollection collection)
        {
            variantComponents = collection;
            return this;
        }
        #endregion

        public CharacterVariantDef? Build()
        {
            CharacterVariantDef? def = null;
            try
            {
                ValidateRequiredFields();

                CharacterVariantDef createdVariant = BuildInternal();
                if (createdVariant != null)
                    def = createdVariant;
            }
            catch(Exception e)
            {
                //catch it and log... say that it wont be caught but that's false lol. we do a little gaslighting
                VAPILog.Error($"Exception caught in CharacterVariantDefBuilder. This exception wont be caught in release builds. {e}");
            }
            return def;
        }

        private CharacterVariantDef BuildInternal()
        {
            try
            {
                CharacterVariantDef result = CharacterVariantDef.CreateInstance<CharacterVariantDef>(characterVariantDefName);

                result.targetCharacter = (CharacterVariantTarget)characterTarget.Clone();
                result.variantTier = characterVariantTier;
                result.isUnique = isUnique;
                result.spawnRate = spawnRate;
                result.arrivalToken = arrivalToken ?? "";
                result.spawnCondition = (IVariantSpawnCondition?)(spawnCondition?.Clone());

                HG.ArrayUtils.EnsureCapacity(ref result.masterModifiers, masterModifiers.Count);
                for(int i = 0; i < masterModifiers.Count; i++)
                {
                    result.masterModifiers[i] = (IVariantMasterModifier)masterModifiers[i].Clone();
                }

                result.inventoryDefinition = (VariantInventoryDefinition)(inventoryDefinition != null ? inventoryDefinition.Clone() : new VariantInventoryDefinition());
                result.variantNameProvider = (IVariantNameProvider?)(nameProvider?.Clone());
                result.deathStateOverride = (VariantDeathStateOverride)deathStateOverride.Clone();
                
                HG.ArrayUtils.EnsureCapacity(ref result.skillReplacements, skillReplacements.Count);
                for(int i = 0; i < skillReplacements.Count; i++)
                {
                    result.skillReplacements[i] = (VariantSkillReplacement)skillReplacements[i].Clone();
                }

                result.statModifier = (IVariantStatModifier?)(statModifier?.Clone());
                result.variantBuffs = (VariantBuffStorage)(variantBuffs != null ? variantBuffs.Clone() : new VariantBuffStorage());
                result.visualModifier = visualModifier;
                result.scaleMultiplier = scaleMultiplier;
                result.additionalVariantComponents = (VariantComponentCollection)(variantComponents != null ? variantComponents.Clone() : new VariantComponentCollection());

                return result;
            }
            catch(Exception e)
            {
                throw new CharacterVariantDefBuilderException("Exception during CharacterVariantDefBuilder.", e);
            }
        }

        private void ValidateRequiredFields()
        {
            if(string.IsNullOrWhiteSpace(characterVariantDefName))
                throw new CharacterVariantDefBuilderException("characterVariantDefName may not be null, empty or whitespace");

            if (!characterTarget.AnyAddressReferencedAssetValid())
                throw new CharacterVariantDefBuilderException("The characterTarget needs to have a valid reference to a Body or a Master. These may be an Address, Prefab Name for Catalog Querying, or Direct Reference.");

            if(!float.IsNormal(spawnRate))
                throw new CharacterVariantDefBuilderException($"The spawnRate is not a normal value. (value={spawnRate})");

            for(int i = 0; i < masterModifiers.Count; i++)
            {
                if (masterModifiers[i] == null)
                    throw new CharacterVariantDefBuilderException($"The MasterModifier at index {i} is null. This is not allowed.");
            }

            for(int i = 0; i < skillReplacements.Count; i++)
            {
                if (skillReplacements[i] == null)
                    throw new CharacterVariantDefBuilderException($"The SkillReplacement at index {i} is null. This is not allowed.");
            }

            if (!float.IsNormal(scaleMultiplier))
                throw new CharacterVariantDefBuilderException($"The scaleMultiplier is not a normal value. (value={scaleMultiplier})");
        }
    }
}