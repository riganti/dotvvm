using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema.CodeGeneration;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp.Models;

namespace DotVVM.CommandLine.Commands.Logic
{
    public class DotvvmCSharpParameterModel : CSharpParameterModel
    {
        private readonly DotvvmCSharpOperationModel operation;

        public string CustomInitializer { get; set; }

        public bool ExcludeFromQuery { get; set; }

        public bool IsLastMethodParameter => operation.MethodParameters.LastOrDefault() == this;
        
        public DotvvmCSharpParameterModel(string parameterName, string variableName, string typeName, SwaggerParameter parameter, IList<SwaggerParameter> allParameters, CodeGeneratorSettingsBase settings, IClientGenerator generator, DotvvmCSharpOperationModel operation) : base(parameterName, variableName, typeName, parameter, allParameters, settings, generator)
        {
            this.operation = operation;
        }
    }
}
