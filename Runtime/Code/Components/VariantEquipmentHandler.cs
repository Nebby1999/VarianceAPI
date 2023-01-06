using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace VAPI.Components
{
    /// <summary>
    /// A Component that allows a variant to use an Equipment, the arguments are taken from the VariantDef's <see cref="VariantDef.variantInventory"/>'s <see cref="VariantInventory.equipmentInfo"/>
    /// </summary>
    public class VariantEquipmentHandler : MonoBehaviour
    {
        /// <summary>
        /// The tied body
        /// </summary>
        public CharacterBody body;
        /// <summary>
        /// The fraction of health that must be missing for this variant to attempt to use the equipment
        /// </summary>
        public float aiMaxUseHealthFraction;
        /// <summary>
        /// The distance required between variant and target for this variant to attempt to use the equipment
        /// </summary>
        public float aiMaxUseDistance;
        /// <summary>
        /// the current timer to determine wether the variant can use the equipment
        /// </summary>
        public float aiUseDelay;
        public float aiUseDelayMax;
        private bool aiCanUse = false;

        private void FixedUpdate()
        {
            aiUseDelay -= Time.fixedDeltaTime;
            if (aiUseDelay <= 0)
            {
                aiCanUse = true;
                aiUseDelay = aiUseDelayMax;
            }

            if (body.equipmentSlot && body.equipmentSlot.stock > 0 && body.inputBank && !body.isPlayerControlled)
            {
                if (aiCanUse)
                {
                    aiCanUse = false;

                    EntityStateMachine[] stateMachines = body.gameObject.GetComponents<EntityStateMachine>();
                    foreach (EntityStateMachine stateMachine in stateMachines)
                    {
                        if (stateMachine.initialStateType.stateType.IsInstanceOfType(stateMachine.state) && stateMachine.initialStateType.stateType != stateMachine.mainStateType.stateType)
                        {
                            return;
                        }
                    }

                    if (aiMaxUseDistance <= 0f) return;
                    if (aiMaxUseDistance != Mathf.Infinity && body.master)
                    {
                        BaseAI[] aiComponents = body.master.aiComponents;
                        foreach (BaseAI ai in aiComponents)
                        {
                            if (ai.currentEnemy.bestHurtBox && Vector3.Distance(body.corePosition, ai.currentEnemy.bestHurtBox.transform.position) > aiMaxUseDistance)
                            {
                                return;
                            }
                        }
                    }

                    if (body.healthComponent && body.healthComponent.combinedHealthFraction > aiMaxUseHealthFraction) return;

                    body.inputBank.activateEquipment.PushState(true);
                }
            }
        }
    }
}
