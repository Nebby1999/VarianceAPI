using R2API;

namespace VAPI.RuleSystem
{
    /// <summary>
    /// <inheritdoc cref="ExtendedRuleChoiceDef"/>
    /// A VAPIRuleChoiceDef also specifies the ChoiceDef's variant index
    /// </summary>
    public class VAPIRuleChoiceDef : ExtendedRuleChoiceDef
    {
        public VariantIndex variantIndex = VariantIndex.None;
    }
}
