#nullable enable
using System;
using UnityEngine;

namespace VAPI
{
    public abstract class VariantComponent : MonoBehaviour
    {
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

            public bool forServer { get; set; }
            public bool forClient { get; set; }

            public TargetComponentObjectAttribute(TargetObject targetObject)
            {
                this.targetObject = targetObject;
            }
        }
    }
}