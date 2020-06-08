using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Metadata;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.TypeScript;

namespace DotVVM.CommandLine.Commands.Logic
{
    // this logic should be moved to a script file that would allow anyone to edit or replace the logic easily
    public static class ApiClientManager
    {
        public static void AddApiClient(Uri swaggerFile, string @namespace, string csharpOutput, string typescriptOutput, DotvvmProjectMetadata project)
        {
            var definition = new ApiClientDefinition {
                CSharpClient = csharpOutput,
                TypescriptClient = typescriptOutput,
                SwaggerFile = swaggerFile,
                Namespace = @namespace
            };

            project.ApiClients.Add(definition);
            RegenApiClient(definition, promptOnFileOverwrite: true).Wait();
        }

        public static async Task RegenApiClient(ApiClientDefinition definition, bool promptOnFileOverwrite)
        {
            Console.WriteLine($"Regenerating API from {definition.SwaggerFile}");
            var document = await LoadDocument(definition.SwaggerFile);

            document.PopulateOperationIds();
            var (isSingleClient, typeName) = GenerateCSharp(document, definition, promptOnFileOverwrite);
            GenerateTS(document, definition, promptOnFileOverwrite);

            Console.WriteLine($"REST API clients generated. Place the following code snippet to your DotvvmStartup.cs: ");
            Console.WriteLine($"config.RegisterApi{(isSingleClient ? "Client" : "Group")}"
                              + $"(typeof({definition.Namespace}.{(definition.GenerateWrapperClass || isSingleClient ? typeName : " ... your client wrapper class ...")}), "
                              + $"\"{ document.BasePath ?? "... your api endpoint ..." }\", "
                              + $"\"{(definition.CompileTypescript ? Path.ChangeExtension(definition.TypescriptClient, "js") : "... path to your compiled javascript")}\", "
                              + $"\"_restApi\");");
        }

        public static (bool isSingleClient, string typeName) GenerateCSharp(SwaggerDocument document, ApiClientDefinition definition, bool promptOnFileOverwrite)
        {
            var className = Path.GetFileNameWithoutExtension(definition.CSharpClient);

            var settings = new SwaggerToCSharpClientGeneratorSettings() {
                GenerateSyncMethods = true,
                OperationNameGenerator = new CustomOperationNameGenerator(),
                GenerateOptionalParameters = true,
                CSharpGeneratorSettings = {
                    ClassStyle = CSharpClassStyle.Poco,
                    Namespace = definition.Namespace,
                    ArrayType = "System.Collections.Generic.List",
                    PropertyNameGenerator = new CustomPropertyNameGenerator(c => ConversionUtilities.ConvertToUpperCamelCase(c, true)),
                }
            };

            // detect whether there will be multiple clients or just one
            var clientNames = document.Operations
                .Select(o => settings.OperationNameGenerator.GetClientName(document, o.Path, o.Method, o.Operation))
                .Distinct()
                .ToArray();
            definition.IsSingleClient = clientNames.Length == 1;

            if (definition.IsSingleClient)
            {
                // set the class name only when Swagger generates one client, otherwise all classes would have the same name
                settings.ClassName = className;
            }

            settings.CSharpGeneratorSettings.TypeNameGenerator =
                new DotvmmCSharpTypeNameGenerator(settings.CSharpGeneratorSettings, document);
            settings.CSharpGeneratorSettings.TemplateFactory = new DotvvmClientTemplateFactory(settings.CodeGeneratorSettings, new[] {
                typeof(CSharpGeneratorSettings).Assembly,
                typeof(SwaggerToCSharpGeneratorSettings).Assembly
            });

            var resolver = SwaggerToCSharpTypeResolver.CreateWithDefinitions(settings.CSharpGeneratorSettings, document);
            var generator = new DotvvmSwaggerToCSharpClientGenerator(document, settings, resolver);
            var csharp = generator.GenerateFile();
            
            if (definition.GenerateWrapperClass && !definition.IsSingleClient)
            {
                csharp = ApiClientUtils.InjectWrapperClass(csharp, className, clientNames);
            }

            FileSystemHelpers.WriteFile(definition.CSharpClient, csharp, promptOnFileOverwrite);

            return (definition.IsSingleClient, className);
        }

        public static void GenerateTS(SwaggerDocument document, ApiClientDefinition definition, bool promptOnFileOverwrite)
        {
            var className = Path.GetFileNameWithoutExtension(definition.TypescriptClient);

            var settings = new SwaggerToTypeScriptClientGeneratorSettings() {
                Template = TypeScriptTemplate.Fetch,
                OperationNameGenerator = new CustomOperationNameGenerator(),
                GenerateOptionalParameters = true,
                UseTransformOptionsMethod = true,
                ClientBaseClass = "ClientBase",
                TypeScriptGeneratorSettings = {
                    PropertyNameGenerator = new CustomPropertyNameGenerator(c => c),
                    NullValue = TypeScriptNullValue.Null
                }
            };

            if (definition.IsSingleClient)
            {
                // set the class name only when Swagger generates one client, otherwise all classes would have the same name
                settings.ClassName = className;
            }

            settings.TypeScriptGeneratorSettings.TemplateFactory = new DotvvmClientTemplateFactory(settings.CodeGeneratorSettings, new[] {
                typeof(TypeScriptGeneratorSettings).Assembly,
                typeof(SwaggerToTypeScriptClientGeneratorSettings).Assembly
            });

            var resolver = new TypeScriptTypeResolver(settings.TypeScriptGeneratorSettings);
            var generator = new DotvvmSwaggerToTypeScriptClientGenerator(document, settings, resolver);
            var ts = generator.GenerateFile();
            var baseClass = definition.CreateBaseClass();
            ts = definition.WrapInNamespace(ts, baseClass);
            FileSystemHelpers.WriteFile(definition.TypescriptClient, ts, promptOnFileOverwrite);

            if (definition.CompileTypescript)
            {
                Process.Start(new ProcessStartInfo() {
                    FileName = "tsc",
                    Arguments = definition.TypescriptClient,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            }
        }

        private static Task<SwaggerDocument> LoadDocument(Uri swaggerUri)
        {
            if (!swaggerUri.IsAbsoluteUri)
            {
                return SwaggerDocument.FromFileAsync(swaggerUri.ToString());
            }
            else if (swaggerUri.Scheme == "file")
            {
                return SwaggerDocument.FromFileAsync(swaggerUri.LocalPath);
            }

            return SwaggerDocument.FromUrlAsync(swaggerUri.ToString());
        }
    }
}
