using System.Collections.Generic;

namespace VAPI
{
    public struct VariantDefIndexComparer : IEqualityComparer<VariantDef>
    {
        public bool Equals(VariantDef x, VariantDef y)
        {
            var xIndex = x ? x.variantIndex : VariantIndex.None;
            var yIndex = y ? y.variantIndex : VariantIndex.None;

            return xIndex == yIndex;
        }

        public int GetHashCode(VariantDef obj)
        {
            return obj.variantIndex.GetHashCode();
        }
    }
}