using RoR2;
using System.Collections.ObjectModel;
using UnityEngine;

namespace VAPI.Legacy.Components
{
    /// <summary>
    /// The base for VariantComponents
    /// </summary>
    public abstract class VariantComponent : MonoBehaviour
    {
        /// <summary>
        /// The Variant's VariantDefs
        /// </summary>
        public ReadOnlyCollection<VariantDef> variantDefs { get; internal set; }
        /// <summary>
        /// The Variant's CharacterBody
        /// </summary>
        public CharacterBody characterBody { get; internal set; }
        /// <summary>
        /// The Variant's CharacterMaster
        /// </summary>
        public CharacterMaster characterMaster { get; internal set; }
        /// <summary>
        /// The Variant's CharacterModel
        /// </summary>
        public CharacterModel characterModel { get; internal set; }
    }
}
