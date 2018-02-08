using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using DotVVM.Core;
using DotVVM.Core.Common;
using DotVVM.Framework.Api.Swashbuckle.Attributes;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin.Filters
{
    public class AddAsObjectAnnotationOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var parameters = apiDescription.ParameterDescriptions
                .Select(d => new {
                    Parameter = d,
                    AsObjectAttribute = d.ParameterDescriptor.GetCustomAttributes<AsObjectAttribute>().FirstOrDefault()
                })
                .Where(d => d.AsObjectAttribute != null);

            foreach (var param in parameters)
            {
                // add full type name to the metadata
                foreach (var jsonParam in operation.parameters.Where(p => p.name.StartsWith(param.Parameter.Name + ".")))
                {
                    var parameterType = param.AsObjectAttribute.ClientType ?? param.Parameter.ParameterDescriptor.ParameterType;

                    // the vendorExtensions dictionary instance is reused, create a new one
                    var dict = jsonParam.vendorExtensions.ToDictionary(e => e.Key, e => e.Value);
                    dict.Add(ApiConstants.DotvvmWrapperTypeKey, parameterType.FullName + ", " + parameterType.Assembly.GetName().Name);
                    jsonParam.vendorExtensions = dict;

                    // fix casing in the second part of the name
                    var propertyName = FindPropertyName(parameterType, jsonParam.name.Substring(jsonParam.name.IndexOf(".") + 1));
                    jsonParam.name = param.Parameter.Name + "." + propertyName;
                }
            }
        }

        private string FindPropertyName(Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            return property?.Name ?? propertyName;
        }
    }
}
