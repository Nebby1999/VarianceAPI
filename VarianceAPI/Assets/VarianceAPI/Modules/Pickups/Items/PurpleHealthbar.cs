using Moonstorm;
using RoR2;
using UnityEngine;

namespace VarianceAPI.Items
{
    public class PurpleHealthbar : ItemBase
    {
        public override ItemDef ItemDef { get; set; } = Assets.VAPIAssets.LoadAsset<ItemDef>("PurpleHealthbar");
        public Color32 color = new Color32(231, 0, 231, byte.MaxValue);
        public override void Initialize()
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
}
