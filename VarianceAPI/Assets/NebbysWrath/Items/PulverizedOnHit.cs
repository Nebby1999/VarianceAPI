using RoR2;
using UnityEngine;
using VarianceAPI.Modules.Items.ItemBases;

namespace NebbysWrath.Items
{
    public class PulverizedOnHit : Thunderkit_ItemBase
    {
        public override void Init()
        {
            ItemDef = ContentPackProvider.contentPack.itemDefs.Find("NW_PulverizedOnHit");
            {
                if(ItemDef)
                {
                    Hooks();
                }
            }
        }
        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, UnityEngine.GameObject victim)
        {
            GameObject attacker = damageInfo.attacker;
            if ((bool)self && (bool)attacker)
            {
                CharacterBody attackerBody = attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                int count = GetCount(attackerBody);
                if(count > 0)
                {
                    victimBody.AddTimedBuff(RoR2Content.Buffs.Pulverized, 8);
                }
            }
            orig(self, damageInfo, victim);
        }
    }
}
