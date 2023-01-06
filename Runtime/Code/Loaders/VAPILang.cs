using Moonstorm.Loaders;

namespace VAPI
{
    /// <summary>
    /// VAPI's LanguageLoader
    /// </summary>
    public class VAPILang : LanguageLoader<VAPILang>
    {
        public override string AssemblyDir => VAPIAssets.Instance.AssemblyDir;

        public override string LanguagesFolderName => "VAPILang";

        internal void Init()
        {
            LoadLanguages();
        }
    }
}