using RoR2.Editor;

namespace VAPI.Editor
{
    public abstract class VAPIPropertyDrawer<T> : VisualElementPropertyDrawer<T>
    {
        protected sealed override bool ValidatePath(string path)
        {
            return path.Contains("varianceapi");
        }
    }
}