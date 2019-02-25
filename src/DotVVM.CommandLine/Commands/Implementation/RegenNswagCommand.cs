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
        public override string Name => "Regenerate REST API clients";

        public override string[] Usages => new []{"dotvvm api regen [ swagger metadata URL or swagger JSON path -- if not specified all of them are refreshed ]" };

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
                    throw new InvalidCommandUsageException($"No API client with the following URL or path was found: {swaggerFile}");
                ApiClientManager.RegenApiClient(apiClient, promptOnFileOverwrite: false).Wait();
            }
            else
            {
                dotvvmProjectMetadata.ApiClients
                    .Select(c => ApiClientManager.RegenApiClient(c, promptOnFileOverwrite: false))
                    .ToArray()
                    .ApplyAction(Task.WaitAll);
            }

        }
    }
}
