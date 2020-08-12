using System;
using DotVVM.CommandLine.Commands.Core;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.Cli;

namespace DotVVM.CommandLine.Commands.Handlers
{
    public class VersionCommandHandler : CommandBase
    {
        public override string Name => "Show CLI version";

        public override string[] Usages => new []{ "dotvvm [--version|-v]" };

        public override bool TryConsumeArgs(Arguments args, ProjectMetadataJson dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "--version", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(1);
                return true;
            }

            if (string.Equals(args[0], "-v", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(1);
                return true;
            }

            return false;
        }

        public override void Handle(Arguments args, ProjectMetadataJson dotvvmProjectMetadata)
        {
            Console.WriteLine(this.GetType().Assembly.GetName().Version);
        }
    }
}
