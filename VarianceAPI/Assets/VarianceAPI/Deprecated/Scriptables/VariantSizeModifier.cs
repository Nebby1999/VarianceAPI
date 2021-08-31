﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "SizeModifier", menuName = "VAPI_OLD/SizeModifier")]
    public class VariantSizeModifier : ScriptableObject
    {
        [Header("Variant Size Modifier")]
            [Tooltip("The new size of the Variant\nWhere 1.0 = 100% Base Size")]
            [Min(0)]
            public float newSize;

            [Tooltip("Wether the SizeModifier changes collider Size\nReccomended for Flying Variants Only.")]
            public bool scaleCollider;
    }
}