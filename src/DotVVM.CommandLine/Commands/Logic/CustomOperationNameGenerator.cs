using System;
using System.Linq;
using NJsonSchema;
using NSwag;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace DotVVM.CommandLine.Commands.Logic
{
    /// <summary>Generates multiple clients and operation names based on the Swagger operation ID (underscore separated).</summary>
    public class CustomOperationNameGenerator : IOperationNameGenerator
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
                if (operationName.StartsWith("get", StringComparison.InvariantCultureIgnoreCase))
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
