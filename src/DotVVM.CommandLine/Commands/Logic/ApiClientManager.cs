using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.CommandLine.Metadata;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;
using NSwag.CodeGeneration.TypeScript;

namespace DotVVM.CommandLine.Commands.Logic
{
    // this logic should be moved to a script file that would allow anyone to edit or replace the logic easily
    public static class ApiClientManager
    {
        public static void AddApiClient(Uri swaggerFile, string @namespace, string csharpoutput, string typescriptOutput, DotvvmProjectMetadata project)
        {
            var definition = new ApiClientDefinition
            {
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
            var document =
                !definition.SwaggerFile.IsAbsoluteUri ? await SwaggerDocument.FromFileAsync(definition.SwaggerFile.ToString()) :
                definition.SwaggerFile.Scheme == "file" ? await SwaggerDocument.FromFileAsync(definition.SwaggerFile.LocalPath) :
                await SwaggerDocument.FromUrlAsync(definition.SwaggerFile.ToString());
            PopulateOperationIds(document);
            var (isSingleClient, typeName) = GenerateCSharp(document, definition);
            GenerateTS(document, definition);

            Console.WriteLine($"API clients generated. Place the following code snippet to your DotvvmStartup.cs: ");
            Console.WriteLine($"config.RegisterApi{(isSingleClient ? "Client" : "Group")}(typeof({definition.Namespace}.{(definition.GenerateWrapperClass || isSingleClient ? typeName : " ... your client wrapper class ...")}), \"{ document.BasePath ?? "... your api endpoint ..." }\", \"{(definition.CompileTypescript ? Path.ChangeExtension(definition.TypescriptClient, "js") : "... path to your compiled javascript")}\");");
        }


        public static (bool isSingleClient, string typeName) GenerateCSharp(SwaggerDocument document, ApiClientDefinition definition)
        {
            var nameGenerator = new CustomNameGenerator();
            var settings = new SwaggerToCSharpClientGeneratorSettings()
            {
                GenerateSyncMethods = true,
                OperationNameGenerator = nameGenerator,
                GenerateOptionalParameters = true
            };
            settings.CSharpGeneratorSettings.ClassStyle = CSharpClassStyle.Poco;
            settings.CSharpGeneratorSettings.Namespace = definition.Namespace;
            settings.CSharpGeneratorSettings.ArrayType = "System.Collections.Generic.List";
            settings.CSharpGeneratorSettings.PropertyNameGenerator =
                new MyPropertyNameGenerator(c => ConversionUtilities.ConvertToUpperCamelCase(c, true));
            settings.CSharpGeneratorSettings.TypeNameGenerator =
                new DotvmmCSharpTypeNameGenerator(settings.CSharpGeneratorSettings);
            settings.CSharpGeneratorSettings.TemplateFactory = new DotvvmClientTemplateFactory(settings.CodeGeneratorSettings, new [] {
                typeof(CSharpGeneratorSettings).Assembly,
                typeof(SwaggerToCSharpGeneratorSettings).Assembly
            });
            
            var resolver = SwaggerToCSharpTypeResolver.CreateWithDefinitions(settings.CSharpGeneratorSettings, document);
            var generator = new DotvvmSwaggerToCSharpClientGenerator(document, settings, resolver);
            var csharp = generator.GenerateFile();

            var newClient = InjectWrapperClass(csharp, Path.GetFileNameWithoutExtension(definition.CSharpClient), document, settings.OperationNameGenerator, out var isSingleClient, out var wrapperTypeName);
            definition.IsSingleClient = isSingleClient;

            if (definition.GenerateWrapperClass)
                csharp = newClient;

            File.WriteAllText(definition.CSharpClient, csharp);

            return (isSingleClient, wrapperTypeName);
        }

        private static string InjectWrapperClass(string csharpCode, string className, SwaggerDocument document, IOperationNameGenerator nameGenerator, out bool isSinlgeClient, out string clientName)
        {
            var clients = document.Operations.Select(o => nameGenerator.GetClientName(document, o.Path, o.Method, o.Operation)).Distinct().ToArray();
            if (clients.Length == 1)
            {
                isSinlgeClient = true;
                clientName = clients[0] + "Client";
                return csharpCode;
            }
            else
            {
                isSinlgeClient = false;
                clientName = className;
                var properties = from c in clients
                                 let name = ConversionUtilities.ConvertToUpperCamelCase(c, true)
                                 let type = name + "Client"
                                 select $"public {type} {(string.IsNullOrEmpty(name) ? "Root" : name)} {{ get; set; }}";
                var wrapperClass = $@"    public class {className}
{{
    {ConversionUtilities.Tab(string.Join("\n", properties), 1)}
}}".Replace("\r\n", "\n");
                var namespaceClosing = csharpCode.LastIndexOf('}');
                return csharpCode.Insert(namespaceClosing, ConversionUtilities.Tab(wrapperClass, 1) + "\n");
            }
        }

        public static void GenerateTS(SwaggerDocument document, ApiClientDefinition definition)
        {
            var settings = new SwaggerToTypeScriptClientGeneratorSettings()
            {
                Template = TypeScriptTemplate.Fetch,
                OperationNameGenerator = new CustomNameGenerator(),
                GenerateOptionalParameters = true,
                UseTransformOptionsMethod = true,
                ClientBaseClass = "ClientBase"
            };
            settings.TypeScriptGeneratorSettings.PropertyNameGenerator = new MyPropertyNameGenerator(c => ConversionUtilities.ConvertToLowerCamelCase(c, true));
            settings.TypeScriptGeneratorSettings.NullValue = TypeScriptNullValue.Null;
            settings.TypeScriptGeneratorSettings.TemplateFactory = new DotvvmClientTemplateFactory(settings.CodeGeneratorSettings, new[] {
                typeof(TypeScriptGeneratorSettings).Assembly,
                typeof(SwaggerToTypeScriptClientGeneratorSettings).Assembly
            });

            var resolver = new TypeScriptTypeResolver(settings.TypeScriptGeneratorSettings);
            var generator = new DotvvmSwaggerToTypeScriptClientGenerator(document, settings, resolver);
            var ts = generator.GenerateFile();
            var baseClass = CreateBaseClass(definition);
            ts = WrapInNamespace(definition, ts, baseClass);
            File.WriteAllText(definition.TypescriptClient, ts);

            if (definition.CompileTypescript)
            {
                Process.Start("tsc", definition.TypescriptClient).WaitForExit();
            }
        }

        private static string CreateBaseClass(ApiClientDefinition definition)
        {
            return $@"class ClientBase {{
    public transformOptions(options: RequestInit) {{
        options.credentials = ""{definition.FetchOptions.Credentials}"";
        return Promise.resolve(options);
    }}
}}";
        }

