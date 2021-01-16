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
            var createDefinitionArg = new Argument<Uri>(
                name: "definition",
                getDefaultValue: () => new Uri(Path.Combine(".", DefaultDefinitionFile)),
                description: "Path or an URL to an OpenApi file");
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

            var apiCmd = new Command("api", "Manage REST API clients");
            apiCmd.AddTargetArgument();
            apiCmd.Add(createCmd);
            command.AddCommand(apiCmd);
        }

        public static async Task<int> HandleCreate(
            DotvvmProject metadata,
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
            var projectDir = Path.GetDirectoryName(metadata.ProjectFilePath)!;
            csPath ??= new FileInfo(Path.Combine(projectDir, $"{name}.cs"));
            tsPath ??= new FileInfo(Path.Combine(projectDir, $"{name}.ts"));
            var clientDefinition = new ApiClientDefinition {
                CSharpClient = csPath.FullName,
                TypescriptClient = tsPath.FullName,
                SwaggerFile = definition,
                Namespace = @namespace
            };
            await ApiClientManager.RegenApiClient(clientDefinition, logger);
            return 0;
        }
    }
}
