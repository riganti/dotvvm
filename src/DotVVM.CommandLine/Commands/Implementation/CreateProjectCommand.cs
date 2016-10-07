using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine.Metadata;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class CreateProjectCommand : CommandBase
    {
        public override string Name => "Create Project";

        public override string Usage => "dotvvm create project <TEMPLATE> <NAME>\ndotvvm cp <TEMPLATE> <NAME>";


        public override bool CanHandle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "create", StringComparison.CurrentCultureIgnoreCase) 
                && string.Equals(args[1], "project", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(2);
                return true;
            }

            if (string.Equals(args[0], "cp", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(1);
                return true;
            }

            return false;
        }

        public override void Handle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            var template = args[0];
            if (string.IsNullOrEmpty(template))
            {
                throw new InvalidCommandUsageException("You have to specify the TEMPLATE.");
            }

            // TODO: download template from web

            var name = args[1];
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidCommandUsageException("You have to specify the NAME.");
            }

            // TODO: substitute name and other parameters
        }
    }
}