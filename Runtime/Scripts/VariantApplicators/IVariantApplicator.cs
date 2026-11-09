using HG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    public interface IVariantBodyApplicator
    {
        public void ApplyVariants(CharacterBodyVariantController src, ReadOnlyArray<CharacterVariantDef> defs);

        public void UnapplyVariants(CharacterBodyVariantController src);
    }

    public interface IVariantMasterApplicator
    {
        public void ApplyVariants(CharacterMasterVariantStorage src, ReadOnlyArray<CharacterVariantDef> defs);

        public void UnapplyVariants(CharacterMasterVariantStorage src, bool bodyIndexChanged);
    }
}