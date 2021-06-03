using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "EquipmentInfo", menuName = "VarianceAPI/EquipmentInfo", order = 4)]
    public class EquipmentInfo : ScriptableObject
    {
        /// <summary>
        /// The internal name of the equipment, refer to the equipment's EQUIPMENTDEF
        /// </summary>
        public string equipmentString;
    }
}
