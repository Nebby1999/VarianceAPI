using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VAPI.Components
{
    public abstract class VariantComponent : MonoBehaviour
    {
        public ReadOnlyCollection<VariantDef>  VariantDefs { get; internal set; }
        public CharacterBody CharacterBody { get; internal set; }
        public CharacterMaster CharacterMaster { get; internal set; }
        public CharacterModel CharacterModel { get; internal set; }
    }
}
