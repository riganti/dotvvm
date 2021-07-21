using System;
using System.Linq;
using System.Reflection;
using System.Web.Http.Description;
using DotVVM.Core.Common;
using DotVVM.Framework.ViewModel;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin.Filters
{
    public class HandleKnownTypesDocumentFilter : IDocumentFilter
    {
        private readonly DotvvmApiOptions apiOptions;
        private readonly IPropertySerialization propertySerialization;

        public HandleKnownTypesDocumentFilter(DotvvmApiOptions apiOptions, IPropertySerialization propertySerialization)
        {
            this.apiOptions = apiOptions;
            this.propertySerialization = propertySerialization;
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var schema in swaggerDoc.definitions.Values)
            {
                if (schema.vendorExtensions.TryGetValue(ApiConstants.DotvvmTypeKey, out var objType) && objType is Type underlyingType)
                {
                    if (apiOptions.IsKnownType(underlyingType))
                    {
                        var name = CreateProperName(underlyingType, swaggerDoc);
                        schema.vendorExtensions.Add(ApiConstants.DotvvmKnownTypeKey, name);

                        SetDotvvmNameToProperties(schema, underlyingType);
                    }
                }
            }

            foreach (var definition in swaggerDoc.definitions)
            {
                definition.Value.vendorExtensions.Remove(ApiConstants.DotvvmTypeKey);
            }
        }

        private void SetDotvvmNameToProperties(Schema schema, Type underlyingType)
        {
            if (schema.properties == null)
            {
                return;
            }

            foreach (var property in schema.properties)
            {
                SetDotvvmNameToProperty(underlyingType, property.Key, property.Value);
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

        public string CreateProperName(Type type, SwaggerDocument swaggerDoc)
        {
            if (type.GetGenericArguments().Length == 0)
            {
                return CreateNameWithNamespace(type);
            }

            var genericArguments = type.GetGenericArguments().Select(t => CreateNameForGenericParameter(t, swaggerDoc));
            var unmangledName = GetNameWithoutGenericArity(type);

            return type.Namespace + '.' + unmangledName + '<' + string.Join(",", genericArguments) + '>';
        }

        public string CreateNameForGenericParameter(Type type, SwaggerDocument swaggerDoc)
        {
            var definition = swaggerDoc.definitions
                .Where(d => d.Value.vendorExtensions.TryGetValue(ApiConstants.DotvvmTypeKey, out var objType) && (Type)objType == type)
                .FirstOrDefault();

            return definition.Key ?? type.FullName;
        }

        public static string GetNameWithoutGenericArity(Type type) => type.Name.Substring(0, type.Name.IndexOf('`'));

        private static string CreateNameWithNamespace(Type type) => type.Namespace + '.' + type.Name;
    }
}
