using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine;
using DotVVM.CommandLine.OpenApi;
using Microsoft.Extensions.Logging;

namespace DotVVM.CommandLine
{
    public static class OpenApiCommands
    {
        public const string DefaultDefinitionFile = "openapi.json";
        public const string DefaultClientName = "ApiClient";

        public static void AddOpenApiCommands(this Command command)
        {
            var targetArg = new Argument<FileSystemInfo>(
                name: CommandLineExtensions.TargetArg,
                getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory),
                description: "Path to a DotVVM project");
            var createDefinitionArg = new Argument<Uri>(
                name: "definition",
                getDefaultValue: () => new Uri(Path.Combine(Environment.CurrentDirectory, DefaultDefinitionFile)),
                description: "Path or an URL to an OpenApi file");
            var regenSwaggerArg = new Argument<string>(
                name: "definition",
                description: "Path or an URL to a OpenApi file that is to be refreshed")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            var namespaceOpt = new Option<string>(
                alias: "--namespace",
                description: "The namespace of the generated C# API client");
            var csPathOpt = new Option<FileInfo>(
                alias: "--cs-path",
                description: "Path of the generated C# client");
            var tsPathOpt = new Option<FileInfo>(
                alias: "--ts-path",
                description: "Path of the generated TypeScript client");

            var createCmd = new Command("create", "Create a REST API client")
            {
                createDefinitionArg, namespaceOpt, csPathOpt, tsPathOpt
            };
            createCmd.Handler = CommandHandler.Create(typeof(OpenApiCommands).GetMethod(nameof(HandleCreate))!);

            var regenCmd = new Command("regen", "Regenerate one or all REST API clients")
            {
                regenSwaggerArg
            };
            regenCmd.Handler = CommandHandler.Create(typeof(OpenApiCommands).GetMethod(nameof(HandleRegen))!);

            var apiCmd = new Command("api", "Manage REST API clients")
            {
                targetArg,
                createCmd,
                regenCmd
            };
            command.AddCommand(apiCmd);
        }

        public static async Task<int> HandleCreate(
            ProjectMetadataOld metadata,
            Uri definition,
            string? @namespace,
            FileInfo? csPath,
            FileInfo? tsPath,
            ILogger logger)
        {
            if (!definition.IsAbsoluteUri)
            {
                definition = new Uri(Path.Combine(Environment.CurrentDirectory, definition.OriginalString));
            }

            if (definition.IsFile && !File.Exists(definition.AbsolutePath))
            {
                logger.LogCritical($"Definition at '{definition.AbsolutePath}' does not exist.");
                return 1;
            }

            @namespace ??= metadata.RootNamespace;
            string name = Path.GetFileNameWithoutExtension(definition.Segments.LastOrDefault()) ?? DefaultClientName;
            name = Names.GetClass(name);
            if (!name.EndsWith(DefaultClientName))
            {
                name += DefaultClientName;
            }
            csPath ??= new FileInfo(Path.Combine(metadata.ProjectDirectory, $"{name}.cs"));
            tsPath ??= new FileInfo(Path.Combine(metadata.ProjectDirectory, $"{name}.ts"));
            metadata = ApiClientManager.AddApiClient(
                definition,
                @namespace,
                csPath.FullName,
                tsPath.FullName,
                metadata,
                logger);
            throw new NotImplementedException("TODO: Implement ApiClient saving mechanism");
            return 0;
        }

        public static async Task<int> HandleRegen(
            ProjectMetadataOld metadata,
            Uri? definition,
            ILogger logger)
        {
            if (definition is null)
            {
                await Task.WhenAll(metadata.ApiClients
                    .Select(c => ApiClientManager.RegenApiClient(c, logger))
                    .ToArray());
                return 0;
            }

            if (!definition.IsAbsoluteUri)
            {
                definition = new Uri(Path.Combine(Environment.CurrentDirectory, definition.OriginalString));
            }

            if (definition.IsFile && !File.Exists(definition.AbsolutePath))
            {
                logger.LogCritical($"Definition at '{definition.AbsolutePath}' does not exist.");
                return 1;
            }

            var client = metadata.ApiClients.FirstOrDefault(c =>
                (c.SwaggerFile is object && c.SwaggerFile == definition)
                || (c.CSharpClient is object && c.CSharpClient == definition.AbsolutePath)
                || (c.TypescriptClient is object && c.TypescriptClient == definition.AbsolutePath));

            if (client is null)
            {
                logger.LogCritical($"Now API client with the '{definition.AbsolutePath}' path or URL was found.");
                return 1;
            }

            await ApiClientManager.RegenApiClient(client, logger);
            return 0;
        }
    }
}
