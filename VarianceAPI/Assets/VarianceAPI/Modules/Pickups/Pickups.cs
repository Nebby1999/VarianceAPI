using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Items;
using VarianceAPI.ModuleBases;

namespace VarianceAPI
{
    public class Pickups : PickupsModuleBase
    {
        public override SerializableContentPack ContentPack { get; set; } = ContentPacks.serializableContentPack;
        public override Assembly Assembly { get; set; } = typeof(Pickups).Assembly;

        public static Pickups instance;

        public static Dictionary<ItemDef, ItemBase> VAPIItems = new Dictionary<ItemDef, ItemBase>();
        public override void Initialize()
        {
            VAPILog.LogI("Initializing Pickups...");
            instance = this;
            base.Initialize();
            InitializeItems();
        }
        public override IEnumerable<ItemBase> InitializeItems()
        {
            VAPILog.LogD("Initializing Items...");
            base.InitializeItems()
                .ToList()
                .ForEach(itemBase =>
                {
                    HG.ArrayUtils.ArrayAppend(ref ContentPack.itemDefs, itemBase.ItemDef);

                    itemBase.Initialize();

                    //This is just to keep track of the vapi items that are active.
                    VAPIItems.Add(itemBase.ItemDef, itemBase);

                    //We also add them to the pickups module base's dictionary.
                    ItemsForManager.Add(itemBase.ItemDef, itemBase);
                    
                    VAPILog.LogD($"Added Item {itemBase.ItemDef.name} to {ContentPack.name}");
                });
            return null;
        }
    }
}
