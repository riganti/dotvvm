using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class HandleGridViewDataSetReturnType : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                if (descriptor.MethodInfo.ReturnType?.IsGenericType == true
                    && descriptor.MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(GridViewDataSet<>))
                {
                    operation.Extensions.Add("x-dotvvm-returnsDataSet", "true");
                }
            }
        }
    }
}
