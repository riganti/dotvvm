using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp.Models;

namespace DotVVM.Tool.OpenApi
{
    public class DotvvmCSharpOperationModel : CSharpOperationModel
    {
        public IEnumerable<DotvvmCSharpParameterModel> MethodParameters => Parameters.OfType<DotvvmCSharpParameterModel>().Where(p => string.IsNullOrEmpty(p.CustomInitializer));

        public IEnumerable<DotvvmCSharpParameterModel> CustomInitializedParameters => Parameters.OfType<DotvvmCSharpParameterModel>().Where(p => !string.IsNullOrEmpty(p.CustomInitializer));

        public IEnumerable<DotvvmCSharpParameterModel> ActualQueryParameters => this.QueryParameters.OfType<DotvvmCSharpParameterModel>().Where(p => !p.ExcludeFromQuery);

        public string AutoRefreshKey { get; set; }

        public DotvvmCSharpOperationModel(
            OpenApiOperation operation,
            CSharpGeneratorBaseSettings settings,
            CSharpGeneratorBase generator,
            CSharpTypeResolver resolver) : base(operation, settings, generator, resolver)
        {
            RewriteParameters(operation, settings, generator, resolver);
        }

        private void RewriteParameters(
            OpenApiOperation operation,
            CSharpGeneratorBaseSettings settings,
            CSharpGeneratorBase generator,
            CSharpTypeResolver resolver)
        {
            var parameters = operation.ActualParameters.ToList();
            if (settings.GenerateOptionalParameters)
                parameters = parameters.OrderBy(p => !p.IsRequired).ToList();

            var newParameters = parameters.Select(parameter => new DotvvmCSharpParameterModel(
                    parameter.Name,
                    GetParameterVariableName(parameter, operation.Parameters),
                    ResolveParameterType(parameter), parameter, parameters,
                    settings.CodeGeneratorSettings,
                    generator,
                    resolver,
                    this)
            );
            Parameters = new List<CSharpParameterModel>(newParameters);
        }
    }
}
