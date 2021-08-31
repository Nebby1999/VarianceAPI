using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Components;
using VarianceAPI.Items;

namespace VarianceAPI.ModuleBases
{
    public abstract class PickupsModuleBase
    {
        public static bool delegates = false;

        public static Dictionary<ItemDef, ItemBase> ItemsForManager = new Dictionary<ItemDef, ItemBase>();

        public ItemDef[] LoadedItemDefs
        {
            get
            {
                return ContentPack.itemDefs;
            }
        }
        public EquipmentDef[] LoadedEquipDefs
        {
            get
            {
                return ContentPack.equipmentDefs;
            }
        }

        public abstract SerializableContentPack ContentPack { get; set; }

        public abstract Assembly Assembly { get; set; }

        public virtual void Initialize()
        {
            //No need to suscribe twice;
            if(!delegates)
            {
                VAPILog.LogI("This should only appear once");
                delegates = true;
                CharacterBody.onBodyStartGlobal += AddItemManager;
                On.RoR2.CharacterBody.RecalculateStats += OnRecalculateStats;
            }
        }

        public virtual IEnumerable<ItemBase> InitializeItems()
        {
            return Assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)))
                .Select(itemBase => (ItemBase)Activator.CreateInstance(itemBase));
        }

        private static void AddItemManager(CharacterBody body)
        {
            if (!body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && body.master.inventory)
            {
                var itemManager = body.gameObject.AddComponent<VAPIItemManager>();
                itemManager.CheckForVAPIItems();
            }
        }

        //Only time we should do this
        private static void OnRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            var manager = self.GetComponent<VAPIItemManager>();
            manager?.RunStatRecalculationsStart();
            orig(self);
            manager?.RunStatRecalculationsEnd();
        }
    }
}
