using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore.Swagger
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
