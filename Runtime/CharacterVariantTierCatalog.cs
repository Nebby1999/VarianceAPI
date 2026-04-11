#nullable enable
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace VAPI
{
    public enum CharacterVariantTierIndex
    {
        None = -1,
    }

    public static class CharacterVariantTierCatalog
    {
        public static int characterVariantTierCount => _characterVariantTierDefs?.Length ?? -1;
        private static CharacterVariantTierDef[]? _characterVariantTierDefs;
        private static Dictionary<string, CharacterVariantTierIndex> _characterVariantTierNametoIndex = new Dictionary<string, CharacterVariantTierIndex>(StringComparer.OrdinalIgnoreCase);

        public static ResourceAvailability catalogAvailability;

        public static CharacterVariantTierDef GetCharacterVariantTierDef(CharacterVariantTierIndex index)
        {
            ThrowIfUnavailable();
            return HG.ArrayUtils.GetSafe(_characterVariantTierDefs!, (int)index);
        }

        public static CharacterVariantTierIndex FindCharacterVariantTierIndex(string characterVariantTierName)
        {
            ThrowIfUnavailable();
            return _characterVariantTierNametoIndex.GetValueOrDefault(characterVariantTierName, CharacterVariantTierIndex.None);
        }

        [SystemInitializer(typeof(CharacterVariantCatalog))]
        private static void Initialize()
        {

        }

        private static void ThrowIfUnavailable()
        {
            if (!catalogAvailability.available)
            {
                throw new InvalidOperationException("Cannot access CharacterVariantTierCatalog when it's not available, consider subscribing to the catalogAvailability.");
            }
        }
    }

    public static partial class Extensions
    {
        public static void Write(this NetworkWriter writer, CharacterVariantTierIndex index)
        {
            writer.WritePackedIndex32((int)index);
        }

        public static CharacterVariantTierIndex ReadCharacterVariantTierIndex(this NetworkReader reader)
        {
            return (CharacterVariantTierIndex)reader.ReadPackedIndex32();
        }
    }
}