using System;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Exceptions
{
    [Serializable]
    public class NotSupportedOperationException : Exception
    {

        public NotSupportedOperationException(string filePath, FileLinePositionSpan linePositionSpan,
            OperationKind operationKind)
            : base($"Unsupported operation {operationKind} on line {linePositionSpan.StartLinePosition.Line} of file {filePath}")
        {

        }

        protected NotSupportedOperationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

    }
}
