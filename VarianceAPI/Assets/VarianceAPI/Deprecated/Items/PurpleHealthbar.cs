/*using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Modules.Items.ItemBases;
using static RoR2.UI.HealthBar;

namespace VarianceAPI.Modules.Items
{
    public class PurpleHealthbar : Thunderkit_ItemBase
    {
        public BarInfo HealthBarInfo;
        public Color32 color;
        public override void Init()
        {
            ItemDef = ContentPackProvider.contentPack.itemDefs.Find("VAPI_PurpleHealthbar");
            if (ItemDef)
            {
                color = new Color32(231, 0, 231, byte.MaxValue);
                Hooks();
            }
            else
            {
                Debug.LogError("VarianceAPI: ItemDef is null! aborting!");
                return;
            }
        }

        public override void Hooks()
        {
            On.RoR2.UI.HealthBar.UpdateBarInfos += HealthBar_UpdateBarInfos;
        }

        private void HealthBar_UpdateBarInfos(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
        {
            orig(self);
            var healthComponent = self._source;
            if (healthComponent)
            {
                var characterBody = healthComponent.body;
                if (characterBody)
                {
                    var inventory = characterBody.inventory;
                    if (inventory)
                    {
                        var itemCount = inventory.GetItemCount(ItemDef);
                        if (itemCount > 0)
                        {
                            self.barInfoCollection.healthBarInfo.color = color;
                        }
                    }
                }
            }
        }
    }
}*/