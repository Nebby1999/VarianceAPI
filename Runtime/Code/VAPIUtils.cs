using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace VAPI
{
    public static class VAPIUtils
    {
        public static void WriteVariantIndex(this NetworkWriter writer, VariantIndex index)
        {
            writer.Write((int)index);
        }

        public static VariantIndex ReadVariantIndex(this NetworkReader reader)
        {
            var integer = reader.ReadInt32();
            return (VariantIndex)integer;
        }
    }
}
