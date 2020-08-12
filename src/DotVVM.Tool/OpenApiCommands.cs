using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using DotVVM.Cli;
using DotVVM.Tool.OpenApi;

namespace DotVVM.Tool
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

        public static void HandleCreate(
            ProjectMetadata metadata,
            Uri definition,
            string? @namespace,
            FileInfo? csPath,
            FileInfo? tsPath)
        {
            @namespace ??= metadata.RootNamespace;
            string name = Names.GetClass(definition.Segments.LastOrDefault() ?? DefaultClientName);
            if (!name.EndsWith(DefaultClientName))
            {
                name += DefaultClientName;
            }
            csPath ??= new FileInfo($"{name}.cs");
            tsPath ??= new FileInfo($"{name}.ts");
            ApiClientManager.AddApiClient(
                definition,
                @namespace,
                csPath.FullName,
                tsPath.FullName,
                metadata);
        }

        public static void HandleRegen()
        {

        }
    }
}
