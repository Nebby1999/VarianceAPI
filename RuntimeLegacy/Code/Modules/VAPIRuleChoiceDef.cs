using R2API;
using RoR2;

namespace VAPI.RuleSystem
{
    /// <summary>
    /// <inheritdoc cref="ExtendedRuleChoiceDef"/>
    /// A VAPIRuleChoiceDef also specifies the ChoiceDef's variant index
    /// </summary>
    public class VAPIRuleChoiceDef : ExtendedRuleChoiceDef
    {
        /// <summary>
        /// The VariantIndex for this rule choice def, this might not always be a valid value.
        /// </summary>
        public VariantIndex variantIndex = VariantIndex.None;
        /// <summary>
        /// The VariantPackIndex for this rule choice def, this might not always be a valid value.
        /// </summary>
        public VariantPackIndex variantPackIndex = VariantPackIndex.None;
        /// <summary>
        /// If <see cref="variantIndex"/> is not <see cref="VariantIndex.None"/>, this will have the RuleChoiceDef for the VariantPack that's tied to the VariantIndex.
        /// </summary>
        public RuleChoiceDef tiedPackEnabledChoice;
    }
}
