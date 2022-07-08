﻿using BepInEx.Logging;
using System.Runtime.CompilerServices;

namespace VAPI
{
    internal class VAPILog
    {
        private static ManualLogSource logger = null;

        internal VAPILog(ManualLogSource logger_)
        {
            logger = logger_;
        }

        internal static void Debug(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            logger.LogDebug(logString(data, i, member));
        }
        internal static void Error(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            logger.LogError(logString(data, i, member));
        }
        internal static void Fatal(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            logger.LogFatal(logString(data, i, member));
        }
        internal static void Info(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            if (VAPIConfig.enableDebugFeatures.Value)
                logger.LogInfo(logString(data, i, member));
            else
                logger.LogInfo(data);
        }
        internal static void Message(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            if (VAPIConfig.enableDebugFeatures.Value)
                logger.LogMessage(logString(data, i, member));
            else
                logger.LogMessage(data);
        }
        internal static void Warning(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            if (VAPIConfig.enableDebugFeatures.Value)
                logger.LogWarning(logString(data, i, member));
            else
                logger.LogWarning(data);
        }

        private static string logString(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            return string.Format("{0} :: Line: {1}, Method {2}", data, i, member);
        }
    }
}