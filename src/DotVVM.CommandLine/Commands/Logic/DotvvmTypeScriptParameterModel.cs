using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema.CodeGeneration;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.TypeScript;
using NSwag.CodeGeneration.TypeScript.Models;

namespace DotVVM.CommandLine.Commands.Logic
{
    public class DotvvmTypeScriptParameterModel : TypeScriptParameterModel
    {
        private readonly DotvvmTypeScriptOperationModel operation;

        public string CustomInitializer { get; set; }

        public bool ExcludeFromQuery { get; set; }

        public bool IsLastMethodParameter => operation.MethodParameters.LastOrDefault() == this;

        public DotvvmTypeScriptParameterModel(string parameterName, string variableName, string typeName, SwaggerParameter parameter, IList<SwaggerParameter> allParameters, SwaggerToTypeScriptClientGeneratorSettings settings, SwaggerToTypeScriptClientGenerator generator, DotvvmTypeScriptOperationModel operation) : base(parameterName, variableName, typeName, parameter, allParameters, settings, generator)
        {
            this.operation = operation;
        }
    }
}
