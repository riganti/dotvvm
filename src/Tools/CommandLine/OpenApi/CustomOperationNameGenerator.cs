using System;
using System.Linq;
using NJsonSchema;
using NSwag;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace DotVVM.CommandLine.OpenApi
{
    public class CustomOperationNameGenerator : IOperationNameGenerator
    {
        public bool SupportsMultipleClients { get; } = true;

        public string GetClientName(
            OpenApiDocument document,
            string path,
            string httpMethod,
            OpenApiOperation operation)
        {
            return GetClientName(operation);
        }

        public string GetOperationName(
            OpenApiDocument document,
            string path,
            string httpMethod,
            OpenApiOperation operation)
        {
            var clientName = GetClientName(operation);
            var operationName = GetOperationName(operation);

            var hasOperationWithSameName = document.Operations
                .Where(o => o.Operation != operation)
                .Any(o => GetClientName(o.Operation) == clientName && GetOperationName(o.Operation) == operationName);

            if (hasOperationWithSameName)
            {
                if (operationName.StartsWith("get", StringComparison.InvariantCultureIgnoreCase))
                {
                    var isArrayResponse = operation.Responses.ContainsKey("200")
                        && operation.Responses["200"].ActualResponse.Schema != null
                        && operation.Responses["200"].ActualResponse.Schema.Type.HasFlag(JsonObjectType.Array);

                    if (isArrayResponse)
                        return "GetAll" + operationName.Substring(3);
                }
            }

            return operationName;
        }

        private string GetClientName(OpenApiOperation operation)
        {
            var segments = operation.OperationId.Split('_').ToArray();
            return segments.Length >= 2 ? segments[0] : string.Empty;
        }

        private string GetOperationName(OpenApiOperation operation)
        {
            var segments = operation.OperationId.Split('_').ToArray();
            if (segments.Length >= 2) segments = segments.Skip(1).ToArray();
            return segments.Length > 0 ? ConversionUtilities.ConvertToUpperCamelCase(string.Join("-", segments), true) : "Index";
        }
    }
}
