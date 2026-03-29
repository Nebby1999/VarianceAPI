using KinematicCharacterController;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A ScriptableObject used to represent a Size Modifier
    /// </summary>
    [CreateAssetMenu(fileName = "VariantSizeModifier", menuName = "VarianceAPI/VariantSizeModifier")]
    public class VariantSizeModifier : ScriptableObject
    {
        [Header("Variant Size Modifier")]
        [Tooltip("The new size of the Variant\nWhere 1.0 = 100% Base Size")]
        [Min(0)]
        public float sizeCoefficient;

        [Tooltip("Wether this size modifier is for a Flying variant (IE: Wisps, Alloy Vultures, Blind Pests")]
        public bool forFlyingBody;

        /// <summary>
        /// Applies the size modifier to the model's transform
        /// </summary>
        /// <param name="mdlTransform">The model's transform</param>
        /// <param name="kinematicCharacterMotors">The character's KinematicCharacterMotors, only relevant if <see cref="forFlyingBody"/> is set to true</param>
        public void ApplySize(Transform mdlTransform, KinematicCharacterMotor[] kinematicCharacterMotors)
        {
            mdlTransform.localScale *= sizeCoefficient;
            if (forFlyingBody)
            {
                foreach (KinematicCharacterMotor motor in kinematicCharacterMotors)
                {
                    if (motor)
                    {
                        motor.SetCapsuleDimensions(motor.Capsule.radius * sizeCoefficient, motor.CapsuleHeight * sizeCoefficient, sizeCoefficient);
                    }
                }
            }
        }
    }
}
