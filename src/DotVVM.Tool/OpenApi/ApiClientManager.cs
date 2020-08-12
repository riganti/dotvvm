using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Cli;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.TypeScript;

namespace DotVVM.Tool.OpenApi
{
    // this logic should be moved to a script file that would allow anyone to edit or replace the logic easily
    public static class ApiClientManager
    {
        public static ProjectMetadata AddApiClient(
            Uri swaggerFile,
            string @namespace,
            string csharpOutput,
            string typescriptOutput,
            ProjectMetadata project)
        {
            var definition = new ApiClientDefinition {
                CSharpClient = csharpOutput,
                TypescriptClient = typescriptOutput,
                SwaggerFile = swaggerFile,
                Namespace = @namespace
            };

            RegenApiClient(definition, promptOnFileOverwrite: true).Wait();
            return project.WithApiClients(project.ApiClients.Add(definition));
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

        public static (bool isSingleClient, string typeName) GenerateCSharp(
            OpenApiDocument document,
            ApiClientDefinition definition,
            bool promptOnFileOverwrite)
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

            settings.CSharpGeneratorSettings.TypeNameGenerator = new DotvmmCSharpTypeNameGenerator(
                settings.CSharpGeneratorSettings,
                document);
            settings.CSharpGeneratorSettings.TemplateFactory = new DotvvmClientTemplateFactory(
                settings.CodeGeneratorSettings,
                new[] {
                    typeof(CSharpGeneratorSettings).Assembly,
                    typeof(CSharpClientGeneratorSettings).Assembly
                });

            var resolver = new CSharpTypeResolver(settings.CSharpGeneratorSettings);
            var generator = new DotvvmSwaggerToCSharpClientGenerator(document, settings, resolver);
            var csharp = generator.GenerateFile();
            
            if (definition.GenerateWrapperClass && !definition.IsSingleClient)
            {
                csharp = ApiClientUtils.InjectWrapperClass(csharp, className, clientNames);
            }

            File.WriteAllText(definition.CSharpClient, csharp);

            return (definition.IsSingleClient, className);
        }

        public static void GenerateTS(
            OpenApiDocument document,
            ApiClientDefinition definition,
            bool promptOnFileOverwrite)
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
