using RoR2;
using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace VAPI.Legacy.Components
{
    [Obsolete("VAPI 2.0's systems are obsolete, utilize \"VAPI.VariantComponent\" instead")]
    public abstract class VariantComponent : MonoBehaviour
    {
        public ReadOnlyCollection<VariantDef> variantDefs { get; internal set; }

        public CharacterBody characterBody { get; internal set; }

        public CharacterMaster characterMaster { get; internal set; }

        public CharacterModel characterModel { get; internal set; }
    }
}
