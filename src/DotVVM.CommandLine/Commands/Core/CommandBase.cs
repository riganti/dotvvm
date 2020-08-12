using System.IO;
using DotVVM.Cli;
using DotVVM.CommandLine.Core.Arguments;

namespace DotVVM.CommandLine.Commands.Core
{
    public abstract class CommandBase
    {
        public abstract string Name { get; }

        public abstract string[] Usages { get; }

        public abstract bool TryConsumeArgs(Arguments args, ProjectMetadataJson dotvvmProjectMetadata);

        public abstract void Handle(Arguments args, ProjectMetadataJson dotvvmProjectMetadata);
    }
}
