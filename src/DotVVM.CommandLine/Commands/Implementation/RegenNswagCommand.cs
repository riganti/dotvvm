using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine.Commands.Logic;
using DotVVM.CommandLine.Commands.Templates;
using DotVVM.CommandLine.Metadata;
using DotVVM.Framework.Utils;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class RegenNswagCommand : CommandBase
    {
        public override string Name => "Add Control";

        public override string Usage => "dotvvm api regen [ swagger path -- if not specified all of them are refreshed ]";

        public override bool CanHandle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "api", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "regen", StringComparison.CurrentCultureIgnoreCase))
            {
                // ahhh, yes, you are not drunk, this function that should only detect if it can be handled has side effects on the args parameter...
                args.Consume(2);
                return true;
            }

            return false;
        }

        public override void Handle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            var swaggerFile = args[0];
            if (swaggerFile != null)
            {
                if (!Uri.TryCreate(swaggerFile, UriKind.RelativeOrAbsolute, out var swaggerFileUri))
                    throw new InvalidCommandUsageException($"'{swaggerFile}' is not a valid uri.");
                ApiClientManager.RegenApiClient(
                    dotvvmProjectMetadata.ApiClients.FirstOrDefault(a => a.SwaggerFile == swaggerFileUri) ??
                        throw new InvalidCommandUsageException($"No registered api client with with '{swaggerFile}' was found.")
                ).Wait();
            }
            else
            {
                dotvvmProjectMetadata.ApiClients
                    .Select(ApiClientManager.RegenApiClient)
                    .ToArray()
                    .ApplyAction(Task.WaitAll);
            }

        }
    }
}
