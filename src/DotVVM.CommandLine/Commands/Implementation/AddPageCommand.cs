using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class AddPageCommand : CommandBase
    {
        public override string Name => "Add Page";

        public override string Usage => "dotvvm add page <NAME>\ndotvvm ap <NAME>";

        public override bool CanHandle(Arguments args)
        {
            if (string.Equals(args[0], "add", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "page", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(2);
                return true;
            }

            if (string.Equals(args[0], "ap", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(1);
                return true;
            }

            return false;
        }

        public override void Handle(Arguments args)
        {
            var name = args[0];
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidCommandUsageException("You have to specify the NAME.");
            }

            var masterPage = args.GetOptionValue("-m", "-master", "-masterPage");

            // TODO: create a new page (with the master page)
        }
    }
}
