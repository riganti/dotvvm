using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class RemoveBindNoneFromUriParametersOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters != null)
            {
                foreach (var param in operation.Parameters.ToList())
                {
                    var description = context.ApiDescription.ParameterDescriptions.SingleOrDefault(p => p.Name == param.Name);
                    var metadata = description?.ModelMetadata as DefaultModelMetadata;
                    var bindAttribute = metadata?.Attributes.PropertyAttributes?.OfType<BindAttribute>().FirstOrDefault();
                    if (bindAttribute != null && !bindAttribute.Direction.HasFlag(Direction.ClientToServer))
                    {
                        operation.Parameters.Remove(param);
                    }
                }
            }
        }
    }
}
