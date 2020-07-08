using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine.Commands.Logic;
using DotVVM.CommandLine.Commands.Templates;
using DotVVM.CommandLine.Metadata;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class AddNswagCommand : CommandBase
    {
        public override string Name => "Add REST API client";

        public override string Usage => "dotvvm api create <http://path/to/swagger.json> <Namespace> <../ApiProject/CSharpClient.cs> <Scripts/TypescriptClient.cs> --silent";

        public override bool TryConsumeArgs(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "api", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "create", StringComparison.CurrentCultureIgnoreCase))
            {
                // ahhh, yes, you are not drunk, this function that should only detect if it can be handled has side effects on the args parameter...
                args.Consume(2);
                return true;
            }

            return false;
        }

        public override void Handle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            var swaggerFile = args[0] ??
                              throw new InvalidCommandUsageException("You have to specify the swagger file.");
            if (!Uri.TryCreate(swaggerFile, UriKind.RelativeOrAbsolute, out var swaggerFileUri))
                throw new InvalidCommandUsageException($"'{swaggerFile}' is not a valid URI or filesystem path.");
            var @namespace = args[1] ??
                             throw new InvalidCommandUsageException("You have to specify the namespace.");
            var csharpFile = args[2] ??
                             throw new InvalidCommandUsageException("You have to specify the C# output file.");
            var typescriptFile = args[3] ??
                                 throw new InvalidCommandUsageException("You have to specify the TypeScript output file.");

            var isSilentMode = args.HasOption("--silent");

            ApiClientManager.AddApiClient(swaggerFileUri, @namespace, csharpFile, typescriptFile, dotvvmProjectMetadata, isSilentMode);
        }
    }
}
