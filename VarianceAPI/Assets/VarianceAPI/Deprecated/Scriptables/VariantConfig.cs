using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI.Scriptables
{
    [CreateAssetMenu(fileName = "VariantConfig", menuName = "VAPI_OLD/VariantConfig")]
    public class VariantConfig : ScriptableObject
    {
        [Header("Variant Config")]
            [Tooltip("The spawnrate of the Variant\\nAccepted Values range from 0 to 100")]
            [Range(0,100)]
            public float spawnRate;

            [Tooltip("Whether the Variant youre creating can overlap with other variants")]
            public bool isUnique;

            [Tooltip("Identifier of the Variant, must be the same as the Identifier of the VariantInfo youre attatching this scriptable object to.")]
            public string identifier;

        [Header("Mod Atributes")]
            [Tooltip("Wether the Variant youre created is from a mod. Doesn't override the value set in VariantInfo\nOnly used in the Config file.")]
            public bool isModded;
            
            [Tooltip("The name of the mod the user needs to have installed for this variant to be added.")]
            public string modName;

            [Tooltip("The author of the original mod.")]
            public string modAuthor;

            [Tooltip("The required mod's GUID for the variant to register.\nGUID is NOT the Dependency string found in the Thunderstore site.")]
            public string modGUID;
    }
}
