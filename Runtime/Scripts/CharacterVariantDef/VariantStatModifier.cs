#nullable enable

using R2API;
using RoR2;
using System;
using UnityEngine;

namespace VAPI
{
    public interface IVariantStatModifier : IValidatable, ICloneable
    {
        public void ApplyStatModifiers(R2API.RecalculateStatsAPI.StatHookEventArgs args, CharacterBody targetBody);
    }

    [Serializable]
    public class BasicStatModifier : IVariantStatModifier
    {
        public float healthMultiplier { get => _healthMultiplier; set => _healthMultiplier = Mathf.Max(0, value); }
        [SerializeField, Min(0)]
        private float _healthMultiplier = 1;

        public float regenBonus = 0;

        public float regenMultiplier { get => _regenMultiplier; set => _regenMultiplier = Mathf.Max(0, value); }
        [SerializeField, Min(0)]
        private float _regenMultiplier = 0;

        public float shieldBonus = 0;

        public float shieldMultiplier { get => _shieldMultiplier; set => _shieldMultiplier = Mathf.Max(0, value); }
        [SerializeField, Min(0)]
        private float _shieldMultiplier = 1;

        public float moveSpeedMultiplier { get => _moveSpeedMultiplier; set => _moveSpeedMultiplier = Mathf.Max(0, value); }
        [SerializeField, Min(0)]
        private float _moveSpeedMultiplier = 1;

        public float attackSpeedMultiplier { get => _attackSpeedMultiplier; set => _attackSpeedMultiplier = Mathf.Max(value, 0); }
        [SerializeField, Min(0)]
        private float _attackSpeedMultiplier = 1;

        public float damageMultiplier { get => _damageMultiplier; set => _damageMultiplier = Mathf.Max(0, value); }
        [SerializeField, Min(0)]
        private float _damageMultiplier = 1;

        public float armorBonus = 0;

        public float armorMultiplier { get => _armorMultiplier; set => _armorMultiplier = Mathf.Max(0, value); }
        [SerializeField, Min(0)]
        private float _armorMultiplier = 1;

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

        public object Clone()
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

        public virtual void Validate() { }

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

        public BasicStatModifier() { }
    }
}