using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantExtraComponent", menuName = "VarianceAPI/VariantExtraComponent", order = 11)]
    public class VariantExtraComponent : ScriptableObject
    {
        [Header("Variant Extra Component")]
            [Tooltip("What component to add to the Variant")]
            public UnityEngine.Object componentToAdd;

            [Tooltip("Wether the component is Aesthetic, such as adding a missile launcher to an enemy")]
            public bool isAesthetic;
    }
}
