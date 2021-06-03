using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantConfig", menuName = "VarianceAPI/VariantConfig", order = 1)]
    public class VariantConfig : ScriptableObject
    {
        public float spawnRate;
        public bool isUnique;
        public string identifier;

        public bool isModded;
        public string modAuthor;
        public string modName;
    }
}
