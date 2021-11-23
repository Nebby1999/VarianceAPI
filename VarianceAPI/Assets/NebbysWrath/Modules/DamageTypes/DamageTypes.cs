using Moonstorm;
using System.Collections.Generic;
using System.Linq;

namespace NebbysWrath.DamageTypes
{
    public class DamageTypes : DamageTypeModuleBase
    {
        public static DamageTypes instance;

        public override void Init()
        {
            MainClass.logger.LogInfo($"Initializing Damage Types");
            base.Init();
            InitializeDamageTypes();
        }
        public override IEnumerable<DamageTypeBase> InitializeDamageTypes()
        {
            base.InitializeDamageTypes()
                .ToList()
                .ForEach(dType => AddDamageType(dType));
            return null;
        }
    }
}
