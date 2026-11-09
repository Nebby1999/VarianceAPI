using HG;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// An <see cref="IVariantBodyApplicator"/> is an interface used to handle the application and unapplication of <see cref="CharacterVariantDef"/>s for a Body.
    /// <br></br>
    /// This is useful on scenarios where certain modifiers from <see cref="CharacterVariantDef"/>s may "ruin" fighting the character.
    /// <para></para>
    /// The default Body Applicatior for all variants is the <see cref="DefaultVariantBodyApplicator"/>, however, a custom applicator for a Body can be specified by implementing the interface on a component and adding said component to the CharacterBody Prefab. When writing custom applicators it is recommended to create an instance of <see cref="DefaultVariantBodyApplicator.MethodImpls"/>, as said class contains the default methods for applying modifiers.
    /// <br></br>
    /// An example of this is the <see cref="MimicVariantBodyApplicator"/>, a custom applicator of <see cref="CharacterVariantDef"/> that applies and unapplies specific modifiers based on wether the Mimic is disguised or not.
    /// </summary>
    public interface IVariantBodyApplicator
    {
        /// <summary>
        /// Apply the <paramref name="characterVariantDefs"/> to the <see cref="CharacterBody"/> from <paramref name="src"/>.
        /// </summary>
        /// <param name="src">The <see cref="CharacterBodyVariantController"/> that called this method</param>
        /// <param name="characterVariantDefs">The <see cref="CharacterVariantDef"/>s to apply.</param>
        public void ApplyVariants(CharacterBodyVariantController src, ReadOnlyArray<CharacterVariantDef> characterVariantDefs);

        /// <summary>
        /// Unapply the <see cref="CharacterVariantDef"/>s applied to the <see cref="CharacterBody"/> from <paramref name="src"/>.
        /// </summary>
        /// <param name="src">The <see cref="CharacterBodyVariantController"/> that called this method</param>
        public void UnapplyVariants(CharacterBodyVariantController src);
    }

    /// <summary>
    /// An <see cref="IVariantMasterApplicator"/> is an interface used to handle the application and unapplication of <see cref="CharacterVariantDef"/>s for a Master
    /// <br></br>
    /// This is useful on scenarios where certain modifiers from <see cref="CharacterVariantDef"/>s may "ruin" fighting the character.
    /// <para></para>
    /// The default Master Applicator for all variants is the <see cref="DefaultVariantMasterApplicator"/>, however, a custom applicator for a Body can be specified by implementing the interface on a component, and adding said component to the CharacterMaster Prefab
    /// </summary>
    public interface IVariantMasterApplicator
    {
        /// <summary>
        /// Apply the <paramref name="characterVariantDefs"/> to the <see cref="CharacterMaster"/> from <paramref name="src"/>
        /// </summary>
        /// <param name="src">The <see cref="CharacterMasterVariantStorage"/> that called this method</param>
        /// <param name="characterVariantDefs">The <see cref="CharacterVariantDef"/>s to apply.</param>
        public void ApplyVariants(CharacterMasterVariantStorage src, ReadOnlyArray<CharacterVariantDef> characterVariantDefs);

        /// <summary>
        /// Unapply the <see cref="CharacterVariantDef"/>s applied to the <see cref="CharacterMaster"/> from <paramref name="src"/>.
        /// <para></para>
        /// In scenarios where <paramref name="bodyIndexChanged"/> is true, it is <b>Heavily Recommended</b> to dispose any and all master modifiers.
        /// </summary>
        /// <param name="src">The <see cref="CharacterMasterVariantStorage"/> that called this method</param>
        /// <param name="bodyIndexChanged">If true, the <see cref="CharacterBody"/> for the CharacterMaster changed, in scenarios where this is true it is <b>heavily recommended</b> to dispose any and all master modifiers.</param>
        public void UnapplyVariants(CharacterMasterVariantStorage src, bool bodyIndexChanged);
    }
}