namespace VAPI.EditorUtils.Importers
{
    using TKPriority = ThunderKit.Common.Constants.Priority;
    public static class Constants
    {
        public static class Priority
        {
            public const int InstallDebugToolkit = TKPriority.AddressableCatalog - 165_500;
        }
    }
}