#nullable enable

using R2API;
using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// An <see cref="ICloneable"/>, <see cref="IValidatable"/> interface used to modify the stats of a variant.
    /// <br></br>
    /// Utilizes R2API's RecalculateStats internally.
    /// <br></br>
    /// Must support cloning via the <see cref="ICloneable"/> implementation, as variants shouldnt share the same instance of a StatModifier.
    /// <para></para>
    /// <b>Built in Stat Modifiers:</b>
    /// <list type="bullet">
    /// <item><see cref="BasicStatModifier"/>: The reimplementation of VAPI2.0's stat modifiers.</item>
    /// </list>
    /// </summary>
    public interface IVariantStatModifier : IValidatable, ICloneable
    {
        public void ApplyStatModifiers(R2API.RecalculateStatsAPI.StatHookEventArgs args, CharacterBody targetBody);
    }

    /// <summary>
    /// A stat modifier that reimplements VAPI2.0's stat modifiers.
    /// <br></br>
    /// Open to inheritance, make sure to override <see cref="ApplyStatModifiers(RecalculateStatsAPI.StatHookEventArgs, CharacterBody)"/>, and <see cref="Clone"/>
    /// </summary>
    [Serializable]
    public class BasicStatModifier : IVariantStatModifier
    {
        /// <summary>
        /// The health multiplier for this variant
        /// </summary>
        public float healthMultiplier { get => _healthMultiplier; set => _healthMultiplier = Mathf.Max(0, value); }
        [Tooltip("The health multiplier for this variant")]
        [SerializeField, Min(0)]
        private float _healthMultiplier = 1;

        [Tooltip("Flat bonus to the variant's regeneration, if negative it'll reduce it's regeneration")]
        public float regenBonus = 0;

        /// <summary>
        /// Regeneration multiplier for this variant
        /// </summary>
        public float regenMultiplier { get => _regenMultiplier; set => _regenMultiplier = Mathf.Max(0, value); }
        [Tooltip("The Regeneration multiplier for this vairant")]
        [SerializeField, Min(0)]
        private float _regenMultiplier = 0;

        [Tooltip("Flat bonus to the variant's shields, if negative it'll reduce it's shields")]
        public float shieldBonus = 0;

        /// <summary>
        /// Shield multiplier for this variant
        /// </summary>
        public float shieldMultiplier { get => _shieldMultiplier; set => _shieldMultiplier = Mathf.Max(0, value); }
        [Tooltip("The Shield multiplier for this variant")]
        [SerializeField, Min(0)]
        private float _shieldMultiplier = 1;

        /// <summary>
        /// Movement Speed multiplier for this variant
        /// </summary>
        public float moveSpeedMultiplier { get => _moveSpeedMultiplier; set => _moveSpeedMultiplier = Mathf.Max(0, value); }
        [Tooltip("The Movement Speed multiplier for this variant")]
        [SerializeField, Min(0)]
        private float _moveSpeedMultiplier = 1;

        /// <summary>
        /// Attack Speed multiplier for this variant
        /// </summary>
        public float attackSpeedMultiplier { get => _attackSpeedMultiplier; set => _attackSpeedMultiplier = Mathf.Max(value, 0); }
        [Tooltip("The Attack Speed multiplier for this variant")]
        [SerializeField, Min(0)]
        private float _attackSpeedMultiplier = 1;

        /// <summary>
        /// Damage multiplier for this variant
        /// </summary>
        public float damageMultiplier { get => _damageMultiplier; set => _damageMultiplier = Mathf.Max(0, value); }
        [Tooltip("The Damage multiplier for this variant")]
        [SerializeField, Min(0)]
        private float _damageMultiplier = 1;

        [Tooltip("Flat bonus to the variant's armor, if negative it'll reduce it's armor")]
        public float armorBonus = 0;

        /// <summary>
        /// Armor multiplier for this variant
        /// </summary>
        public float armorMultiplier { get => _armorMultiplier; set => _armorMultiplier = Mathf.Max(0, value); }
        [Tooltip("The Armor multiplier for this variant")]
        [SerializeField, Min(0)]
        private float _armorMultiplier = 1;

        /// <summary>
        /// Applies the stat modifiers in this to <paramref name="targetBody"/>. Should not be called directly
        /// </summary>
        public virtual void ApplyStatModifiers(R2API.RecalculateStatsAPI.StatHookEventArgs args, CharacterBody targetBody)
        {
            args.healthMultAdd += _healthMultiplier;
            args.baseRegenAdd += regenBonus;
            args.regenMultAdd += _regenMultiplier;
            args.baseShieldAdd += shieldBonus;
            args.shieldMultAdd += _shieldMultiplier;
            args.moveSpeedMultAdd += _moveSpeedMultiplier;
            args.attackSpeedMultAdd += _attackSpeedMultiplier;
            args.damageMultAdd += _damageMultiplier;
            args.armorAdd += armorBonus;
            args.armorTotalMult += _armorMultiplier; 
        }

        /// <summary>
        /// Returns a clone of this as an instance of <see cref="BasicStatModifier"/>.
        /// <br></br>
        /// Override this method if you're inheriting from this class
        /// </summary>
        public virtual object Clone()
        {
            return new BasicStatModifier
            {
                healthMultiplier = healthMultiplier,
                armorBonus = armorBonus,
                armorMultiplier = armorMultiplier,
                attackSpeedMultiplier = attackSpeedMultiplier,
                damageMultiplier = damageMultiplier,
                moveSpeedMultiplier = moveSpeedMultiplier,
                regenBonus = regenBonus,
                regenMultiplier = regenMultiplier,
                shieldBonus = shieldBonus,
                shieldMultiplier = shieldMultiplier,
            };
        }

        /// <summary>
        /// Validate method
        /// </summary>
        public virtual void Validate() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public BasicStatModifier(float healthMultiplier = 1, float regenBonus = 0, float regenMultiplier = 1, float shieldBonus = 0, float shieldMultiplier = 1, float moveSpeedMultiplier = 1, float attackSpeedMultiplier = 1, float damageMultiplier = 1, float armorBonus = 0, float armorMultiplier = 1)
        {
            this.healthMultiplier = healthMultiplier;
            this.regenBonus = regenBonus;
            this.regenMultiplier = regenMultiplier;
            this.shieldBonus = shieldBonus;
            this.shieldMultiplier = shieldMultiplier;
            this.moveSpeedMultiplier = moveSpeedMultiplier;
            this.attackSpeedMultiplier = attackSpeedMultiplier;
            this.damageMultiplier = damageMultiplier;
            this.armorBonus = armorBonus;
            this.armorMultiplier = armorMultiplier;
        }

        /// <summary>
        /// Parameterless Constructor
        /// </summary>
        public BasicStatModifier() { }
    }
}