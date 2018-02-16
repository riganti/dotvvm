using System;
using System.Reflection;
using DotVVM.Core.Common;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class HandleDotvvmNameSchemaFilter : ISchemaFilter
    {
        private readonly IPropertySerialization propertySerialization;
        private readonly IOptions<DotvvmApiOptions> apiOptions;

        public HandleDotvvmNameSchemaFilter(IOptions<DotvvmApiOptions> apiOptions)
        {
            this.propertySerialization = new DefaultPropertySerialization();
            this.apiOptions = apiOptions;
        }

        public void Apply(Schema model, SchemaFilterContext context)
        {
            if (model.Type == "object")
            {
                model.Extensions.Add(ApiConstants.DotvvmTypeKey, context.SystemType);

                if (model.Properties != null)
                {
                    foreach (var property in model.Properties)
                    {
                        SetDotvvmNameToProperty(context.SystemType, property.Key, property.Value);
                    }
                }
            }
        }

        private void SetDotvvmNameToProperty(Type type, string propertyName, Schema targetSchema)
        {
            var propertyInfo = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            if (propertyInfo != null)
            {
                targetSchema.Extensions.Add(ApiConstants.DotvvmNameKey, propertySerialization.ResolveName(propertyInfo));
            }
        }
    }
}
