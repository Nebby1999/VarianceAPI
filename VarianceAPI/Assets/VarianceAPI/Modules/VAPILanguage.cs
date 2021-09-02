using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R2API;
using System.Reflection;
using System.IO;

namespace VarianceAPI
{
    public static class VAPILanguage
    {
        public static string languageFileName = "VAPILanguage.language";
        public static string pathToLanguage;
        public static void Initialize()
        {
            VAPILog.LogI("Initializing Language");
            var path = Path.Combine(Assets.assemblyPath, languageFileName);
            LanguageAPI.AddPath(path);
        }
    }
}