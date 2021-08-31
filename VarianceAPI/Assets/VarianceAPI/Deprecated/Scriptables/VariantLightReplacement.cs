﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "LightReplacement", menuName = "VAPI_OLD/LightReplacement")]
    public class VariantLightReplacement : ScriptableObject
    {
        [Header("VariantLightReplacement")]
            [Tooltip("Which RendererIndex You're Targetting")]
            [Min(0)]
            public int rendererIndex;

            [Tooltip("The Replacement Color")]
            public Color color;
    }
}