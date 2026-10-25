#nullable enable
using EntityStates;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using VAPI.AddressReferencedAssets;

namespace VAPI
{
    /// <summary>
    /// Class used to build <see cref="CharacterVariantDef"/>s at runtime.
    /// <br></br>
    /// Contains a variety of chaning methods to create the variants themselves.
    /// <para></para>
    /// Keep in mind that all of VAPI 3.0's types for a CharacterVariantDef (Interfaces, Classes and Structs) should not be reused across diferent variants. As a result it is recommended to utilize this builder, as it will class said types' "Clone" method.
    /// </summary>
    public sealed class CharacterVariantDefBuilder
    {
        private class CharacterVariantDefBuilderException : Exception
        {
            public CharacterVariantDefBuilderException() { }
            public CharacterVariantDefBuilderException(string message) : base(message) { }
            public CharacterVariantDefBuilderException(string message, Exception innerException) : base(message, innerException) { }
        }

        /// <summary>
        /// The name value for the CharacterVariantDef that will be built
        /// </summary>
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
        /// <summary>
        /// The target character for this variant
        /// </summary>
        public CharacterVariantTarget targetBody = new CharacterVariantTarget();

        /// <summary>
        /// The Tier for this variant, can be null
        /// </summary>
        public CharacterVariantTierDef? characterVariantTier = null;

        /// <summary>
        /// Wether the variant is unique or not by default. This shouldn't be configured directly as VAPI's systems will configure it properly.
        /// </summary>
        public bool isUnique = false;

        /// <summary>
        /// The Spawn rate (0 -> 100) for this variant to spawn. Cannot be NaN, Infinity or Subnormal.
        /// <br></br>
        /// Gets clamped between 0 and 100
        /// </summary>
        public float spawnRate
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
        private float _spawnRate;

        /// <summary>
        /// A token to display in the chat when this variant spawns, can be left null.
        /// </summary>
        public string? arrivalToken = null;

        /// <summary>
        /// The Spawn Condition for this variant, see <see cref="IVariantSpawnCondition"/> for more information
        /// </summary>
        public IVariantSpawnCondition? spawnCondition = null;

        /// <summary>
        /// A list of <see cref="IVariantMasterModifier"/>s for this variant, see said interface for more information.
        /// </summary>
        public readonly List<IVariantMasterModifier> masterModifiers = new List<IVariantMasterModifier>();

        /// <summary>
        /// The inventory definition for this variant, can be left null.
        /// </summary>
        public VariantInventoryDefinition? inventoryDefinition = new VariantInventoryDefinition();

        /// <summary>
        /// The name provider for this variant, can be left null. See <see cref="IVariantNameProvider"/> for more information
        /// </summary>
        public IVariantNameProvider? nameProvider = null;

        /// <summary>
        /// An <see cref="EntityState"/> that'll be used as the variant's death state.
        /// </summary>
        public VariantDeathStateOverride deathStateOverride = new VariantDeathStateOverride("");

        /// <summary>
        /// A list of <see cref="VariantSkillReplacement"/>s for this variant
        /// </summary>
        public readonly List<VariantSkillReplacement> skillReplacements = new List<VariantSkillReplacement>();

        /// <summary>
        /// A StatModifier for this variant, see <see cref="IVariantStatModifier"/> for more information
        /// </summary>
        public IVariantStatModifier? statModifier = null;

        /// <summary>
        /// The BuffStorage for this variant
        /// </summary>
        public VariantBuffStorage? variantBuffs = new VariantBuffStorage();

        /// <summary>
        /// The visual modifier for this variant
        /// </summary>
        public CharacterVariantVisualModifier? visualModifier = null;

        /// <summary>
        /// The scale multiplier for this variant, value cannot be NaN, Infinity or Subnormal.
        /// <para></para>
        /// Ensures value cannot be <= to 0.
        /// </summary>
        public float scaleMultiplier
        {
            get => _scaleMultiplier;
            set
            {
                if(!float.IsNormal(value))
                {
                    throw new Exception("scaleMultiplier cannot be NaN, Infinity or Subnormal");
                }
                _scaleMultiplier = Mathf.Max(0 + float.Epsilon, value);
            }
        }
        private float _scaleMultiplier = 1;

        /// <summary>
        /// The components for this variant
        /// </summary>
        public VariantComponentCollection? variantComponents = new VariantComponentCollection();

