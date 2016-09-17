using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class AddControlCommand : CommandBase
    {
        public override string Name => "Add Control";

        public override string Usage => "dotvvm add control <NAME>\ndotvvm ac <NAME>";

        public override bool CanHandle(Arguments args)
        {
            if (string.Equals(args[0], "add", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "control", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(2);
                return true;
            }

            if (string.Equals(args[0], "ac", StringComparison.CurrentCultureIgnoreCase))
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

            var baseType = args.HasOption("-c", "-code");

            // TODO: create a new master page (with the master page)
        }
    }
}
