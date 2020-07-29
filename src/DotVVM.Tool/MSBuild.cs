using System.IO;
using Microsoft.Build.Locator;

namespace DotVVM.Tool
{
    public class MSBuild
    {
        public const string DefaultTargetFramework = "netcoreapp3.1";

        public string TargetFramework { get; } = DefaultTargetFramework;

        public static MSBuild CreateFromSdk()
        {
        }

        public static MSBuild Create(FileInfo project)
        {
        }
    }
}
