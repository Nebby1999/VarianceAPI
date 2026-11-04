#nullable enable
using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// Base class for Components that are applied to Variants.
    /// <para></para>
    /// The <see cref="CharacterVariantDef"/> has an array of this class in the form of the <see cref="VariantComponentCollection"/> class. When said variant def is applied, any stored components will also be applied to the character.
    /// <para></para>
    /// VarianceAPI supports specifying at what object this component needs to be added via the <see cref="TargetComponentObjectAttribute"/>. This attribute is necessary for VAPI to be able to automatically add your component to a character.
    /// </summary>
    public abstract class VariantComponent : MonoBehaviour
    {
        /// <summary>
        /// Attribute that defines the target object for this component
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
        public sealed class TargetComponentObjectAttribute : Attribute
        {
            /// <summary>
            /// Represents valid objects for this component to be added to
            /// </summary>
            public enum TargetObject
            {
                /// <summary>
                /// The component will be added to the CharacterBody object
                /// </summary>
                CharacterBody,
                /// <summary>
                /// The component will be added to the CharacterMaster object
                /// </summary>
                CharacterMaster,
                /// <summary>
                /// The component will be added to the CharacterModel object
                /// </summary>
                CharacterModel
            }

            /// <summary>
            /// The target object for this component
            /// </summary>
            public TargetObject targetObject { get; set; }

            /// <summary>
            /// If true, this component will be added on the Server.
            /// <para></para>
            /// <b>Defaults to False</b>
            /// </summary>
            public bool useOnServer { get; set; }

            /// <summary>
            /// If true, this component will be added on the Client.
            /// <para></para>
            /// <b>Defaults to False</b>
            /// </summary>
            public bool useOnClient { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public TargetComponentObjectAttribute(TargetObject targetObject)
            {
                this.targetObject = targetObject;
            }
        }
    }
}