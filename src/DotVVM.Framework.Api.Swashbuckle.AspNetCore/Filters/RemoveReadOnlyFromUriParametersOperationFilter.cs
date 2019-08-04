using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class RemoveReadOnlyFromUriParametersOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters != null)
            {
                foreach (var param in operation.Parameters.ToList())
                {
                    var description = context.ApiDescription.ParameterDescriptions.SingleOrDefault(p => p.Name == param.Name);
                    if (description?.ModelMetadata.IsReadOnly == true)
                    {
                        operation.Parameters.Remove(param);
                    }
                }
            }
        }
    }
}
