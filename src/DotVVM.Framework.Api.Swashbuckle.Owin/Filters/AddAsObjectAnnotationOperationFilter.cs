using System;
using System.Linq;
using System.Reflection;
using System.Web.Http.Description;
using DotVVM.Core.Common;
using DotVVM.Framework.Api.Swashbuckle.Attributes;
using DotVVM.Framework.ViewModel;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin.Filters
{
    public class AddAsObjectAnnotationOperationFilter : IOperationFilter
    {
        private readonly IPropertySerialization propertySerialization;

        public AddAsObjectAnnotationOperationFilter(IPropertySerialization propertySerialization)
        {
            this.propertySerialization = propertySerialization;
        }

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var parameters = apiDescription.ParameterDescriptions
                                 .Select(p => new {
                                     Value = p,
                                     Attribute = p.ParameterDescriptor
                                        .GetCustomAttributes<AsObjectAttribute>()
                                        .FirstOrDefault()
                                 })
                                 .Where(d => d.Attribute != null);

            var groups = apiDescription.ParameterDescriptions.GroupBy(p => p.ParameterDescriptor);

            foreach (var param in parameters)
            {
                // add full type name to the metadata
                foreach (var jsonParam in operation.parameters.Where(p => p.name.StartsWith(param.Value.Name + ".")))
                {
                    var parameterType = param.Attribute.ClientType ?? param.Value.ParameterDescriptor.ParameterType;

                    // the vendorExtensions dictionary instance is reused, create a new one
                    var dict = jsonParam.vendorExtensions.ToDictionary(e => e.Key, e => e.Value);
                    dict.Add(ApiConstants.DotvvmWrapperTypeKey, parameterType.FullName + ", " + parameterType.Assembly.GetName().Name);
                    jsonParam.vendorExtensions = dict;

                    // fix casing in the second part of the name
                    var propertyName = GetPropertyName(parameterType, jsonParam.name.Substring(jsonParam.name.IndexOf(".") + 1));
                    jsonParam.name = param.Value.Name + "." + propertyName;
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
