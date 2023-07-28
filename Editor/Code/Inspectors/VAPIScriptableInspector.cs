using RoR2EditorKit.Inspectors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI.EditorUtils.Inspectors
{
    public abstract class VAPIScriptableInspector<T> : ScriptableObjectInspector<T> where T : UnityEngine.ScriptableObject
    {
        protected sealed override bool ValidateUXMLPath(string path)
        {
            return path.Contains("nebby-varianceapi");
        }
    }
}
