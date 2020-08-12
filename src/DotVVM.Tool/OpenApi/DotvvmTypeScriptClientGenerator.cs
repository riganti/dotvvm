using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Core.Common;
using NJsonSchema;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.TypeScript;
using NSwag.CodeGeneration.TypeScript.Models;

namespace DotVVM.Tool.OpenApi
{
    public class DotvvmTypeScriptClientGenerator : TypeScriptClientGenerator
    {
        private readonly TypeScriptTypeResolver resolver;

        public DotvvmTypeScriptClientGenerator(
            OpenApiDocument document,
            TypeScriptClientGeneratorSettings settings,
            TypeScriptTypeResolver resolver) : base(document, settings, resolver)
        {
            this.resolver = resolver;
        }

        protected override TypeScriptOperationModel CreateOperationModel(
            OpenApiOperation operation,
            ClientGeneratorBaseSettings settings)
        {
            var model = new DotvvmTypeScriptOperationModel(operation, Settings, this, resolver);
            HandleAsObjectParameters(operation, model, (TypeScriptClientGeneratorSettings)settings);
            return model;
        }

        private void HandleAsObjectParameters(
            OpenApiOperation operation,
            DotvvmTypeScriptOperationModel model,
            TypeScriptClientGeneratorSettings settings)
        {
            // find groups of parameters that should be treated as one
            var parameters = model.QueryParameters.Where(p => p.Name.Contains('.') && p.Schema.ExtensionData.ContainsKey(ApiConstants.DotvvmWrapperTypeKey));
            var groups = parameters.GroupBy(p => p.Name.Substring(0, p.Name.IndexOf('.'))).ToList();
            foreach (var group in groups)
            {
                var swaggerParameter = new OpenApiParameter() {
                    Name = group.Key,
                    Schema = new JsonSchema(),
                    Kind = OpenApiParameterKind.Query
                };
                var newParameter = new DotvvmTypeScriptParameterModel(
                    group.Key,
                    group.Key,
                    "any",
                    swaggerParameter,
                    operation.Parameters,
                    settings,
                    this,
                    resolver,
                    model)
                {
                    ExcludeFromQuery = true
                };
                var targetIndex = group.Min(p => model.Parameters.IndexOf(p));
                foreach (var p in group)
                {
                    ((DotvvmTypeScriptParameterModel)p).CustomInitializer = $"let {p.VariableName} = ({group.Key} !== null && typeof {group.Key} === 'object') ? {p.Name} : null;";
                }
                model.Parameters.Insert(targetIndex, newParameter);
            }
        }
    }
}