        private static string WrapInNamespace(ApiClientDefinition definition, string typescript, string baseClass)
        {
            return "namespace " + definition.Namespace + " {\n    " + ConversionUtilities.Tab(baseClass, 1).TrimEnd('\n') + "\n    " + ConversionUtilities.Tab(typescript, 1).TrimEnd('\n') + "\n}\n";
        }


        private static void PopulateOperationIds(SwaggerDocument d)
        {
            // Generate missing IDs
            foreach (var operation in d.Operations.Where(o => string.IsNullOrEmpty(o.Operation.OperationId)))
                operation.Operation.OperationId = GetOperationNameFromPath(operation);

            void consolidateGroup(string name, SwaggerOperationDescription[] operations)
            {
                if (operations.Count() == 1) return;

                // Append "All" if possible
                if (!name.EndsWith("All") && !d.Operations.Any(n => n.Operation.OperationId == name + "All"))
                {
                    var arrayResponseOperations = operations.Where(
                            a => a.Operation.Responses.Any(r => HttpUtilities.IsSuccessStatusCode(r.Key) && r.Value.ActualResponseSchema != null && r.Value.ActualResponseSchema.Type == JsonObjectType.Array)).ToArray();

                    foreach (var op in arrayResponseOperations)
                    {
                        op.Operation.OperationId = name + "All";
                    }
                    if (arrayResponseOperations.Length > 0)
                    {
                        consolidateGroup(name + "All", arrayResponseOperations);
                        consolidateGroup(name, operations.Except(arrayResponseOperations).ToArray());
                        return;
                    }
                }

                // Add numbers
                var i = 2;
                foreach (var operation in operations.Skip(1))
                {
                    while (d.Operations.Any(o => o.Operation.OperationId == name + i)) i++;
                    operation.Operation.OperationId = name + i++;
                }
            }

            // Find non-unique operation IDs
            foreach (var group in d.Operations.GroupBy(o => o.Operation.OperationId))
            {
                var operations = group.ToList();
                consolidateGroup(group.Key, group.ToArray());
            }
        }

