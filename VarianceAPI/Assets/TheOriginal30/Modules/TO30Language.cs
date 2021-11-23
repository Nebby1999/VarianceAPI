using R2API;
using System.IO;

namespace TheOriginal30
{
    public static class TO30Language
    {
        public static string languageFileName = "TO30Language.language";
        public static string pathToLanguage;
        public static void Initialize()
        {
            MainClass.logger.LogInfo("Initializing Language");
            var path = Path.Combine(MainClass.pluginInfo.Location, languageFileName);
            LanguageAPI.AddPath(path);
        }
    }
}