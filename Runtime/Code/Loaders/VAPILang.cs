using Moonstorm.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPI
{
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