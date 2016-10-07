using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine.Metadata;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class AddMasterPageCommand : CommandBase
    {
        public override string Name => "Add Master Page";

        public override string Usage => "dotvvm add master <NAME>\ndotvvm am <NAME>";

        public override bool CanHandle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "add", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "master", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(2);
                return true;
            }

            if (string.Equals(args[0], "am", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(1);
                return true;
            }

            return false;
        }

        public override void Handle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            var name = args[0];
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidCommandUsageException("You have to specify the NAME.");
            }

            var masterPage = args.GetOptionValue("-m", "-master", "-masterPage");

            // TODO: create a new master page (with the master page)
        }
    }
}
