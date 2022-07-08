/*using Moonstorm;
using RoR2.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace VarianceAPI
{
    public class Pickups : PickupModuleBase
    {
        public override SerializableContentPack ContentPack { get; set; } = ContentPacks.serializableContentPack;

        public static Pickups instance;

        public override void Init()
        {
            VAPILog.LogI("Initializing Pickups...");
            instance = this;
            base.Init();
            InitializeItems();
        }
        public override IEnumerable<ItemBase> InitializeItems()
        {
            VAPILog.LogD("Initializing Items...");
            base.InitializeItems()
                .ToList()
                .ForEach(itemBase => AddItem(itemBase, ContentPack));
            return null;
        }
    }
}*/
