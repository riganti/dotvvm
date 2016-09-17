using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.CommandLine.Commands
{
    public abstract class CommandBase
    {
        
        public abstract string Name { get; }

        public abstract string Usage { get; }

        public abstract bool CanHandle(Arguments args);

        public abstract void Handle(Arguments args);


        protected string[] ExpandFileNames(string name)
        {
            // TODO: add wildcard support
            return new[] { name };
        }

    }
}
