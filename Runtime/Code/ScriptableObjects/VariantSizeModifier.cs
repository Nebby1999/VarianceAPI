using KinematicCharacterController;
using UnityEngine;

namespace VAPI
{
    [CreateAssetMenu(fileName = "VariantSizeModifier", menuName = "VarianceAPI/VariantSizeModifier")]
    public class VariantSizeModifier : ScriptableObject
    {
        [Header("Variant Size Modifier")]
        [Tooltip("The new size of the Variant\nWhere 1.0 = 100% Base Size")]
        [Min(0)]
        public float sizeCoefficient;

        public bool forFlyingBody;

        public void ApplySize(Transform mdlTransform, KinematicCharacterMotor[] kinematicCharacterMotors)
        {
            mdlTransform.localScale *= sizeCoefficient;
            if(forFlyingBody)
            {
                foreach(KinematicCharacterMotor motor in kinematicCharacterMotors)
                {
                    if(motor)
                    {
                        motor.SetCapsuleDimensions(motor.Capsule.radius * sizeCoefficient, motor.CapsuleHeight * sizeCoefficient, sizeCoefficient);
                    }
                }
            }
        }
    }
}
