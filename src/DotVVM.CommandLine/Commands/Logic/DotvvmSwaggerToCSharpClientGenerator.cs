using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp.Models;

namespace DotVVM.CommandLine.Commands.Logic
{
    public class DotvvmSwaggerToCSharpClientGenerator : SwaggerToCSharpClientGenerator
    {
        private readonly SwaggerToCSharpTypeResolver resolver;

        public DotvvmSwaggerToCSharpClientGenerator(SwaggerDocument document, SwaggerToCSharpClientGeneratorSettings settings, SwaggerToCSharpTypeResolver resolver) : base(document, settings, resolver)
        {
            this.resolver = resolver;
        }

        protected override CSharpOperationModel CreateOperationModel(SwaggerOperation operation, ClientGeneratorBaseSettings settings)
        {
            var model = new DotvvmCSharpOperationModel(operation, Settings, this, resolver);
            HandleAsObjectParameters(operation, model, settings);
            return model;
        }

        private void HandleAsObjectParameters(SwaggerOperation operation, DotvvmCSharpOperationModel model, ClientGeneratorBaseSettings settings)
        {
            // find groups of parameters that should be treated as one
            var parameters = model.QueryParameters.Where(p => p.Name.Contains(".") && p.Schema.ExtensionData.ContainsKey("x-dotvvm-wrapperType"));
            var groups = parameters.GroupBy(p => p.Name.Substring(0, p.Name.IndexOf("."))).ToList();
            foreach (var g in groups)
            {
                var typeName = g.First().Schema.ExtensionData["x-dotvvm-wrapperType"].ToString();
                typeName = typeName.Substring(0, typeName.IndexOf(","));

                var swaggerParameter = new SwaggerParameter()
                { 
                    Name = g.Key,
                    Schema = new JsonSchema4(),
                    Kind = SwaggerParameterKind.Query
                };
                var newParameter = new DotvvmCSharpParameterModel(g.Key, g.Key, typeName, swaggerParameter, operation.Parameters, settings.CodeGeneratorSettings, this, model)
                {
                    ExcludeFromQuery = true
                };
                var targetIndex = g.Min(p => model.Parameters.IndexOf(p));
                foreach (var p in g)
                {
                    ((DotvvmCSharpParameterModel)p).CustomInitializer = $"var {p.VariableName} = {g.Key} != null ? {p.Name} : default({p.Type});";
                }
                model.Parameters.Insert(targetIndex, newParameter);
            }
        }
    }
}
