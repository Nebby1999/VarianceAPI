#nullable enable
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// Monobehaviour used to fire equipments from variants
    /// </summary>
    [AddComponentMenu("")]
    public sealed class VariantEquipmentHandler : MonoBehaviour
    {
        /// <summary>
        /// The time between attempts of firing the equipment
        /// </summary>
        public const float TIME_BETWEEN_ATTEMPTS = 0.25f;

        /// <summary>
        /// Data for the equipment
        /// </summary>
        public struct EquipmentData
        {
            public EquipmentIndex requiredIndex;
            public float aiMaxUseHealthFraction;
            public float aiMaxUseDistance;
            public float timeUntilEquipmentSwitch;
        }

        /// <summary>
        /// The CharacterMaster that's firing the equipment
        /// </summary>
        public CharacterMaster? characterMaster { get; private set; }

        /// <summary>
        /// The <see cref="characterMaster"/>'s inventory
        /// </summary>
        public Inventory inventory { get; private set; }

        private List<EquipmentData> _equipmentDatas = new List<EquipmentData>();
        private int _currentDataIndex = -1;
        private Run.FixedTimeStamp _attemptFireTimeStamp = default;
        private Run.FixedTimeStamp _switchTimeStamp;

        /// <summary>
        /// Adds a new equipment data to this behaviour
        /// </summary>
        /// <param name="equipmentIndex">The equipment we want to fire</param>
        /// <param name="aiMaxUseHealthFraction">The AI's health must be lower than this fraction for the equipment to fire</param>
        /// <param name="aiMaxUseDistance">The AI's distance to it's target must be lower than this value for the equipment to fire</param>
        /// <param name="timeBetweenEquipmentSwitches">The time between equipment switches</param>
        public void AddEquipmentData(EquipmentIndex equipmentIndex, float aiMaxUseHealthFraction, float aiMaxUseDistance, float timeBetweenEquipmentSwitches)
        {
            foreach (var data in _equipmentDatas)
            {
                if (data.requiredIndex == equipmentIndex)
                    return;
            }

            _equipmentDatas.Add(new EquipmentData
            {
                aiMaxUseDistance = aiMaxUseDistance,
                aiMaxUseHealthFraction = aiMaxUseHealthFraction,
                requiredIndex = equipmentIndex,
                timeUntilEquipmentSwitch = timeBetweenEquipmentSwitches
            });
            _currentDataIndex = -1;
        }

        /// <summary>
        /// Removes the equipment data associated with <paramref name="index"/>
        /// </summary>
        public void RemoveEquipmentData(EquipmentIndex index)
        {
            for (int i = _equipmentDatas.Count - 1; i >= 0; i--)
            {
                if (_equipmentDatas[i].requiredIndex == index)
                {
                    _equipmentDatas.RemoveAt(i);
                    _currentDataIndex = -1;
                    return;
                }
            }
        }

        private void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
            inventory = GetComponent<Inventory>();
        }

        private void Start()
        {
            _switchTimeStamp = Run.FixedTimeStamp.now;
            _attemptFireTimeStamp = Run.FixedTimeStamp.now;
        }

        private void FixedUpdate()
        {
            if (!characterMaster || !characterMaster!.GetBody())
            {
                return;
            }

            //Switch equipment
            if(_switchTimeStamp.hasPassed)
            {
                SwitchEquipment();
                _switchTimeStamp = Run.FixedTimeStamp.now + GetTimeUntilEquipmentSwitch();
            }

            //Attempt to fire equipment using current data.
            if (_attemptFireTimeStamp.hasPassed)
            {
                _attemptFireTimeStamp = Run.FixedTimeStamp.now + TIME_BETWEEN_ATTEMPTS;
                AttemptToFireEquipment();
            }
        }

        private void SwitchEquipment()
        {
            inventory.SwitchToNextEquipmentInSet();
            for (int i = 0; i < _equipmentDatas.Count; i++)
            {
                if (_equipmentDatas[i].requiredIndex == inventory.currentEquipmentIndex)
                {
                    _currentDataIndex = i;
                    return;
                }
            }
            _currentDataIndex = -1;
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
            if(HG.ListUtils.IsInBounds(_equipmentDatas, _currentDataIndex))
            {
                var data = _equipmentDatas[_currentDataIndex];
                if(Mathf.Approximately(data.timeUntilEquipmentSwitch, 0f))
                {
                    return float.PositiveInfinity;
                }
                return data.timeUntilEquipmentSwitch;
            }
            return 0;
        }

        private float GetAIMaxUseHealthFraction()
        {
            if(HG.ListUtils.IsInBounds(_equipmentDatas, _currentDataIndex))
            {
                return _equipmentDatas[_currentDataIndex].aiMaxUseHealthFraction;
            }
            return float.PositiveInfinity;
        }

        private float GetAiMaxUseDistance()
        {
            if(HG.ListUtils.IsInBounds(_equipmentDatas, _currentDataIndex))
            {
                return _equipmentDatas[_currentDataIndex].aiMaxUseDistance;
            }
            return float.PositiveInfinity;
        }
    }
}