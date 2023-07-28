using Moonstorm;
using RoR2;
using UnityEngine;

namespace VAPI.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class GreenHealthBar : ItemBase
    {
        public override ItemDef ItemDef { get; } = VAPIAssets.LoadAsset<ItemDef>("GreenHealthBar");
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
                var iv = healthComponent.body.inventory;
                if (iv && healthComponent.body.inventory.GetItemCount(ItemDef) > 0)
                {
                    self.barInfoCollection.trailingOverHealthbarInfo.color = VAPIConfig.variantHealthBarColor;
                }
            }
        }
    }
}
