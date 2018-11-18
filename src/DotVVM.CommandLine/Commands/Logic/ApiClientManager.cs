using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine.Metadata;
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
        public static void AddApiClient(Uri swaggerFile, string @namespace, string csharpoutput, string typescriptOutput, DotvvmProjectMetadata project)
        {
            var definition = new ApiClientDefinition {
                CSharpClient = csharpoutput,
                TypescriptClient = typescriptOutput,
                SwaggerFile = swaggerFile,
                Namespace = @namespace
            };

            project.ApiClients.Add(definition);
            RegenApiClient(definition).Wait();
        }

        public static async Task RegenApiClient(ApiClientDefinition definition)
        {
            Console.WriteLine($"Regenerating API from {definition.SwaggerFile}");
            var document = await LoadDocument(definition.SwaggerFile);

            document.PopulateOperationIds();
            var (isSingleClient, typeName) = GenerateCSharp(document, definition);
            GenerateTS(document, definition);

            Console.WriteLine($"API clients generated. Place the following code snippet to your DotvvmStartup.cs: ");
            Console.WriteLine($"config.RegisterApi{(isSingleClient ? "Client" : "Group")}"
                              + $"(typeof({definition.Namespace}.{(definition.GenerateWrapperClass || isSingleClient ? typeName : " ... your client wrapper class ...")}), "
                              + $"\"{ document.BasePath ?? "... your api endpoint ..." }\", "
                              + $"\"{(definition.CompileTypescript ? Path.ChangeExtension(definition.TypescriptClient, "js") : "... path to your compiled javascript")}\", "
                              + $"\"_restApi\");");
        }

        public static (bool isSingleClient, string typeName) GenerateCSharp(SwaggerDocument document, ApiClientDefinition definition)
        {
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

            settings.CSharpGeneratorSettings.TypeNameGenerator =
                new DotvmmCSharpTypeNameGenerator(settings.CSharpGeneratorSettings, document);
            settings.CSharpGeneratorSettings.TemplateFactory = new DotvvmClientTemplateFactory(settings.CodeGeneratorSettings, new[] {
                typeof(CSharpGeneratorSettings).Assembly,
                typeof(SwaggerToCSharpGeneratorSettings).Assembly
            });

            var resolver = SwaggerToCSharpTypeResolver.CreateWithDefinitions(settings.CSharpGeneratorSettings, document);
            var generator = new DotvvmSwaggerToCSharpClientGenerator(document, settings, resolver);
            var csharp = generator.GenerateFile();

            var newClient = ApiClientUtils.InjectWrapperClass(csharp, Path.GetFileNameWithoutExtension(definition.CSharpClient),
                document, settings.OperationNameGenerator, out var isSingleClient, out var wrapperTypeName);
            definition.IsSingleClient = isSingleClient;

            if (definition.GenerateWrapperClass)
                csharp = newClient;

            File.WriteAllText(definition.CSharpClient, csharp);

            return (isSingleClient, wrapperTypeName);
        }

        public static void GenerateTS(SwaggerDocument document, ApiClientDefinition definition)
        {
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

            settings.TypeScriptGeneratorSettings.TemplateFactory = new DotvvmClientTemplateFactory(settings.CodeGeneratorSettings, new[] {
                typeof(TypeScriptGeneratorSettings).Assembly,
                typeof(SwaggerToTypeScriptClientGeneratorSettings).Assembly
            });

            var resolver = new TypeScriptTypeResolver(settings.TypeScriptGeneratorSettings);
            var generator = new DotvvmSwaggerToTypeScriptClientGenerator(document, settings, resolver);
            var ts = generator.GenerateFile();
            var baseClass = definition.CreateBaseClass();
            ts = definition.WrapInNamespace(ts, baseClass);
            File.WriteAllText(definition.TypescriptClient, ts);

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
