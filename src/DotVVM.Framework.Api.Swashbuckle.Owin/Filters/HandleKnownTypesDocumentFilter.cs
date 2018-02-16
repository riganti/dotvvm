using System;
using System.Linq;
using System.Web.Http.Description;
using DotVVM.Core.Common;
using Microsoft.Extensions.Options;
using Swashbuckle.Swagger;

namespace DotVVM.Framework.Api.Swashbuckle.Owin.Filters
{
    public class HandleKnownTypesDocumentFilter : IDocumentFilter
    {
        private readonly DotvvmApiOptions apiOptions;

        public HandleKnownTypesDocumentFilter(DotvvmApiOptions apiOptions)
        {
            this.apiOptions = apiOptions;
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var definition in swaggerDoc.definitions)
            {
                if (definition.Value.vendorExtensions.TryGetValue(ApiConstants.DotvvmTypeKey, out var objType) && objType is Type underlayingType
                    && apiOptions.IsKnownType(underlayingType))
                {
                    var name = CreateProperName(underlayingType, swaggerDoc);
                    definition.Value.vendorExtensions.Add(ApiConstants.DotvvmKnownTypeKey, name);
                }
            }

            foreach (var definition in swaggerDoc.definitions)
            {
                definition.Value.vendorExtensions.Remove(ApiConstants.DotvvmTypeKey);
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
