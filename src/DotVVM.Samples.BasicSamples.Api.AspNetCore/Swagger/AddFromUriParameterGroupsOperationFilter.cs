using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore.Swagger
{
    public class AddFromUriParameterGroupsOperationFilter : IOperationFilter 
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor)
            {
                // all properties of objects with FromQuery parameters have the same ParameterDescriptor
                var groups = context.ApiDescription.ParameterDescriptions.GroupBy(p => p.ParameterDescriptor);
                foreach (var group in groups.Where(p => p.Count() > 1))
                {
                    // determine group name
                    var name = ((ControllerParameterDescriptor)group.First().ParameterDescriptor).ParameterInfo
                        .GetCustomAttribute<FromQueryAttribute>()?.Name;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = group.First().ParameterDescriptor.Name;
                    }

                    // add group name in the metadata
                    foreach (var param in group)
                    {
                        var jsonParam = operation.Parameters.Single(p => p.Name == param.Name);
                        jsonParam.Extensions.Add("paramGroup", name);
                    }
                }
            }
        }
    }
}
