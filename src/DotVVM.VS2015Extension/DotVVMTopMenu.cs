namespace DotVVM.VS2015Extension
{
    using System;
    
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class PackageGuids
    {
        public const string GuidDotvvmMenuPackagePkgString = "1b39c66b-16e9-4b17-a90d-841357cc282c";
        public const string GuidDotvvmMenuCmdSetString = "1658abcf-e4d1-423c-879c-ae9323010bff";
        public const string ButtonsGuidString = "7708d46c-ad36-45ec-ad6e-9e1737cb6fe6";
        public const string GuidHelpMenuGroupString = "4ea6b302-e341-4d55-8d3c-7f547b629ae9";
        public static Guid GuidDotvvmMenuPackagePkg = new Guid(GuidDotvvmMenuPackagePkgString);
        public static Guid GuidDotvvmMenuCmdSet = new Guid(GuidDotvvmMenuCmdSetString);
        public static Guid ButtonsGuid = new Guid(ButtonsGuidString);
        public static Guid GuidHelpMenuGroup = new Guid(GuidHelpMenuGroupString);
    }
    /// <summary>
    /// Helper class that encapsulates all CommandIDs uses across VS Package.
    /// </summary>
    internal sealed partial class PackageIds
    {
        public const int TopMenu = 0x3001;
        public const int ShowDocs = 0x0002;
        public const int ShowOptions = 0x0003;
        public const int ShowAbout = 0x0004;
        public const int CheckUpdates = 0x0005;
        public const int HelpMenuGroupCmd = 0x0001;
    }
}
