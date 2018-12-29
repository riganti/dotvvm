using System;
using System.Collections.Generic;
using System.Linq;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp.Models;

namespace DotVVM.CommandLine.Commands.Logic
{
    public class DotvvmCSharpOperationModel : CSharpOperationModel
    {
        public IEnumerable<DotvvmCSharpParameterModel> MethodParameters => Parameters.OfType<DotvvmCSharpParameterModel>().Where(p => string.IsNullOrEmpty(p.CustomInitializer));

        public IEnumerable<DotvvmCSharpParameterModel> CustomInitializedParameters => Parameters.OfType<DotvvmCSharpParameterModel>().Where(p => !string.IsNullOrEmpty(p.CustomInitializer));

        public IEnumerable<DotvvmCSharpParameterModel> ActualQueryParameters => this.QueryParameters.OfType<DotvvmCSharpParameterModel>().Where(p => !p.ExcludeFromQuery);

        public DotvvmCSharpOperationModel(SwaggerOperation operation, SwaggerToCSharpGeneratorSettings settings, SwaggerToCSharpGeneratorBase generator, SwaggerToCSharpTypeResolver resolver) : base(operation, settings, generator, resolver)
        {
            RewriteParameters(operation, settings, generator);
        }

        private void RewriteParameters(SwaggerOperation operation, SwaggerToCSharpGeneratorSettings settings, SwaggerToCSharpGeneratorBase generator)
        {
            var parameters = operation.ActualParameters.ToList();
            if (settings.GenerateOptionalParameters)
                parameters = parameters.OrderBy(p => !p.IsRequired).ToList();

            var newParameters = parameters.Select(parameter =>
                new DotvvmCSharpParameterModel(parameter.Name, GetParameterVariableName(parameter, operation.Parameters),
                    ResolveParameterType(parameter), parameter, parameters,
                    settings.CodeGeneratorSettings,
                    generator,
                    this)
            );
            Parameters = new List<CSharpParameterModel>(newParameters);
        }
    }
}
