#nullable enable
using System;
using UnityEngine;

namespace VAPI
{
    public abstract class VariantComponent : MonoBehaviour
    {
        internal static GameObject? earlyAssignmentBody;
        internal static GameObject? earlyAssignmentMaster;
        internal static GameObject? earlyAssignmentModel;

        public GameObject? bodyObject { get; private set; }
        public GameObject? masterObject { get; private set; }
        public GameObject? modelObject { get; private set; }

        protected virtual void Awake()
        {
            bodyObject = earlyAssignmentBody;
            masterObject = earlyAssignmentMaster;
            modelObject = earlyAssignmentModel;

            earlyAssignmentBody = null;
            earlyAssignmentMaster = null;
            earlyAssignmentModel = null;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TargetComponentObjectAttribute : Attribute
    {
        public enum TargetObject
        {
            CharacterBody,
            CharacterMaster,
            CharacterModel
        }

        public TargetObject targetObject { get; set; }

        public TargetComponentObjectAttribute(TargetObject targetObject)
        {
            this.targetObject = targetObject;
        }
    }
}