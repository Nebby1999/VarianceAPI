﻿/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Modules.Items.ItemBases;
using Logger = VarianceAPI.MainClass;

namespace VarianceAPI.Modules.Items
{
    public class UtilityCDR : Thunderkit_ItemBase
    {
        public override void Init()
        {
            ItemDef = ContentPackProvider.contentPack.itemDefs.Find("VAPI_UtilityCDR");
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
            On.RoR2.CharacterBody.RecalculateStats += UtilityCooldownReduction;
        }

        private void UtilityCooldownReduction(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
        {
            orig(self);
            var thisInventory = self.inventory;
            var skillLocator = self.skillLocator;
            if (thisInventory)
            {
                float itemCount = thisInventory.GetItemCount(ItemDef);
                if (itemCount > 0)
                {
                    if (skillLocator)
                    {
                        if ((bool)skillLocator.utility)
                        {
                            skillLocator.utility.cooldownScale -= itemCount / 100f;
                        }
                    }
                }
            }
        }
    }
}*/