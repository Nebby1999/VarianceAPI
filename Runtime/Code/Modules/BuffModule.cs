using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moonstorm;
using R2API.ScriptableObjects;

namespace VAPI.Modules
{
    public class BuffModule : BuffModuleBase
    {
        public override R2APISerializableContentPack SerializableContentPack => VAPIContent.Instance.SerializableContentPack;
        public static BuffModule Instance { get; private set; }

        public override void Initialize()
        {
            Instance = this;
            VAPILog.Info($"Initializing Buffs");
            base.Initialize();
            GetBuffBases();
        }

        protected override IEnumerable<BuffBase> GetBuffBases()
        {
            base.GetBuffBases()
                .ToList()
                .ForEach(bb => AddBuff(bb));
            return null;
        }
    }
}
