using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.Providers;

namespace DotVVM.CommandLine.Commands.Logic.Compiler
{
    public class DotvvmSeleniumGeneratorProvider : DotvvmToolProvider
    {
        public static DotvvmToolMetadata GetToolMetadata(IResolvedProjectMetadata metadata)
        {
            if ((metadata.TargetFramework & TargetFramework.NetFramework) > 0)
            {
                return new DotvvmToolMetadata() {
                    MainModulePath = CombineNugetPath(metadata, "tools\\selenium\\net46\\DotVVM.Framework.Tools.SeleniumGenerator.exe"),
                    Version = DotvvmToolExecutableVersion.FullFramework
                };
            }

            return new DotvvmToolMetadata() {
                MainModulePath = CombineNugetPath(metadata, "tools\\selenium\\netcoreapp2.0\\DotVVM.Framework.Tools.SeleniumGenerator.exe"),
                Version = DotvvmToolExecutableVersion.DotNetCore
            };
        }
    }
}