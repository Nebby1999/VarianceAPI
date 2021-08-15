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
    public class ExtraSecondary : Thunderkit_ItemBase
    {
        public override void Init()
        {
            ItemDef = ContentPackProvider.contentPack.itemDefs.Find("VAPI_ExtraSecondary");
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
            On.RoR2.CharacterBody.RecalculateStats += AddExtraSecondary;
        }

        private void AddExtraSecondary(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
        {
            orig(self);
            var thisInventory = self.inventory;
            var skillLocator = self.skillLocator;
            if(thisInventory)
            {
                var itemCount = thisInventory.GetItemCount(ItemDef);
                if(itemCount > 0)
                {
                    if(skillLocator)
                    {
                        if((bool)skillLocator.secondary)
                        {
                            skillLocator.secondary.SetBonusStockFromBody(itemCount + thisInventory.GetItemCount(RoR2.RoR2Content.Items.SecondarySkillMagazine));
                        }
                    }
                }
            }
        }
    }
}
