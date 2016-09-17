using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class GenerateUiTestStubCommand : CommandBase
    {
        public override string Name => "Generate UI Test Stub";

        public override string Usage => "dotvvm gen uitest <NAME>\ndotvvm gut <NAME>";

        public override bool CanHandle(Arguments args)
        {
            if (string.Equals(args[0], "gen", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "uitest", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(2);
                return true;
            }

            if (string.Equals(args[0], "gut", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(1);
                return true;
            }

            return false;
        }

        public override void Handle(Arguments args)
        {
            var name = args[0];
            var files = ExpandFileNames(name);

            foreach (var file in files)
            {
                Console.WriteLine($"Generating stub for {file}...");


            }
        }

    }
}