        private static string GetOperationNameFromPath(SwaggerOperationDescription operation)
        {
            var pathSegments = operation.Path.Trim('/').Split('/').Where(s => !s.Contains('{')).ToArray();
            var lastPathSegment = pathSegments.LastOrDefault();
            var path = string.Concat(pathSegments.Take(pathSegments.Length - 1).Select(s => s + "_"));
            return path + operation.Method.ToString()[0].ToString().ToUpper() + operation.Method.ToString().Substring(1).ToLower() + ConversionUtilities.ConvertToUpperCamelCase(lastPathSegment.Replace('_', '-'), false);
        }
    }

    public class DotvmmCSharpTypeNameGenerator : DefaultTypeNameGenerator
    {
        private readonly CSharpGeneratorSettings settings;

        public DotvmmCSharpTypeNameGenerator(CSharpGeneratorSettings settings)
        {
            this.settings = settings;
        }

        public override string Generate(JsonSchema4 schema, string typeNameHint, IEnumerable<string> reservedTypeNames)
        {
            var name = base.Generate(schema, typeNameHint, reservedTypeNames);

            if (name.StartsWith("GridViewDataSetOf"))
            {
                var type = "DotVVM.Framework.Controls.GridViewDataSet<" + name.Substring("GridViewDataSetOf".Length) + ">";
                settings.ExcludedTypeNames = (settings.ExcludedTypeNames ?? new string[] { }).Concat(new [] { type }).ToArray();
                return type;
            }

            return name;
        }
    }

    public class MyPropertyNameGenerator : IPropertyNameGenerator
    {
        private readonly Func<string, string> editCasing;

        public MyPropertyNameGenerator(Func<string, string> editCasing)
        {
            this.editCasing = editCasing;
        }

        public string Generate(JsonProperty property)
        {
            if (!property.Name.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_' || c == '+'))
                // crazy property name (like `+1` and `-1` in github), encode it in hex
                return editCasing("prop_" + BitConverter.ToString(Encoding.UTF8.GetBytes(property.Name)));
            else return editCasing(
                    property.Name.Replace("@", "")
                    .Replace(".", "-")
                    .Replace("+", "Plus"))
                    .Replace('-', '_');
        }
    }

    /// <summary>Generates multiple clients and operation names based on the Swagger operation ID (underscore separated).</summary>
    public class CustomNameGenerator : IOperationNameGenerator
    {
        /// <summary>Gets a value indicating whether the generator supports multiple client classes.</summary>
        public bool SupportsMultipleClients { get; } = true;

        /// <summary>Gets the client name for a given operation (may be empty).</summary>
        /// <param name="document">The Swagger document.</param>
        /// <param name="path">The HTTP path.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="operation">The operation.</param>
        /// <returns>The client name.</returns>
        public string GetClientName(SwaggerDocument document, string path, SwaggerOperationMethod httpMethod, SwaggerOperation operation)
        {
            return GetClientName(operation);
        }

        /// <summary>Gets the operation name for a given operation.</summary>
        /// <param name="document">The Swagger document.</param>
        /// <param name="path">The HTTP path.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="operation">The operation.</param>
        /// <returns>The operation name.</returns>
        public string GetOperationName(SwaggerDocument document, string path, SwaggerOperationMethod httpMethod, SwaggerOperation operation)
        {
            var clientName = GetClientName(operation);
            var operationName = GetOperationName(operation);

            var hasOperationWithSameName = document.Operations
                .Where(o => o.Operation != operation)
                .Any(o => GetClientName(o.Operation) == clientName && GetOperationName(o.Operation) == operationName);

            if (hasOperationWithSameName)
            {
                if (operationName.ToLowerInvariant().StartsWith("get"))
                {
                    var isArrayResponse = operation.Responses.ContainsKey("200") &&
                                          operation.Responses["200"].ActualResponseSchema != null &&
                                          operation.Responses["200"].ActualResponseSchema.Type.HasFlag(JsonObjectType.Array);

                    if (isArrayResponse)
                        return "GetAll" + operationName.Substring(3);
                }
            }

            return operationName;
        }

        private string GetClientName(SwaggerOperation operation)
        {
            var segments = operation.OperationId.Split('_').ToArray();
            return segments.Length >= 2 ? segments[0] : string.Empty;
        }

        private string GetOperationName(SwaggerOperation operation)
        {
            var segments = operation.OperationId.Split('_').ToArray();
            if (segments.Length >= 2) segments = segments.Skip(1).ToArray();
            return segments.Length > 0 ? ConversionUtilities.ConvertToUpperCamelCase(string.Join("-", segments), true) : "Index";
        }
    }
}
