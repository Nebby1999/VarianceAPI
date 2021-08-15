using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Modules.Items.ItemBases;
using Logger = VarianceAPI.MainClass;

namespace VarianceAPI.Modules.Items
{
    public class Plus1Crit : Thunderkit_ItemBase
    {
        public override void Init()
        {
            ItemDef = ContentPackProvider.contentPack.itemDefs.Find("VAPI_Plus1Crit");
            if (ItemDef)
            {
                Hooks();
            }
            else
            {
                Logger.Log.LogError("ItemDef is Null! aborting...");
                return;
            }
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += AddCrit;
        }

        private void AddCrit(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
        {
            orig(self);
            var thisInventory = self.inventory;
            if (thisInventory)
            {
                var itemCount = thisInventory.GetItemCount(ItemDef);
                if (itemCount > 0)
                {
                    self.crit += itemCount;
                }
            }
        }
    }
}