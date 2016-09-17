using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class AddViewModelCommand : CommandBase
    {
        public override string Name => "Add ViewModel";

        public override string Usage => "dotvvm add viewmodel <NAME>\ndotvvm avm <NAME>";

        public override bool CanHandle(Arguments args)
        {
            if (string.Equals(args[0], "add", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "viewmodel", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(2);
                return true;
            }

            if (string.Equals(args[0], "avm", StringComparison.CurrentCultureIgnoreCase))
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

            // TODO: create the viewmodel
        }
    }
}
