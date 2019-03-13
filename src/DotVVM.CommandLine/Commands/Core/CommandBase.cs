using System.IO;
using DotVVM.CommandLine.Metadata;

namespace DotVVM.CommandLine.Commands.Core
{
    public abstract class CommandBase
    {
        
        public abstract string Name { get; }

        public abstract string[] Usages { get; }

        public abstract bool TryConsumeArgs(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata);

        public abstract void Handle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata);


        protected string[] ExpandFileNames(string name)
        {
            // TODO: add wildcard support
            return new[] { Path.GetFullPath(name) };
        }

    }
}
