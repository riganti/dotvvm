using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Core.Common;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp.Models;

namespace DotVVM.CommandLine.OpenApi
{
    public class DotvvmCSharpClientGenerator : CSharpClientGenerator
    {
        private readonly OpenApiDocument document;
        private readonly CSharpTypeResolver resolver;

        public DotvvmCSharpClientGenerator(
            OpenApiDocument document,
            CSharpClientGeneratorSettings settings,
            CSharpTypeResolver resolver)
            : base(document, settings, resolver)
        {
            this.document = document;
            this.resolver = resolver;
        }

        protected override CSharpOperationModel CreateOperationModel(
            OpenApiOperation operation,
            ClientGeneratorBaseSettings settings)
        {
            var model = new DotvvmCSharpOperationModel(operation, Settings, this, resolver);
            HandleAsObjectParameters(operation, model, settings);

            model.AutoRefreshKey = string.Join("/", document.Operations.Single(o => o.Operation.OperationId == operation.OperationId).Operation.Tags);

            return model;
        }

        private void HandleAsObjectParameters(
            OpenApiOperation operation,
            DotvvmCSharpOperationModel model,
            ClientGeneratorBaseSettings settings)
        {
            // find groups of parameters that should be treated as one
            var parameters = model.QueryParameters.Where(p => p.Name.Contains('.')
                && p.Schema.ExtensionData.ContainsKey(ApiConstants.DotvvmWrapperTypeKey));
            var groups = parameters
                .GroupBy(p => p.Name.Substring(0, p.Name.IndexOf('.')))
                .ToList();

            foreach (var group in groups)
            {
                var typeNameWithAssembly = group.First()
                    .Schema.ExtensionData[ApiConstants.DotvvmWrapperTypeKey].ToString();
                var typeName = typeNameWithAssembly!.Substring(0, typeNameWithAssembly.IndexOf(','));

                var swaggerParameter = new OpenApiParameter()
                {
                    Name = group.Key,
                    Schema = new JsonSchema(),
                    Kind = OpenApiParameterKind.Query
                };
                var newParameter = new DotvvmCSharpParameterModel(
                    group.Key,
                    group.Key,
                    typeName,
                    swaggerParameter,
                    operation.Parameters,
                    settings.CodeGeneratorSettings,
                    this,
                    resolver,
                    model)
                {
                    ExcludeFromQuery = true
                };

                foreach (var parameter in group)
                {
                    ((DotvvmCSharpParameterModel)parameter).CustomInitializer =
                        $"var {parameter.VariableName} = {group.Key} != null ? {parameter.Name} : default({parameter.Type});";
                }

                var targetIndex = group.Min(p => model.Parameters.IndexOf(p));
                model.Parameters.Insert(targetIndex, newParameter);
            }
        }


    }
}
