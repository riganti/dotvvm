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
    public class HandleKnownTypesSchemaFilter : ISchemaFilter
    {
        private readonly IOptions<DotvvmApiKnownTypesOptions> knownTypesOptions;

        public HandleKnownTypesSchemaFilter(IOptions<DotvvmApiKnownTypesOptions> knownTypesOptions)
        {
            this.knownTypesOptions = knownTypesOptions;
        }

        public void Apply(Schema model, SchemaFilterContext context)
        {
            if (model.Type == "object")
            {
                model.Extensions.Add(ApiConstants.DotvvmTypeKey, context.SystemType);

                if (model.Properties != null && knownTypesOptions.Value.IsKnownType(context.SystemType))
                {
                    foreach (var property in model.Properties)
                    {
                        SetDotvvmNameToProperty(context.SystemType, property.Key, property.Value);
                    }
                }
            }
        }

        private static void SetDotvvmNameToProperty(Type type, string propertyName, Schema targetSchema)
        {
            var propertyInfo = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            if (propertyInfo != null)
            {
                targetSchema.Extensions.Add(ApiConstants.DotvvmNameKey, ResolvePropertyName(propertyInfo));
            }
        }

        // TODO: share implementation with DotVVM.Framework
        private static string ResolvePropertyName(PropertyInfo property)
        {
            var bindAttribute = property.GetCustomAttribute<BindAttribute>();
            if (bindAttribute != null)
            {
                if (!string.IsNullOrEmpty(bindAttribute.Name))
                {
                    return bindAttribute.Name;
                }
            }

            if (string.IsNullOrEmpty(bindAttribute?.Name))
            {
                // use JsonProperty name if Bind attribute is not present or doesn't specify it
                var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                if (!string.IsNullOrEmpty(jsonPropertyAttribute?.PropertyName))
                {
                    return jsonPropertyAttribute.PropertyName;
                }
            }

            return property.Name;
        }
    }
}
