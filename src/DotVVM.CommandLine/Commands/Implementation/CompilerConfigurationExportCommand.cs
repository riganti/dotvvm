using System;
using DotVVM.CommandLine.Metadata;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class CompilerConfigurationExportCommand : CommandBase
    {
        public override string Name => "Export DotVVM configuration";
        public override string[] Usages => new[] { "compiler export-config" };

        public override bool TryConsumeArgs(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata) =>
            args[0] == "compiler" && args[1] == "export-config";

        public override void Handle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            //TODO: resolve arguments for compiler and start process
            throw new NotImplementedException();
        }
    }
}
