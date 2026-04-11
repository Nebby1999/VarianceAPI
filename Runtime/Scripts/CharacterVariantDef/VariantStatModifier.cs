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
        [Min(0)]
        public float healthMultiplier = 1;

        public float regenBonus = 0;

        [Min(0)]
        public float regenMultiplier = 0;

        public float shieldBonus = 0;

        [Min(0)]
        public float shieldMultiplier = 1;

        [Min(0)]
        public float moveSpeedMultiplier = 1;

        [Min(0)]
        public float attackSpeedMultiplier = 1;

        [Min(0)]
        public float damageMultiplier = 1;

        public float armorBonus = 0;

        [Min(0)]
        public float armorMultiplier = 1;

        public virtual void ApplyStatModifiers(R2API.RecalculateStatsAPI.StatHookEventArgs args, CharacterBody targetBody)
        {
            args.healthMultAdd += healthMultiplier;
            args.baseRegenAdd += regenBonus;
            args.regenMultAdd += regenMultiplier;
            args.baseShieldAdd += shieldBonus;
            args.shieldMultAdd += shieldMultiplier;
            args.moveSpeedMultAdd += moveSpeedMultiplier;
            args.attackSpeedMultAdd += attackSpeedMultiplier;
            args.damageMultAdd += damageMultiplier;
            args.armorAdd += armorBonus;
            args.armorTotalMult += armorMultiplier; 
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
    }
}