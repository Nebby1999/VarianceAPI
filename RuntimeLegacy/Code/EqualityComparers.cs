using System.Collections.Generic;

namespace VAPI.Legacy
{
    public class VariantDefIndexComparer : IEqualityComparer<VariantDef>
    {

        public bool Equals(VariantDef x, VariantDef y)
        {
            var xIndex = x ? x.variantIndex : VariantIndex.None;
            var yIndex = y ? y.variantIndex : VariantIndex.None;

            return xIndex == yIndex;
        }

        public int GetHashCode(VariantDef obj)
        {
            if (!obj)
                return -1;

            return obj.variantIndex.GetHashCode();
        }
    }
}