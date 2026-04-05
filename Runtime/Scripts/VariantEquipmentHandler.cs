#nullable enable
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    public class VariantEquipmentHandler : MonoBehaviour
    {
        public const float TIME_BETWEEN_ATTEMPTS = 0.25f;
        public struct EquipmentData
        {
            public EquipmentIndex requiredIndex;
            public float aiMaxUseHealthFraction;
            public float aiMaxUseDistance;
            public float timeUntilEquipmentSwitch;
        }
        public CharacterMaster? characterMaster { get; private set; }
        public Inventory inventory { get; private set; }

        public List<EquipmentData> equipmentDatas = new List<EquipmentData>();
        private int currentDataIndex = -1;
        private Run.FixedTimeStamp attemptFireTimeStamp = default;
        private Run.FixedTimeStamp switchTimeStamp;

        public void AddEquipmentData(EquipmentIndex equipmentIndex, float aiMaxUseHealthFraction, float aiMaxUseDistance, float timeBetweenEquipmentSwitches)
        {
            foreach (var data in equipmentDatas)
            {
                if (data.requiredIndex == equipmentIndex)
                    return;
            }

            equipmentDatas.Add(new EquipmentData
            {
                aiMaxUseDistance = aiMaxUseDistance,
                aiMaxUseHealthFraction = aiMaxUseHealthFraction,
                requiredIndex = equipmentIndex,
                timeUntilEquipmentSwitch = timeBetweenEquipmentSwitches
            });
            currentDataIndex = -1;
        }

        public void RemoveEquipmentData(EquipmentIndex index)
        {
            for (int i = equipmentDatas.Count - 1; i >= 0; i--)
            {
                if (equipmentDatas[i].requiredIndex == index)
                {
                    equipmentDatas.RemoveAt(i);
                    return;
                }
            }
            currentDataIndex = -1;
        }

        private void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
            inventory = GetComponent<Inventory>();
        }

        private void Start()
        {
            switchTimeStamp = Run.FixedTimeStamp.now;
            attemptFireTimeStamp = Run.FixedTimeStamp.now;
        }

        private void FixedUpdate()
        {
            if (!characterMaster || !characterMaster!.GetBody())
            {
                return;
            }

            //Switch equipment
            if(switchTimeStamp.hasPassed)
            {
                SwitchEquipment();
                switchTimeStamp = Run.FixedTimeStamp.now + GetTimeUntilEquipmentSwitch();
            }

            //Attempt to fire equipment using current data.
            if (attemptFireTimeStamp.hasPassed)
            {
                attemptFireTimeStamp = Run.FixedTimeStamp.now + TIME_BETWEEN_ATTEMPTS;
                AttemptToFireEquipment();
            }
        }

        private void SwitchEquipment()
        {
            inventory.SwitchToNextEquipmentInSet();
            for (int i = 0; i < equipmentDatas.Count; i++)
            {
                if (equipmentDatas[i].requiredIndex == inventory.currentEquipmentIndex)
                {
                    currentDataIndex = i;
                    return;
                }
            }
            currentDataIndex = -1;
        }

        private void AttemptToFireEquipment()
        {
            CharacterBody body = characterMaster!.GetBody();

            if (!body.inputBank)
                return;

            float maxUseDistance = GetAiMaxUseDistance();
            if (maxUseDistance <= 0f)
                return;

            BaseAI[] aiComponents = body.master.aiComponents;
            foreach(BaseAI aiComponent in aiComponents)
            {
                if(aiComponent.currentEnemy.bestHurtBox && Vector3.Distance(body.corePosition, aiComponent.currentEnemy.bestHurtBox.transform.position) > maxUseDistance)
                {
                    return;
                }
            }

            float maxUseHealthFraction = GetAIMaxUseHealthFraction();
            if (body.healthComponent && body.healthComponent.combinedHealthFraction > maxUseHealthFraction)
                return;

            body.inputBank.activateEquipment.PushState(true);
        }

        private float GetTimeUntilEquipmentSwitch()
        {
            if(HG.ListUtils.IsInBounds(equipmentDatas, currentDataIndex))
            {
                return equipmentDatas[currentDataIndex].timeUntilEquipmentSwitch;
            }
            return 0;
        }

        private float GetAIMaxUseHealthFraction()
        {
            if(HG.ListUtils.IsInBounds(equipmentDatas, currentDataIndex))
            {
                return equipmentDatas[currentDataIndex].aiMaxUseHealthFraction;
            }
            return float.PositiveInfinity;
        }

        private float GetAiMaxUseDistance()
        {
            if(HG.ListUtils.IsInBounds(equipmentDatas, currentDataIndex))
            {
                return equipmentDatas[currentDataIndex].aiMaxUseDistance;
            }
            return float.PositiveInfinity;
        }
    }
}