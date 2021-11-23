using Moonstorm;
using R2API;
using RoR2;

namespace NebbysWrath.DamageTypes
{
    public class PulverizeOnHit : DamageTypeBase
    {
        public override DamageAPI.ModdedDamageType ModdedDamageType { get; set; }

        public static DamageAPI.ModdedDamageType pulverizeOnHit;

        public override void Initialize()
        {
            pulverizeOnHit = DamageAPI.ReserveDamageType();
            ModdedDamageType = pulverizeOnHit;
            Delegates();
        }

        public override DamageAPI.ModdedDamageType GetDamageType()
        {
            return pulverizeOnHit;
        }

        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += Pulverize;
        }

        private void Pulverize(DamageReport damageReport)
        {
            var victimBody = damageReport.victimBody;
            var attackerBody = damageReport.attackerBody;
            var damageInfo = damageReport.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                victimBody.AddTimedBuff(RoR2Content.Buffs.Pulverized, 16 * damageInfo.procCoefficient);
            }
        }
    }
}
