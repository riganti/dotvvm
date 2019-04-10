using System;
using DotVVM.CommandLine.Commands.Core;
using DotVVM.CommandLine.Commands.Logic;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.CommandLine.Core.Metadata;

namespace DotVVM.CommandLine.Commands.Handlers
{
    public class AddNswagCommandHandler : CommandBase
    {
        public override string Name => "Add REST API client";

        public override string[] Usages => new []{ "dotvvm api create <http://path/to/swagger.json> <Namespace> <../ApiProject/CSharpClient.cs> <Scripts/TypescriptClient.cs>" };

        public override bool TryConsumeArgs(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "api", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "create", StringComparison.CurrentCultureIgnoreCase))
            {
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

            ApiClientManager.AddApiClient(swaggerFileUri, @namespace, csharpFile, typescriptFile, dotvvmProjectMetadata);
       }
    }
}
