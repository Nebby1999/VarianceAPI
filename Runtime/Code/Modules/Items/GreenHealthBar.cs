using Moonstorm;
using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using System;

namespace VAPI.Items
{
    public class GreenHealthBar : ItemBase
    {
        public override ItemDef ItemDef { get; } = VAPIAssets.LoadAsset<ItemDef>("GreenHealthBar");
        public Color32 color = new Color32(0, 255, 144, byte.MaxValue);
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
                if (healthComponent.body.GetItemCount(ItemDef) > 0)
                {
                    self.barInfoCollection.trailingOverHealthbarInfo.color = color;
                }
            }
        }
    }
}
