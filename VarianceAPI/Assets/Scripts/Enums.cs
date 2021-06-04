using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI
{
    /// <summary>
    /// The Variant tier is the system that the mod uses for easily identifying variants and handling certain processes, such as the rewards.
    /// <para>The higher the inheritance, the more effects it has</para>
    /// <para>For example, a Legendary variant makes a spawn sound, has red health and annoucnes their arrival on the chat</para>
    /// </summary>
    public enum VariantTier
    {
        /// <summary>
        /// Common Variants dont have anything special to separate them from regular entities or when they spawn
        /// </summary>
        Common,

        /// <summary>
        /// Uncommon Variants have their health bars tinted Red
        /// </summary>
        Uncommon,
        
        /// <summary>
        /// Rare variants have a spawn sound
        /// </summary>
        Rare,

        /// <summary>
        /// Legendary variants announce their arrival in the chat.
        /// </summary>
        Legendary
    }
    /// <summary>
    /// AI Modifiers, these modifiers change the behavior of variants
    /// </summary>
    public enum VariantAIModifier
    {
        /// <summary>
        /// Default modifier causes no changes
        /// </summary>
        Default,

        /// <summary>
        /// Unstable causes the variant to use their Desperation attack whenever they can. 
        /// </summary>
        Unstable,

        /// <summary>
        /// ForceSprint causes the variant to always sprint
        /// </summary>
        ForceSprint
    };

    public enum OverrideNameOrder
    {
        Preffix,
        Suffix
    }
}
