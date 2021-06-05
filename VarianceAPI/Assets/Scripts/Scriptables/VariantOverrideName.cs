using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantOverrideName", menuName = "VarianceAPI/VariantOverrideName", order = 1)]
    public class VariantOverrideName : ScriptableObject
    {
        [Header("Variant Override Name")]
            [Tooltip("Override order of this override Name\nPrefix: The text to add is applied before the Variant's original name\nSuffix: The text to add is applied after the Variant's original name")]
            public OverrideNameType overrideOrder;

            [Tooltip("The text to add to the variant's original name")]
            public string textToAdd;
    }
}
