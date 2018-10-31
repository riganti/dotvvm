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

        public override string Usage => "dotvvm api regen [ swagger path or generated file path -- if not specified all of them are refreshed ]";

        public override bool TryConsumeArgs(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "api", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "regen", StringComparison.CurrentCultureIgnoreCase))
            {
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
                var apiClient =
                    (Uri.TryCreate(swaggerFile, UriKind.Absolute, out var swaggerFileUri) ?
                    dotvvmProjectMetadata.ApiClients.FirstOrDefault(a => a.SwaggerFile == swaggerFileUri) : null) ??
                    dotvvmProjectMetadata.ApiClients.FirstOrDefault(a => a.CSharpClient == swaggerFile || a.TypescriptClient == swaggerFile);
                if (apiClient == null)
                    throw new InvalidCommandUsageException($"No api client is using {swaggerFile} url or file.");
                ApiClientManager.RegenApiClient(apiClient).Wait();
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
