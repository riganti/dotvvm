using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema.CodeGeneration;
using NSwag;
using NSwag.CodeGeneration.TypeScript;
using NSwag.CodeGeneration.TypeScript.Models;

namespace DotVVM.Tool.OpenApi
{
    public class DotvvmTypeScriptOperationModel : TypeScriptOperationModel
    {
        public IEnumerable<DotvvmTypeScriptParameterModel> MethodParameters => Parameters.OfType<DotvvmTypeScriptParameterModel>().Where(p => string.IsNullOrEmpty(p.CustomInitializer));

        public IEnumerable<DotvvmTypeScriptParameterModel> CustomInitializedParameters => Parameters.OfType<DotvvmTypeScriptParameterModel>().Where(p => !string.IsNullOrEmpty(p.CustomInitializer));

        public IEnumerable<DotvvmTypeScriptParameterModel> ActualQueryParameters => this.QueryParameters.OfType<DotvvmTypeScriptParameterModel>().Where(p => !p.ExcludeFromQuery);

        public DotvvmTypeScriptOperationModel(
            OpenApiOperation operation,
            TypeScriptClientGeneratorSettings settings,
            TypeScriptClientGenerator generator,
            TypeResolverBase resolver) : base(operation, settings, generator, resolver)
        {
            var parameters = operation.ActualParameters.ToList();
            if (settings.GenerateOptionalParameters)
            {
                parameters = parameters.OrderBy(p => !p.IsRequired).ToList();
            }

            var newParameters = parameters.Select(parameter =>
                new DotvvmTypeScriptParameterModel(parameter.Name, GetParameterVariableName(parameter, operation.Parameters),
                    ResolveParameterType(parameter), parameter, parameters,
                    settings,
                    generator,
                    resolver,
                    this)
            );
            Parameters = new List<TypeScriptParameterModel>(newParameters);
        }
    }
}
