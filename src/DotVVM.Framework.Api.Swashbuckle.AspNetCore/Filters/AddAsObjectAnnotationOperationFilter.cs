using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Api.Swashbuckle.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class AddAsObjectAnnotationOperationFilter : IOperationFilter 
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
                    var attribute = ((ControllerParameterDescriptor)group.First().ParameterDescriptor)
                        .ParameterInfo
                        .GetCustomAttribute<AsObjectAttribute>();
                    if (attribute == null)
                    {
                        continue;
                    }

                    // add group name in the metadata
                    foreach (var param in group)
                    {
                        var jsonParam = operation.Parameters.SingleOrDefault(p => p.Name == param.Name);
                        if (jsonParam != null)
                        {
                            var parameterType = attribute.ClientType ?? param.ParameterDescriptor.ParameterType;

                            jsonParam.Name = group.First().ParameterDescriptor.Name + "." + jsonParam.Name;
                            jsonParam.Extensions.Add("x-dotvvm-wrapperType", parameterType.FullName + ", " + parameterType.Assembly.GetName().Name);
                        }
                    }
                }
            }
        }
    }
}
