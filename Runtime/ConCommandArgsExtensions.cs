#nullable enable
using RoR2;
using System;

namespace VAPI
{
    internal static class ConCommandArgsExtensions
    {
        public static bool TryGetOptionalArgIndex(this ConCommandArgs args, string startingIdentifier, out int index, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            for (int i = 0; i < args.userArgs.Count; i++)
            {
                if (args.TryGetArgString(i).StartsWith(startingIdentifier, stringComparison))
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        public static bool? TryGetOptionalBool(this ConCommandArgs args, string startingIdentifier, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            for (int i = 0; i < args.userArgs.Count; i++)
            {
                if (args.TryGetArgString(i).StartsWith(startingIdentifier, stringComparison))
                {
                    string rawArg = args.userArgs[i];
                    if (bool.TryParse(rawArg.Substring(startingIdentifier.Length), out bool result))
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public static string? TryGetOptionalString(this ConCommandArgs args, string startingIdentifier, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            for (int i = 0; i < args.userArgs.Count; i++)
            {
                if (args.TryGetArgString(i).StartsWith(startingIdentifier, stringComparison))
                {
                    string rawArg = args.userArgs[i];
                    return rawArg.Substring(startingIdentifier.Length);
                }
            }
            return null;
        }

        public static float? TryGetOptionalFloat(this ConCommandArgs args, string startingIdentifier, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            for (int i = 0; i < args.userArgs.Count; i++)
            {
                if (args.TryGetArgString(i).StartsWith(startingIdentifier, stringComparison))
                {
                    string rawArg = args.userArgs[i];
                    if (float.TryParse(rawArg.Substring(startingIdentifier.Length), out float result))
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public static int? TryGetOptionalInt(this ConCommandArgs args, string startingIdentifier, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            for(int i = 0; i < args.userArgs.Count; i++)
            {
                if(args.TryGetArgString(i).StartsWith(startingIdentifier, stringComparison))
                {
                    string rawArg = args.userArgs[i];
                    if(int.TryParse(rawArg.Substring(startingIdentifier.Length), out int result))
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public static EquipmentIndex? TryGetOptionalEquipmentIndex(this ConCommandArgs args, string startingIdentifier)
        {
            string? optionalString = args.TryGetOptionalString(startingIdentifier);
            if (optionalString == null)
                return null;

            EquipmentIndex equipmentIndex = EquipmentCatalog.FindEquipmentIndex(optionalString);
            return equipmentIndex;
        }

        public static TEnum? TryGetOptionalEnum<TEnum>(this ConCommandArgs args, string startingIdentifier, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase) where TEnum : struct
        {
            for (int i = 0; i < args.userArgs.Count; i++)
            {
                if (args.TryGetArgString(i).StartsWith(startingIdentifier, stringComparison))
                {
                    string rawArg = args.userArgs[i];
                    if (Enum.TryParse(rawArg.Substring(startingIdentifier.Length), true, out TEnum result))
                    {
                        return result;
                    }
                }
            }
            return null;
        }
    }
}