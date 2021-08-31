using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.DamageTypes;
using static R2API.DamageAPI;

namespace VarianceAPI.ModuleBases
{
    public abstract class DamageTypesModuleBase
    {
        public static bool delegates = false;

        public static Dictionary<ModdedDamageType, DamageTypeBase> damageTypes = new Dictionary<ModdedDamageType, DamageTypeBase>();

        public static ModdedDamageType[] ModdedDamageTypes
        {
            get
            {
                return damageTypes.Keys.ToArray();
            }
        }

        public abstract Assembly Assembly { get; set; }

        public virtual void Initialize()
        {
            if(!delegates)
            {
                VAPILog.LogI("This should only appear once");
                delegates = true;
            }
        }
        public virtual IEnumerable<DamageTypeBase> InitializeDamageTypes()
        {
            return Assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(DamageTypeBase)))
                .Select(dtBase => (DamageTypeBase)Activator.CreateInstance(dtBase));
        }
    }
}
