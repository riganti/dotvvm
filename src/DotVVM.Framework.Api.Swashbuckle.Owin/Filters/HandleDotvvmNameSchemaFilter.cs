using System;
using System.Reflection;
using DotVVM.Core.Common;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin.Filters
{
    public class HandleDotvvmNameSchemaFilter : ISchemaFilter
    {
        private readonly DotvvmApiOptions apiOptions;
        private readonly IPropertySerialization propertySerialization;

        public HandleDotvvmNameSchemaFilter(DotvvmApiOptions apiOptions, IPropertySerialization propertySerialization)
        {
            this.apiOptions = apiOptions;
            this.propertySerialization = propertySerialization;
        }

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema.type == "object")
            {
                schema.vendorExtensions.Add(ApiConstants.DotvvmTypeKey, type);

                if (schema.properties != null)
                {
                    foreach (var property in schema.properties)
                    {
                        SetDotvvmNameToProperty(type, property.Key, property.Value);
                    }
                }
            }
        }

        private void SetDotvvmNameToProperty(Type type, string propertyName, Schema targetSchema)
        {
            var propertyInfo = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            if (propertyInfo != null)
            {
                targetSchema.vendorExtensions.Add(ApiConstants.DotvvmNameKey, propertySerialization.ResolveName(propertyInfo));
            }
        }
    }
}
