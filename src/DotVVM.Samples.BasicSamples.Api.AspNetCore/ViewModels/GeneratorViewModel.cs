using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;
using NSwag.CodeGeneration.TypeScript;
using NSwag.SwaggerGeneration.WebApi;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore.ViewModels
{
    public class GeneratorViewModel : DotvvmViewModelBase
    {

        public string CSharpPath { get; set; } = "d:\\temp\\SwaggerTest\\ApiClient.cs";

        public string TSPath { get; set; } = "d:\\temp\\SwaggerTest\\ApiClient.ts";

        public string Namespace { get; set; } = "MyClientNamespace";

        public async Task GenerateCSharp()
        {
            var document = await GetSwaggerDocument();

            var settings = new SwaggerToCSharpClientGeneratorSettings() {
                GenerateSyncMethods = true,
                OperationNameGenerator = new CustomNameGenerator(),
                GenerateOptionalParameters = true,
            };
            settings.CSharpGeneratorSettings.Namespace = Namespace;
            settings.CSharpGeneratorSettings.PropertyNameGenerator = new MyPropertyNameGenerator(c => ConversionUtilities.ConvertToUpperCamelCase(c, true));


            var generator = new SwaggerToCSharpClientGenerator(document, settings);
            Context.ReturnFile(Encoding.UTF8.GetBytes(generator.GenerateFile()), "ApiClient.cs", "text/plain");
            //File.WriteAllText(CSharpPath, generator.GenerateFile());
        }

        public async Task GenerateTS()
        {
            var document = await GetSwaggerDocument();

            var settings = new SwaggerToTypeScriptClientGeneratorSettings() {
                Template = TypeScriptTemplate.Fetch,
                OperationNameGenerator = new CustomNameGenerator()
            };
            settings.TypeScriptGeneratorSettings.PropertyNameGenerator = new MyPropertyNameGenerator(c => ConversionUtilities.ConvertToLowerCamelCase(c, true));
            settings.TypeScriptGeneratorSettings.NullValue = NJsonSchema.CodeGeneration.TypeScript.TypeScriptNullValue.Null;


            var generator = new SwaggerToTypeScriptClientGenerator(document, settings);
            var ts = generator.GenerateFile();
            ts = "namespace " + Namespace + " {\n" + ConversionUtilities.Tab(ts, 1).TrimEnd('\n') + "\n}\n";
            Context.ReturnFile(Encoding.UTF8.GetBytes(ts), "ApiClient.ts", "text/plain");
            //File.WriteAllText(TSPath, generator.GenerateFile());
        }

        public async Task GenerateSwagger()
        {
            var settings = new WebApiToSwaggerGeneratorSettings();
            var generator = new WebApiToSwaggerGenerator(settings);

            var controllers = typeof(GeneratorViewModel)
                .Assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t));
            var d = await generator.GenerateForControllersAsync(controllers);
            Context.ReturnFile(Encoding.UTF8.GetBytes(d.ToJson()), "WebApi.swagger.json", "text/json");
        }

        private async Task<SwaggerDocument> GetSwaggerDocument()
        {
            // Workaround: NSwag semm to have a bug in enum handling
            void editEnumType(JsonSchema4 type)
            {
                if (type.IsEnumeration && type.Type == JsonObjectType.None)
                    type.Type = JsonObjectType.Integer;
                foreach (var t in type.Properties.Values)
                    editEnumType(t);
            }

            // var d = await SwaggerDocument.FromFileAsync("c:/users/exyi/Downloads/github-swagger.json");
           


            var settings = new WebApiToSwaggerGeneratorSettings();
            var generator = new WebApiToSwaggerGenerator(settings);

            var controllers = typeof(GeneratorViewModel)
                .Assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t));
            var d = await generator.GenerateForControllersAsync(controllers);

            this.PopulateOperationIds(d);
            foreach (var t in d.Definitions.Values)
                editEnumType(t);
            return d;
        }

        private void PopulateOperationIds(SwaggerDocument d)
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

        private string GetOperationNameFromPath(SwaggerOperationDescription operation)
        {
            var pathSegments = operation.Path.Trim('/').Split('/').Where(s => !s.Contains('{')).ToArray();
            var lastPathSegment = pathSegments.LastOrDefault();
            var path = string.Concat(pathSegments.Take(pathSegments.Length - 1).Select(s => s + "_"));
            return path + operation.Method.ToString()[0].ToString().ToUpperInvariant() + operation.Method.ToString().Substring(1).ToLowerInvariant() + ConversionUtilities.ConvertToUpperCamelCase(lastPathSegment.Replace('_', '-'), false);
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
                // crazy property name, encode it in hex
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

