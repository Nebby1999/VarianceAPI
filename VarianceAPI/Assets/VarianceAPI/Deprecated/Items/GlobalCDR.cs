/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Modules.Items.ItemBases;
using Logger = VarianceAPI.MainClass;

namespace VarianceAPI.Modules.Items
{
    public class GlobalCDR : Thunderkit_ItemBase
    {
        public override void Init()
        {
            ItemDef = ContentPackProvider.contentPack.itemDefs.Find("VAPI_GlobalCDR");
            if(ItemDef)
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
            On.RoR2.CharacterBody.RecalculateStats += GlobalCooldownReduction;
        }

        private void GlobalCooldownReduction(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
        {
            orig(self);
            var thisInventory = self.inventory;
            var skillLocator = self.skillLocator;
            if(thisInventory)
            {
                float itemCount = thisInventory.GetItemCount(ItemDef);
                if(itemCount > 0)
                {
                    if(skillLocator)
                    {
                        if((bool)skillLocator.primary)
                        {
                            skillLocator.primary.cooldownScale -= itemCount / 100f;
                        }
                        if((bool)skillLocator.secondary)
                        {
                            skillLocator.secondary.cooldownScale -= itemCount / 100f;
                        }
                        if((bool)skillLocator.utility)
                        {
                            skillLocator.utility.cooldownScale -= itemCount / 100f;
                        }
                        if((bool)skillLocator.special)
                        {
                            skillLocator.special.cooldownScale -= itemCount / 100f;
                        }
                    }
                }
            }
        }
    }
}
*/