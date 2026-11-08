#nullable enable
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// Runtime Monobehaviour used to fire equipments from variants, you do not need to add this component manually.
    /// </summary>
    [AddComponentMenu("")]
    public sealed class VariantEquipmentHandler : MonoBehaviour
    {
        /// <summary>
        /// The time between attempts of firing the equipment
        /// </summary>
        public const float TIME_BETWEEN_ATTEMPTS = 0.25f;

        private struct EquipmentData
        {
            public EquipmentIndex requiredIndex;
            public float aiMaxUseHealthFraction;
            public float aiMaxUseDistance;
        }

        /// <summary>
        /// The CharacterMaster that's firing the equipment
        /// </summary>
        public CharacterMaster? characterMaster { get; private set; }

        /// <summary>
        /// The <see cref="characterMaster"/>'s inventory
        /// </summary>
#pragma warning disable CS8618
        public Inventory inventory { get; private set; }
#pragma warning restore CS8618

        private EquipmentData? _assignedData = null;
        private Run.FixedTimeStamp _attemptFireTimeStamp = default;

        /// <summary>
        /// Adds a new equipment data to this behaviour
        /// </summary>
        /// <param name="equipmentIndex">The equipment we want to fire</param>
        /// <param name="aiMaxUseHealthFraction">The AI's health must be lower than this fraction for the equipment to fire</param>
        /// <param name="aiMaxUseDistance">The AI's distance to it's target must be lower than this value for the equipment to fire</param>
        /// <param name="timeBetweenEquipmentSwitches">The time between equipment switches</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Call SetEquipmentData instead")]
        public void AddEquipmentData(EquipmentIndex equipmentIndex, float aiMaxUseHealthFraction, float aiMaxUseDistance, float timeBetweenEquipmentSwitches)
        {
            SetEquipmentData(equipmentIndex, aiMaxUseHealthFraction, aiMaxUseDistance);
        }

        /// <summary>
        /// Sets the equipment data to this behaviour
        /// </summary>
        /// <param name="equipmentIndex"></param>
        /// <param name="forceOverride"></param>
        /// <param name="aiMaxUseHealthFraction"></param>
        /// <param name="aiMaxUseDistance"></param>
        public void SetEquipmentData(EquipmentIndex equipmentIndex, float aiMaxUseHealthFraction, float aiMaxUseDistance)
        {
            if(_assignedData == null)
            {
                _assignedData = new EquipmentData
                {
                    aiMaxUseDistance = aiMaxUseDistance,
                    aiMaxUseHealthFraction = aiMaxUseHealthFraction,
                    requiredIndex = equipmentIndex
                };
            }
        }

        /// <summary>
        /// Removes the equipment data associated with <paramref name="index"/>
        /// </summary>
        public void RemoveEquipmentData(EquipmentIndex index)
        {
            if(_assignedData != null && _assignedData.Value.requiredIndex == index)
            {
                _assignedData = null;
            }
        }

        private void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
            inventory = GetComponent<Inventory>();
        }

        private void Start()
        {
            _attemptFireTimeStamp = Run.FixedTimeStamp.now;
        }

        private void FixedUpdate()
        {
            if (!characterMaster || !characterMaster!.GetBody())
            {
                return;
            }

            //Attempt to fire equipment using current data.
            if (_attemptFireTimeStamp.hasPassed)
            {
                _attemptFireTimeStamp = Run.FixedTimeStamp.now + TIME_BETWEEN_ATTEMPTS;
                AttemptToFireEquipment();
            }
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
            foreach (BaseAI aiComponent in aiComponents)
            {
                if (aiComponent.currentEnemy.bestHurtBox && Vector3.Distance(body.corePosition, aiComponent.currentEnemy.bestHurtBox.transform.position) > maxUseDistance)
                {
                    return;
                }
            }

            float maxUseHealthFraction = GetAIMaxUseHealthFraction();
            if (body.healthComponent && body.healthComponent.combinedHealthFraction > maxUseHealthFraction)
                return;

            body.inputBank.activateEquipment.PushState(true);
        }


        private float GetAIMaxUseHealthFraction()
        {
            return _assignedData?.aiMaxUseHealthFraction ?? float.PositiveInfinity;
        }

        private float GetAiMaxUseDistance()
        {
            return _assignedData?.aiMaxUseDistance ?? float.PositiveInfinity;
        }
    }
}