using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.CommandLine;
using DotVVM.CommandLine.OpenApi;
using Microsoft.Extensions.Logging;

namespace DotVVM.CommandLine
{
    public static class OpenApiCommands
    {
        public const string DefaultClientName = "ApiClient";
        public const string DefaultConfigName = "dotvvm-api.json";

        public static void AddOpenApiCommands(this Command command)
        {
            var createDefinitionArg = new Argument<Uri>("definition")
            {
                Description = "Path or a URL to an OpenAPI definition"
            };
            var namespaceOpt = new Option<string>("--namespace")
            {
                Description = "The namespace of the generated C# API client"
            };
            var csPathOpt = new Option<FileInfo>("--cs-path")
            {
                Description = "Path to the generated C# client"
            };
            var tsPathOpt = new Option<FileInfo>("--ts-path")
            {
                Description = "Path to the generated TypeScript client"
            };
            var configOpt = new Option<FileInfo>("--config")
            {
                Description = "Path to the DotVVM API configuration JSON"
            };
            var noConfigOpt = new Option<bool>("--no-config")
            {
                Description = "Disable the DotVVM API configuration JSON"
            };

            var createCmd = new Command("create", "Create a REST API client")
            {
                createDefinitionArg, namespaceOpt, csPathOpt, tsPathOpt
            };
            createCmd.SetAction(async parseResult =>
            {
                if (!CommandLineExtensions.TryGetProject(parseResult, out var project, out var logger))
                    return 1;

                return await HandleCreate(
                    project,
                    parseResult.GetRequiredValue(createDefinitionArg),
                    parseResult.GetValue(namespaceOpt),
                    parseResult.GetValue(csPathOpt),
                    parseResult.GetValue(tsPathOpt),
                    parseResult.GetValue(configOpt),
                    parseResult.GetValue(noConfigOpt),
                    logger);
            });


            var regenDefinitionArg = new Argument<Uri>("definition")
            {
                Description = "Path or a URL to an OpenAPI file"
            };
            regenDefinitionArg.Arity = ArgumentArity.ZeroOrOne;

            var regenCmd = new Command("regen", "Regenerate a specific API client or all of them")
            {
                regenDefinitionArg, configOpt
            };
            regenCmd.SetAction(async parseResult =>
            {
                if (!CommandLineExtensions.TryGetProject(parseResult, out var project, out var logger))
                    return 1;

                return await HandleRegen(
                    project,
                    parseResult.GetValue(regenDefinitionArg),
                    parseResult.GetValue(configOpt),
                    logger);
            });

            var apiCmd = new Command("api", "Manage REST API clients");
            apiCmd.AddTargetArgument();
            apiCmd.Add(createCmd);
            apiCmd.Add(regenCmd);
            command.Subcommands.Add(apiCmd);
        }

        public static async Task<int> HandleCreate(
            DotvvmProject metadata,
            Uri definition,
            string? @namespace,
            FileInfo? csPath,
            FileInfo? tsPath,
            FileInfo? config,
            bool noConfig,
            ILogger logger)
        {
            if (!TryValidateDefinition(ref definition, logger))
            {
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
                CSharpClient = Path.GetRelativePath(projectDir, csPath.FullName),
                TypescriptClient = Path.GetRelativePath(projectDir, tsPath.FullName),
                SwaggerFile = definition,
                Namespace = @namespace
            };

            if (!noConfig)
            {
                config ??= new FileInfo(Path.Combine(
                    projectDir ?? Directory.GetCurrentDirectory(),
                    DefaultConfigName));
                var exists = config.Exists;
                using var stream = config.Open(FileMode.OpenOrCreate);
                var clients = exists
                    ? await JsonSerializer.DeserializeAsync<List<ApiClientDefinition>>(stream)
                        ?? new List<ApiClientDefinition>()
                    : new List<ApiClientDefinition>();
                clients.Add(clientDefinition);
                stream.Seek(0, SeekOrigin.Begin);
                await JsonSerializer.SerializeAsync(stream, clients);
            }

            if (projectDir is {})
                Directory.SetCurrentDirectory(projectDir);
            await ApiClientManager.RegenApiClient(clientDefinition, logger);
            return 0;
        }

        public static async Task<int> HandleRegen(
            DotvvmProject metadata,
            Uri? definition,
            FileInfo? config,
            ILogger logger)
        {
            // TODO: A proper validity check of the API JSON.

            var projectDir = Path.GetDirectoryName(metadata.ProjectFilePath)!;
            config ??= new FileInfo(Path.Combine(
                projectDir ?? Directory.GetCurrentDirectory(),
                DefaultConfigName));
            if (!config.Exists)
            {
                logger.LogError($"No API clients can be regenerated from '{config}' because it doesn't exist.");
                return 1;
            }

            var json = File.ReadAllText(config.FullName);
            var clients = JsonSerializer.Deserialize<ApiClientDefinition[]>(json) ?? Array.Empty<ApiClientDefinition>();

            if (definition is object)
            {
                if (!TryValidateDefinition(ref definition, logger))
                {
                    return 1;
                }

                clients = clients.Where(c => c.SwaggerFile == definition)
                    .ToArray();
                if (clients.Length == 0)
                {
                    logger.LogError($"'{config}' doesn't contain an API client definition for '{definition}'.");
                    return 1;
                }
            }

            if (projectDir is {})
                Directory.SetCurrentDirectory(projectDir);
            foreach(var client in clients)
            {
                await ApiClientManager.RegenApiClient(client, logger);
            }
            return 0;
        }

        private static bool TryValidateDefinition(ref Uri definition, ILogger logger)
        {
            if (!definition.IsAbsoluteUri)
            {
                definition = new Uri(Path.Combine(Environment.CurrentDirectory, definition.OriginalString));
            }

            if (definition.IsFile && !File.Exists(definition.AbsolutePath))
            {
                logger.LogCritical($"Definition at '{definition.AbsolutePath}' does not exist.");
                return false;
            }
            return true;
        }
    }
}
