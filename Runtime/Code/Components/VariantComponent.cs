using RoR2;
using System.Collections.ObjectModel;
using UnityEngine;

namespace VAPI.Components
{
    /// <summary>
    /// The base for VariantComponents
    /// </summary>
    public abstract class VariantComponent : MonoBehaviour
    {
        /// <summary>
        /// The Variant's VariantDefs
        /// </summary>
        public ReadOnlyCollection<VariantDef> VariantDefs { get; internal set; }
        /// <summary>
        /// The Variant's CharacterBody
        /// </summary>
        public CharacterBody CharacterBody { get; internal set; }
        /// <summary>
        /// The Variant's CharacterMaster
        /// </summary>
        public CharacterMaster CharacterMaster { get; internal set; }
        /// <summary>
        /// The Variant's CharacterModel
        /// </summary>
        public CharacterModel CharacterModel { get; internal set; }
    }
}
