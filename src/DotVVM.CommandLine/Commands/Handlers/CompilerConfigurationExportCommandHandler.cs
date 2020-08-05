using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Cli;
using DotVVM.CommandLine.Commands.Core;
using DotVVM.CommandLine.Commands.Logic.Compiler;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.CommandLine.Core.Metadata;
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
            var projectsMetadata = new ProjectSystemProvider().GetProjectMetadata(dotvvmProjectMetadata.ProjectDirectory).ToList();
            WriteVerboseInformation(projectsMetadata);

            foreach (var meta in projectsMetadata)
            {
                var opts = new CompilerStartupOptions() {
                    Options = new CompilerOptions {
                        DothtmlFiles = null,
                        AssemblyName = meta.AssemblyName,
                        WebSiteAssembly = meta.AssemblyPath,
                        WebSitePath = meta.ProjectRootDirectory,
                        OutputResolvedDothtmlMap = true,
                        CheckBindingErrors = true,
                        SerializeConfig = true,
                    },
                    WaitForDebugger = false,
                    WaitForDebuggerAndBreak = false,
                };
                DotvvmCompilerLauncher.Start(opts, meta);
            }

        }
      

        private static void WriteVerboseInformation(List<IResolvedProjectMetadata> results)
        {
            Console.WriteLine($"Found {results.Count} project{(results.Count > 1 ? "s" : "")}.");
            foreach (var result in results)
            {
                Console.WriteLine($">  {result.CsprojFullName}");
            }
        }
    }
}
