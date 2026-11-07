using KinematicCharacterController;
using System;
using UnityEngine;

namespace VAPI.Legacy
{
    [Obsolete("VarianceAPI's 2.0 systems are obsolete, Modifying the scale of a character is now done in the CharacterVariantDef itself")]
    public class VariantSizeModifier : ScriptableObject
    {
        [Min(0)]
        public float sizeCoefficient;

        public bool forFlyingBody;
    }
}
