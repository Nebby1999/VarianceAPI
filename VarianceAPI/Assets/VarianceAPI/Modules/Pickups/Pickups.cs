using RoR2;
using VarianceAPI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Item = VarianceAPI.Items.ItemBase;

namespace VarianceAPI
{
    internal static class Pickups
    {
        public static ItemDef[] loadedItemDefs
        {
            get
            {
                return ContentPacks.serializableContentPack.itemDefs;
            }
        }
        public static EquipmentDef[] loadedEquipDefs
        {
            get
            {
                return ContentPacks.serializableContentPack.equipmentDefs;
            }
        }

        public static Dictionary<ItemDef, Item> items = new Dictionary<ItemDef, Item>();

        public static Dictionary<string, UnityEngine.Object> itemPickups = new Dictionary<string, UnityEngine.Object>();
        public static Dictionary<string, UnityEngine.Object> equipPickups = new Dictionary<string, UnityEngine.Object>();
        public static void Initialize()
        {
            VAPILog.LogI("Initializing Pickups.");
            InitializeItems();
            CharacterBody.onBodyStartGlobal += AddItemManager;
            On.RoR2.CharacterBody.RecalculateStats += OnRecalculateStats;
        }

        private static void InitializeItems()
        {
            typeof(Pickups).Assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Item)))
                .Select(itemType => (Item)Activator.CreateInstance(itemType))
                .ToList()
                .ForEach(item =>
                {
                    HG.ArrayUtils.ArrayAppend(ref ContentPacks.serializableContentPack.itemDefs, item.ItemDef);
                    item.Initialize();
                    items.Add(item.ItemDef, item);
                    VAPILog.LogD($"Added item {item.ItemDef.name}");
                });
        }

        private static void AddItemManager(CharacterBody body)
        {
            if (!body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && body.master.inventory)
            {
                var itemManager = body.gameObject.AddComponent<VAPIItemManager>();
                itemManager.CheckForSS2Items();
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