using System.Linq;
using DotVVM.Cli;
using NJsonSchema;
using NSwag;

namespace DotVVM.CommandLine.OpenApi
{
    internal static class ApiClientUtils
    {
        public static string CreateBaseClass(this ApiClientDefinition definition)
        {
            return $@"class ClientBase {{
    public transformOptions(options: RequestInit) {{
        options.credentials = ""{definition.FetchOptions.Credentials}"";
        return Promise.resolve(options);
    }}
}}";
        }

        public static string WrapInNamespace(this ApiClientDefinition definition, string typescript, string baseClass)
        {
            return "namespace " + definition.Namespace + " {\n    " + ConversionUtilities.Tab(baseClass, 1).TrimEnd('\n') + "\n    " + ConversionUtilities.Tab(typescript, 1).TrimEnd('\n') + "\n}\n";
        }

        public static string InjectWrapperClass(string csharpCode, string className, string[] clientNames)
        {
            var properties = from c in clientNames
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

        public static void PopulateOperationIds(this OpenApiDocument document)
        {
            // Generate missing IDs
            foreach (var operation in document.Operations.Where(o => string.IsNullOrEmpty(o.Operation.OperationId)))
                operation.Operation.OperationId = GetOperationNameFromPath(operation);

            void consolidateGroup(string name, OpenApiOperationDescription[] operations)
            {
                if (operations.Count() == 1) return;

                // Append "All" if possible
                if (!name.EndsWith("All") && !document.Operations.Any(n => n.Operation.OperationId == name + "All"))
                {
                    var arrayResponseOperations = operations.Where(
                        a => a.Operation.Responses
                            .Any(r => HttpUtilities.IsSuccessStatusCode(r.Key)
                                && r.Value.ActualResponse.Schema != null
                                && r.Value.ActualResponse.Schema.Type == JsonObjectType.Array))
                        .ToArray();

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
                    while (document.Operations.Any(o => o.Operation.OperationId == name + i)) i++;
                    operation.Operation.OperationId = name + i++;
                }
            }

            // Find non-unique operation IDs
            foreach (var group in document.Operations.GroupBy(o => o.Operation.OperationId))
            {
                var operations = group.ToList();
                consolidateGroup(group.Key, group.ToArray());
            }
        }

        private static string GetOperationNameFromPath(OpenApiOperationDescription operation)
        {
            var pathSegments = operation.Path.Trim('/').Split('/').Where(s => !s.Contains('{')).ToArray();
            var lastPathSegment = pathSegments.LastOrDefault();
            var path = string.Concat(pathSegments.Take(pathSegments.Length - 1).Select(s => s + "_"));
            return path + operation.Method.ToString()[0].ToString().ToUpper() + operation.Method.ToString().Substring(1).ToLower() + ConversionUtilities.ConvertToUpperCamelCase(lastPathSegment.Replace('_', '-'), false);
        }
    }
}
