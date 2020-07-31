using System.IO;
using Microsoft.VisualStudio.Setup.Configuration;

namespace DotVVM.Tool
{
    public class MSBuild
    {
        public const string DefaultTargetFramework = "netcoreapp3.1";
        public const string VSRelativePath = "./MSBuild/Current/Bin/MSBuild.exe";

        public string TargetFramework { get; } = DefaultTargetFramework;
        public string Path { get; } = string.Empty;

        public static string FindSDKPath()
        {
            return new MSBuild();
        }

        public static string? FindVSPath()
        {
            var query = new SetupConfiguration();
            var query2 = (ISetupConfiguration2)query;
            var @enum = query2.EnumAllInstances();
            var instances = new ISetupInstance[1];
            int fetchedCount;
            do
            {
                @enum.Next(1, instances, out fetchedCount);
                if (fetchedCount > 0)
                {
                    var instance2 = (ISetupInstance2)instances[0];
                    var path = instance2.GetInstallationPath();
                    var exePath = System.IO.Path.Combine(path, VSRelativePath);
                    if (File.Exists(exePath))
                    {
                        return exePath;
                    }
                }
            }
            while (fetchedCount > 0);
            return null;
        }

        public static MSBuild Create(FileInfo project)
        {
            var vs = CreateFromVS();
            return new MSBuild();
        }
    }
}
