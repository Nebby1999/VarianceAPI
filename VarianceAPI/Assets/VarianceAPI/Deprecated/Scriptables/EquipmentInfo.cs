using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "EquipmentInfo", menuName = "VarianceAPI/EquipmentInfo", order = 5)]
    public class EquipmentInfo : ScriptableObject
    {
        [Header("Equipment Info")]
            [Tooltip("The Equipment to give to this Variant\nMUST be the same as the Equipment's EquipmentDef!")]
            public string equipmentString;
            
            [Header("Minimum health fraction")]
            [Tooltip("This is a Chance curve that dictate")]
            public AnimationCurve animationCurve;
    }
}
