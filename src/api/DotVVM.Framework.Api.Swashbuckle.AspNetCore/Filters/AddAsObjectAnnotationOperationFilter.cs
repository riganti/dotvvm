using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Core.Common;
using DotVVM.Framework.Api.Swashbuckle.Attributes;
using DotVVM.Framework.ViewModel;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class AddAsObjectOperationFilter : IOperationFilter
    {
        private readonly DotvvmApiOptions knownTypesOptions;
        private readonly DefaultPropertySerialization propertySerialization;

        public AddAsObjectOperationFilter(IOptions<DotvvmApiOptions> knownTypesOptions)
        {
            this.knownTypesOptions = knownTypesOptions.Value;
            this.propertySerialization = new DefaultPropertySerialization();
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor)
            {
                ApplyControllerAction(operation, context.ApiDescription);
            }
        }

        private void ApplyControllerAction(OpenApiOperation operation, ApiDescription apiDescription)
        {
            // all properties of objects with FromQuery parameters have the same ParameterDescriptor
            var groups = apiDescription.ParameterDescriptions.GroupBy(p => p.ParameterDescriptor);
            foreach (var group in groups.Where(p => p.Count() > 1))
            {
                var parameterDescriptor = (ControllerParameterDescriptor)group.First().ParameterDescriptor;

                // determine group name
                var attribute = parameterDescriptor
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

                        jsonParam.Name = parameterDescriptor.Name + '.' + GetPropertyName(parameterType, jsonParam.Name);
                        jsonParam.Extensions.Add(ApiConstants.DotvvmWrapperTypeKey, new OpenApiString(parameterType.FullName + ", " + parameterType.Assembly.GetName().Name));
                    }
                }
            }
        }

        private string GetPropertyName(Type type, string propertyName)
        {
            var propertyInfo = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            if (propertyInfo == null)
            {
                return propertyName;
            }

            return propertySerialization.ResolveName(propertyInfo);
        }
    }
}
