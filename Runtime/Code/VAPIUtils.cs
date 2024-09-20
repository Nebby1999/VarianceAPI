using UnityEngine.Networking;

namespace VAPI
{
    /// <summary>
    /// Utility and extension class from VAPI
    /// </summary>
    public static class VAPIUtils
    {
        /// <summary>
        /// Writes a <see cref="VariantIndex"/> to the network writer
        /// </summary>
        /// <param name="writer">The writer to use</param>
        /// <param name="index">The index to write</param>
        public static void WriteVariantIndex(this NetworkWriter writer, VariantIndex index)
        {
            writer.Write((int)index);
        }

        /// <summary>
        /// Reads a VariantIndex from the network reader
        /// </summary>
        /// <param name="reader">The reader to use</param>
        /// <returns>The read variant index</returns>
        public static VariantIndex ReadVariantIndex(this NetworkReader reader)
        {
            var integer = reader.ReadInt32();
            return (VariantIndex)integer;
        }

        public static void WriteVariantTierIndex(this NetworkWriter writer, VariantTierIndex index)
        {
            writer.Write((int)index);
        }

        public static VariantTierIndex ReadVariantTierIndex(this NetworkReader reader)
        {
            var integer = reader.ReadInt32();
            return (VariantTierIndex)integer;
        }

        public static void WriteVariantPackIndex(this NetworkWriter writer, VariantPackIndex index)
        {
            writer.Write((int)index);
        }

        public static VariantPackIndex ReadVariantPackIndex(this NetworkReader reader)
        {
            var integer = reader.ReadInt32();
            return (VariantPackIndex)integer;
        }
    }
}
