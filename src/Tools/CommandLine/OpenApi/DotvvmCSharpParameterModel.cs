using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema.CodeGeneration;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp.Models;

namespace DotVVM.CommandLine.OpenApi
{
    public class DotvvmCSharpParameterModel : CSharpParameterModel
    {
        private readonly DotvvmCSharpOperationModel operation;

        public string? CustomInitializer { get; set; }

        public bool ExcludeFromQuery { get; set; }

        public bool IsLastMethodParameter => operation.MethodParameters.LastOrDefault() == this;

        public DotvvmCSharpParameterModel(
            string parameterName,
            string variableName,
            string typeName,
            OpenApiParameter parameter,
            IList<OpenApiParameter> allParameters,
            CodeGeneratorSettingsBase settings,
            IClientGenerator generator,
            TypeResolverBase resolver,
            DotvvmCSharpOperationModel operation)
            : base(parameterName, variableName, variableName, typeName, parameter, allParameters, settings, generator, resolver)
        {
            this.operation = operation;
        }
    }
}
