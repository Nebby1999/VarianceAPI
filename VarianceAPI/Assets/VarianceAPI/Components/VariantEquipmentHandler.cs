/*
 * Code taken from MysticSword's Aspect Abilities mod.
 */

using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VarianceAPI.Scriptables;

namespace VarianceAPI.Components
{
    public class VariantEquipmentHandler : NetworkBehaviour
    {
        public EquipmentInfo equipmentInfo;
        public float aiUseDelay = 1f;
        public float aiUseDelayMax = 1f;
        public float aiMaxDistance = 60;
        public bool aiCanUse = false;
        public void Awake()
        {
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
        }

        public void FixedUpdate()
        {
            aiUseDelay -= Time.fixedDeltaTime;
            if(aiUseDelay <= 0f)
            {
                aiCanUse = true;
                aiUseDelay = aiUseDelayMax;
            }
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, RoR2.CharacterBody self)
        {
            if(self.equipmentSlot && self.equipmentSlot.stock > 0 && self.inputBank && !self.isPlayerControlled)
            {
                if(aiCanUse)
                {
                    aiCanUse = false;
                    bool spawning = false;
                    EntityStateMachine[] stateMachines = self.gameObject.GetComponents<EntityStateMachine>();
                    foreach (EntityStateMachine stateMachine in stateMachines)
                    {
                        if(stateMachine.initialStateType.stateType.IsInstanceOfType(stateMachine.state) && stateMachine.initialStateType.stateType != stateMachine.mainStateType.stateType)
                        {
                            spawning = true;
                            break;
                        }
                    }

                    bool enemyNearby = false;
                    if(aiMaxDistance == Mathf.Infinity)
                    {
                        enemyNearby = true;
                    }
                    else if(aiMaxDistance <= 0)
                    {
                        enemyNearby = false;
                    }
                    else if(self.master)
                    {
                        BaseAI[] aiComponents = self.master.GetFieldValue<BaseAI[]>("aiComponents");
                        foreach (BaseAI ai in aiComponents)
                        {
                            if(ai.currentEnemy.bestHurtBox && Vector3.Distance(self.corePosition, ai.currentEnemy.bestHurtBox.transform.position) <= aiMaxDistance)
                            {
                                enemyNearby = true;
                            }
                        }
                    }

                    float randomChance = equipmentInfo.animationCurve.Evaluate(1f - (self.healthComponent ? self.healthComponent.combinedHealthFraction : 1f)) * 100f;
                    if(!spawning && Util.CheckRoll(randomChance) && enemyNearby)
                    {
                        self.inputBank.activateEquipment.PushState(true);
                    }
                }
            }
            orig(self);
        }
    }
}
