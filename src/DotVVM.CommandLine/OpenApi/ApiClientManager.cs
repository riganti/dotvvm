using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.TypeScript;

namespace DotVVM.CommandLine.OpenApi
{
    // this logic should be moved to a script file that would allow anyone to edit or replace the logic easily
    public static class ApiClientManager
    {
        public static async Task RegenApiClient(
            ApiClientDefinition definition,
            ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

            logger.LogInformation($"Loading API from '{definition.SwaggerFile}'.");
            var document = await LoadDocument(definition.SwaggerFile);

            document.PopulateOperationIds();
            logger.LogInformation($"Generating '{definition.CSharpClient}'.");
            var (isSingleClient, typeName) = GenerateCSharpClient(document, definition);
            logger.LogInformation($"Generating '{definition.TypescriptClient}'.");
            GenerateTypeScriptClient(document, definition);

            var snippet = $"config.RegisterApi{(isSingleClient ? "Client" : "Group")}"
                + $"(typeof({definition.Namespace}.{(definition.GenerateWrapperClass || isSingleClient ? typeName : " ... your client wrapper class ...")}), "
                + $"\"{ document.BasePath ?? "Your API endpoint" }\", "
                + $"\"{(definition.CompileTypescript ? Path.ChangeExtension(definition.TypescriptClient, "js") : "... path to your compiled javascript")}\", "
                + $"\"_restApi\");";
            logger.LogInformation($"Place the following in your DotvvmStartup: '{snippet}'.");
        }

        public static (bool isSingleClient, string typeName) GenerateCSharpClient(
            OpenApiDocument document,
            ApiClientDefinition definition)
        {
            var className = Path.GetFileNameWithoutExtension(definition.CSharpClient);

            var settings = new CSharpClientGeneratorSettings {
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

            settings.CSharpGeneratorSettings.TypeNameGenerator = new DotvvmCSharpTypeNameGenerator(
                settings.CSharpGeneratorSettings,
                document);
            settings.CSharpGeneratorSettings.TemplateFactory = new DotvvmClientTemplateFactory(
                settings.CodeGeneratorSettings,
                new[] {
                    typeof(CSharpGeneratorSettings).Assembly,
                    typeof(CSharpClientGeneratorSettings).Assembly
                });

            var resolver = new CSharpTypeResolver(settings.CSharpGeneratorSettings);
            var generator = new DotvvmCSharpClientGenerator(document, settings, resolver);
            var csharp = generator.GenerateFile();

            if (definition.GenerateWrapperClass && !definition.IsSingleClient)
            {
                csharp = ApiClientUtils.InjectWrapperClass(csharp, className, clientNames);
            }

            File.WriteAllText(definition.CSharpClient, csharp);

            return (definition.IsSingleClient, className);
        }

        public static void GenerateTypeScriptClient(
            OpenApiDocument document,
            ApiClientDefinition definition)
        {
            var className = Path.GetFileNameWithoutExtension(definition.TypescriptClient);

            var settings = new TypeScriptClientGeneratorSettings {
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
                typeof(TypeScriptClientGeneratorSettings).Assembly
            });

            var resolver = new TypeScriptTypeResolver(settings.TypeScriptGeneratorSettings);
            var generator = new DotvvmTypeScriptClientGenerator(document, settings, resolver);
            var ts = generator.GenerateFile();
            var baseClass = definition.CreateBaseClass();
            ts = definition.WrapInNamespace(ts, baseClass);
            File.WriteAllText(definition.TypescriptClient, ts);
        }

        private static Task<OpenApiDocument> LoadDocument(Uri swaggerUri)
        {
            if (!swaggerUri.IsAbsoluteUri)
            {
                return OpenApiDocument.FromFileAsync(swaggerUri.ToString());
            }
            else if (swaggerUri.Scheme == "file")
            {
                return OpenApiDocument.FromFileAsync(swaggerUri.LocalPath);
            }

            return OpenApiDocument.FromUrlAsync(swaggerUri.ToString());
        }
    }
}
