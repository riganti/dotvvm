using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine.Core.Metadata;
using DotVVM.CommandLine.Metadata;

namespace DotVVM.CommandLine.Commands
{
    public abstract class CommandBase
    {
        
        public abstract string Name { get; }

        public abstract string Usage { get; }

        public abstract bool TryConsumeArgs(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata);

        public abstract void Handle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata);


        protected string[] ExpandFileNames(string name)
        {
            // TODO: add wildcard support
            return new[] { Path.GetFullPath(name) };
        }

    }
}
