using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Editor;

namespace VAPI.Editor.Inspectors
{
    public abstract class VAPIScriptableInspector<T> : VisualElementScriptableObjectInspector<T> where T : UnityEngine.ScriptableObject
    {
        protected sealed override bool ValidatePath(string path)
        {
            return path.Contains("nebby-varianceapi");
        }
    }
}
