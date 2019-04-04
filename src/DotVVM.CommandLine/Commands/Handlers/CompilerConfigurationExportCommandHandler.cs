using System;
using System.Linq;
using DotVVM.CommandLine.Commands.Core;
using DotVVM.CommandLine.Commands.Logic.Compiler;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.CommandLine.Core.Metadata;
using DotVVM.Compiler;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Lookup;

namespace DotVVM.CommandLine.Commands.Handlers
{
    public class CompilerConfigurationExportCommandHandler : CommandBase
    {
        public override string Name => "Export DotVVM configuration";
        public override string[] Usages => new[] { "compiler export-config" };

        public override bool TryConsumeArgs(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata) =>
            args[0] == "compiler" && args[1] == "export-config";

        public override void Handle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            var configuration = new ProjectServiceConfiguration {
                Version = CsprojVersion.OlderProjectSystem,
                LookupFolder = Environment.CurrentDirectory
            };

            var results = new ProjectSystemSearcher().Search(configuration).ToList();
            
            var opts = new CompilerStartupOptions() {
                Options = new CompilerOptions {
                    DothtmlFiles = null,
                    AssemblyName = "DotVVM.Samples.Compiler.Net461.Owin",
                    WebSiteAssembly = "C:\\dev\\dotvvm\\src\\DotVVM.Samples.Compiler.Net461.Owin\\bin\\DotVVM.Samples.Compiler.Net461.Owin.dll",
                    WebSitePath = "C:\\dev\\dotvvm\\src\\DotVVM.Samples.Compiler.Net461.Owin",
                    OutputResolvedDothtmlMap = true,
                    CheckBindingErrors = true,
                    SerializeConfig = true,
                },
                WaitForDebugger = false,
                WaitForDebuggerAndBreak = false
            };

            DotvvmCompilerLauncher.Start(opts);
        }
    }

    public class DotvvmAppMetadataProvider
    {
        public void GetAppMetadata()
        {
            
        }
        public void GetBindingRedirects()
        {

        }
    }
}
