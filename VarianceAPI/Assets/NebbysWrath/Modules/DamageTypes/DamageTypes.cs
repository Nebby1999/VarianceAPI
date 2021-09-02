using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.ModuleBases;
using VarianceAPI.DamageTypes;
using static R2API.DamageAPI;

namespace NebbysWrath.DamageTypes
{
    public class DamageTypes : DamageTypesModuleBase
    {
        public override Assembly Assembly { get; set; } = typeof(DamageTypes).Assembly;

        public static DamageTypes instance;

        public static Dictionary<ModdedDamageType, DamageTypeBase> NWDamageTypes = new Dictionary<ModdedDamageType, DamageTypeBase>();

        public override void Initialize()
        {
            base.Initialize();
            InitializeDamageTypes();
        }
        public override IEnumerable<DamageTypeBase> InitializeDamageTypes()
        {
            base.InitializeDamageTypes()
                .ToList()
                .ForEach(dType =>
                {
                    dType.Initialize();
                    var moddedDamageType = dType.GetDamageType();
                    damageTypes.Add(moddedDamageType, dType);
                    NWDamageTypes.Add(moddedDamageType, dType);
                });
            return null;
        }
    }
}
