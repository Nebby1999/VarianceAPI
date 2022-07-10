using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VAPI.Components
{
    public class VariantEquipmentHandler : MonoBehaviour
    {
        public CharacterBody body;
        public float aiMaxUseHealthFraction;
        public float aiMaxUseDistance;
        public float aiUseDelay;
        public float aiUseDelayMax;
        private bool aiCanUse = false;

        private void FixedUpdate()
        {
            aiUseDelay -= Time.fixedDeltaTime;
            if(aiUseDelay <= 0)
            {
                aiCanUse = true;
                aiUseDelay = aiUseDelayMax;
            }

            if(body.equipmentSlot && body.equipmentSlot.stock > 0 && body.inputBank && !body.isPlayerControlled)
            {
                if(aiCanUse)
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
