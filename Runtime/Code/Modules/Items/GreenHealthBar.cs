using MSU;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace VAPI.Items
{
    /// <summary>
    /// <inheritdoc cref="GlobalCDR"/>
    /// </summary>
    public class GreenHealthBar : VAPIItem
    {
        public override VAPIAssetRequest<ItemDef> GetAssetRequest() => VAPIAssets.LoadAssetAsync<ItemDef>("GreenHealthBar");
        public override void Initialize()
        {
            On.RoR2.UI.HealthBar.UpdateBarInfos += HealthBar_UpdateBarInfos;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        private void HealthBar_UpdateBarInfos(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
        {
            orig(self);
            var healthComponent = self._source;
            if (healthComponent)
            {
                var iv = healthComponent.body.inventory;
                if (iv && healthComponent.body.HasItem(itemDef))
                {
                    self.barInfoCollection.trailingOverHealthbarInfo.color = VAPIConfig._variantHealthBarColor;
                }
            }
        }
    }
}
