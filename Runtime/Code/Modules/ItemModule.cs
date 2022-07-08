using Moonstorm;
using R2API.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPI.Modules
{
    public class ItemModule : ItemModuleBase
    {
        public override R2APISerializableContentPack SerializableContentPack => VAPIContent.Instance.SerializableContentPack;

        public static ItemModule Instance { get; private set; }

        public override void Initialize()
        {
            Instance = this;
            VAPILog.Info($"Initializing Items...");
            base.Initialize();
            GetItemBases();
        }

        protected override IEnumerable<ItemBase> GetItemBases()
        {
            base.GetItemBases()
                .ToList()
                .ForEach(itemBase => AddItem(itemBase));
            return null;
        }
    }
}