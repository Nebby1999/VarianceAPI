using ThunderKit.Integrations.Thunderstore;

namespace VAPI.EditorUtils.Importers
{
    public class InstallDebugToolkit : ThunderstorePackageInstaller
    {
        public override string DependencyId => "IHarbHD-DebugToolkit";

        public override string ThunderstoreAddress => "https://thunderstore.io";

        public override int Priority => Constants.Priority.InstallDebugToolkit;

        public override string Description => "Installs DebugToolKit. Required for VAPI's debug commands to function properly";
    }
}
