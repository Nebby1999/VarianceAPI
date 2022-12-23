using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPI.RuleSystem
{
    public class VAPIRuleChoiceDef : RuleChoiceDef
    {
        public VariantPackDef requiredVariantPack;
        public VariantIndex variantIndex = VariantIndex.None;


    }
}
