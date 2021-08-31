using R2API;
using RoR2;
using static R2API.DamageAPI;

namespace VarianceAPI.DamageTypes
{
    public abstract class DamageTypeBase
    {
        public abstract ModdedDamageType ModdedDamageType { get; set; }

        public abstract ModdedDamageType GetDamageType();

        public virtual void Initialize() { }

        public virtual void Delegates() { }
    }
}
