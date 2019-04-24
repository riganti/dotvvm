using System.IO;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.Providers;

namespace DotVVM.CommandLine.Commands.Logic.Compiler
{
    public class DotvvmCompilerProvider
    {
        public DotvvmCompilerMetadata GetCompilerMetadata(IResolvedProjectMetadata metadata)
        {
            //tools
            //dnc
            if ((metadata.TargetFramework & TargetFramework.NetFramework) > 0)
            {
                return new DotvvmCompilerMetadata() {
                    MainModulePath = Path.Combine(metadata.DotvvmPackageNugetFolder, "tools\\DotVVM.Compiler.exe"),
                    Version = DotvvmCompilerExecutableVersion.FullFramework
                };
            }
            else
            {
                return new DotvvmCompilerMetadata() {
                    MainModulePath = Path.Combine(metadata.DotvvmPackageNugetFolder, "tools\\dnc\\DotVVM.Compiler.dll"),
                    Version = DotvvmCompilerExecutableVersion.DotNetCore
                };
            }
        }
    }
}
