namespace VAPI
{
    /// <summary>
    /// Interface that determines that a struct or class can be Validated by using a ScriptableObject/MonoBehaviour's "OnValidate" method
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Validate the data in this
        /// </summary>
        public void Validate();
    }
}