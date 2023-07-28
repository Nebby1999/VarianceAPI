using BepInEx.Logging;
using Moonstorm.Loaders;
using System.Runtime.CompilerServices;

namespace VAPI
{
    internal class VAPILog : LogLoader<VAPILog>
    {
        public override ManualLogSource LogSource { get => _logSource; protected set => _logSource = value; }
        public ManualLogSource _logSource;
        public override BreakOnLog BreakOn => BreakOnLog.Fatal | BreakOnLog.Error;

        public VAPILog(ManualLogSource logSource) : base(logSource)
        {
        }
    }
}