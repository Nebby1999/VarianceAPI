using EntityStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOriginal30.VariantEntityStates
{
    public class TO30EntityStates : EntityState
    {
        internal static List<Type> EntityStates { get; set; } = new List<Type>
        {
            typeof(Beetle.HeavyHeadbutt),
            typeof(Beetle.ToxicHeadbutt),
            typeof(Lemurian.ChargeFireballnMissile),
            typeof(Jellyfish.NuclearNova),
            typeof(Jellyfish.SpawnNova),
            typeof(LesserWisp.ChargeGreaterCannon)
        };
    }
}
