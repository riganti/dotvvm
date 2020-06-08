using System;
using System.Linq;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Lookup;
using DotVVM.Utils.ProjectService.Operations.Providers;

namespace DotVVM.CommandLine.Commands.Logic.Compiler
{
    public class DotvvmSeleniumGeneratorProvider : DotvvmToolProvider
    {
        protected override DotvvmToolMetadata GetToolMetadata(IResolvedProjectMetadata metadata)
        {
            var dotvvm = metadata.DotvvmProjectDependencies.First(s => s.Name.Equals("DotVVM", StringComparison.OrdinalIgnoreCase));

            // since the tool itself cannot be compiled for .net framework 4.6.1,
            // use the .net core version
            if (dotvvm.IsProjectReference)
            {
                return CreateMetadataOrDefault(
                    mainModule:
                    CombineDotvvmRepositoryRoot(
                        metadata,
                        dotvvm,
                        @"..\..\src\DotVVM.Framework.Tools.SeleniumGenerator\bin\Debug\netcoreapp2.0\DotVVM.Framework.Tools.SeleniumGenerator.dll") ??
                    CombineDotvvmRepositoryRoot(
                        metadata,
                        dotvvm,
                        @"DotVVM.Framework.Tools.SeleniumGenerator\bin\Debug\netcoreapp2.0\DotVVM.Framework.Tools.SeleniumGenerator.dll"),
                    version: DotvvmToolExecutableVersion.DotNetCore);
            }

            return CreateMetadataOrDefault(
                mainModule: CombineNugetPath(metadata,
                    "tools\\selenium\\netcoreapp2.0\\DotVVM.Framework.Tools.SeleniumGenerator.dll"),
                version: DotvvmToolExecutableVersion.DotNetCore
            );
        }
    }
}