        #region Chaining Methods
        /// <summary>Chainable method, sets <see cref="characterVariantDefName"/> to <paramref name="name"/></summary>
        public CharacterVariantDefBuilder SetCharacterVariantDefName(string name)
        {
            characterVariantDefName = name;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="targetBody"/> to <paramref name="target"/></summary>
        public CharacterVariantDefBuilder SetCharacterTarget(CharacterVariantTarget target)
        {
            targetBody = target;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="characterVariantTier"/> to <paramref name="tierDef"/></summary>
        public CharacterVariantDefBuilder SetVariantTier(CharacterVariantTierDef? tierDef)
        {
            characterVariantTier = tierDef;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="isUnique"/> to <paramref name="_isUnique"/></summary>
        public CharacterVariantDefBuilder SetIsUnique(bool _isUnique)
        {
            isUnique = _isUnique;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="spawnRate"/> to <paramref name="_spawnRate"/></summary>
        public CharacterVariantDefBuilder SetSpawnRate(float _spawnRate)
        {
            spawnRate = _spawnRate;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="arrivalToken"/> to <paramref name="_arrivalToken"/></summary>
        public CharacterVariantDefBuilder SetArrivalToken(string? _arrivalToken)
        {
            arrivalToken = _arrivalToken;
            return this;
        }

        /// <summary>Chainable method, adds <paramref name="modifier"/> to <see cref="masterModifiers"/></summary>
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

        /// <summary>Chainable method, Sets <see cref="inventoryDefinition"/> to <paramref name="_inventoryDefinition"/></summary>
        public CharacterVariantDefBuilder SetVariantInventoryDefinition(VariantInventoryDefinition _inventoryDefinition)
        {
            inventoryDefinition = _inventoryDefinition;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="nameProvider"/> to <paramref name="_nameProvider"/></summary>
        public CharacterVariantDefBuilder SetNameProvider(IVariantNameProvider? _nameProvider)
        {
            nameProvider = _nameProvider;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="deathStateOverride"/> to the type in <typeparamref name="T"/></summary>
        public CharacterVariantDefBuilder SetDeathStateOverride<T>() where T : EntityState => SetDeathStateOverride(new VariantDeathStateOverride(typeof(T)));

        /// <summary>Chainable method, sets <see cref="deathStateOverride"/> to <paramref name="deathOverride"/></summary>
        public CharacterVariantDefBuilder SetDeathStateOverride(VariantDeathStateOverride deathOverride)
        {
            deathStateOverride = deathOverride;
            return this;
        }

        public CharacterVariantDefBuilder AddSkillReplacement(SkillDef? skill, string skillSlotName) => AddSkillReplacement(new VariantSkillReplacement(skill, skillSlotName));
        public CharacterVariantDefBuilder AddSkillReplacement(SkillDef? skill, SkillSlot slot) => AddSkillReplacement(new VariantSkillReplacement(skill, slot));
        /// <summary>Chainable method, adds <paramref name="replacement"/> to <see cref="skillReplacements"/></summary>
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

        /// <summary>Chainable method, sets <see cref="statModifier"/> to <paramref name="_statModifier"/></summary>
        public CharacterVariantDefBuilder SetStatModifier(IVariantStatModifier? _statModifier)
        {
            statModifier = _statModifier;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="variantBuffs"/> to <paramref name="storage"/></summary>
        public CharacterVariantDefBuilder SetVariantBuffs(VariantBuffStorage storage)
        {
            variantBuffs = storage;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="visualModifier"/> to <paramref name="_visualModifier"/></summary>
        public CharacterVariantDefBuilder SetVisualModifier(CharacterVariantVisualModifier _visualModifier)
        {
            visualModifier = _visualModifier;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="scaleMultiplier"/> to <paramref name="_scaleMultiplier"/></summary>
        public CharacterVariantDefBuilder SetScaleMultiplier(float _scaleMultiplier)
        {
            this.scaleMultiplier = _scaleMultiplier;
            return this;
        }

        /// <summary>Chainable method, sets <see cref="variantComponents"/> to <paramref name="collection"/></summary>
        public CharacterVariantDefBuilder SetComponentCollection(VariantComponentCollection collection)
        {
            variantComponents = collection;
            return this;
        }
        #endregion

        /// <summary>
        /// Builds the <see cref="CharacterVariantDef"/> from the data within the builder
        /// </summary>
        /// <returns>The created variant, returns null if an exception occurs</returns>
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

                result.targetBody = (CharacterVariantTarget)targetBody.Clone();
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

            if (!targetBody.IsKeyValid())
                throw new CharacterVariantDefBuilderException("The targetBody needs to have a valid reference to a Body. These may be an Address, Prefab Name for Catalog Querying, or Direct Reference.");

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